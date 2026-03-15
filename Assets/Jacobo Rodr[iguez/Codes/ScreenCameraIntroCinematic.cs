using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ScreenCameraIntroCinematic : MonoBehaviour
{
    private struct RoutePoint
    {
        public Vector3 position;
        public bool isMiddle;
        public int middleIndex;

        public RoutePoint(Vector3 position, bool isMiddle, int middleIndex)
        {
            this.position = position;
            this.isMiddle = isMiddle;
            this.middleIndex = middleIndex;
        }
    }

    private static readonly HashSet<string> CinematicPlayedScenes = new HashSet<string>();

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private ScreenManager screenManager;
    [SerializeField] private Transform player;

    [Header("Intro Camera Cinematic")]
    [SerializeField] private bool enableIntroCinematic = false;
    [SerializeField] private bool onlyWhenNoCheckpoint = true;
    [SerializeField] private float introCameraSpeed = 5f;
    [SerializeField] private Transform introPointA;
    [SerializeField] private Transform[] introExtraPoints;
    [SerializeField] private Transform introPointB;
    [SerializeField] private bool hidePlayerUntilCinematicEnds = true;

    [Header("Events")]
    public UnityEvent OnCameraStart;
    public UnityEvent OnCameraReachMiddlePoint;
    [SerializeField] private List<UnityEvent> OnCameraReachMiddlePointByIndex = new List<UnityEvent>();
    public UnityEvent OnCameraFinish;

    private Vector3 initialCameraStartPos;
    private bool shouldRunThisScene;
    private string activeSceneName;

    private PlayerMovement playerMovement;
    private bool restoreCanMove;
    private bool layerInputWasDisabledByCinematic;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private bool restorePlayerInput;
#endif

    private void Awake()
    {
        activeSceneName = SceneManager.GetActiveScene().name;

        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (screenManager == null) screenManager = GetComponent<ScreenManager>();

        if (cam != null)
            initialCameraStartPos = cam.transform.position;

        shouldRunThisScene = ShouldPlayIntroCinematic()
                             && !HasCinematicAlreadyPlayedForScene(activeSceneName)
                             && !CheckPointManager.TryGetCheckpointCameraSnapPosition(out _);
        if (!shouldRunThisScene || cam == null)
            return;

        // Posicionar cámara en A antes de Start para evitar glitch visual de 1 frame.
        Vector3 aPos = introPointA.position;
        aPos.z = cam.transform.position.z;
        cam.transform.position = aPos;
    }

    private void Start()
    {
        if (!shouldRunThisScene)
        {
            // Si la cinemática se omite (por respawn o porque ya se reprodujo),
            // asegurar que el ScreenManager quede operativo y sincronizado.
            if (screenManager != null)
            {
                screenManager.enabled = true;
                screenManager.SyncCameraStateToCurrentPosition();
            }

            return;
        }

        MarkCinematicPlayedForScene(activeSceneName);

        StartCoroutine(PlayIntroCinematic());
    }

    public static void ClearPlayedScenesCache()
    {
        CinematicPlayedScenes.Clear();
    }

    private static bool HasCinematicAlreadyPlayedForScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        return CinematicPlayedScenes.Contains(sceneName);
    }

    private static void MarkCinematicPlayedForScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        CinematicPlayedScenes.Add(sceneName);
    }

    private bool ShouldPlayIntroCinematic()
    {
        if (!enableIntroCinematic) return false;
        if (introPointA == null) return false;

        if (!onlyWhenNoCheckpoint) return true;
        return GameManager.Instance == null || !GameManager.Instance.hasCheckpoint;
    }

    private IEnumerator PlayIntroCinematic()
    {
        if (screenManager != null)
            screenManager.enabled = false;

        OnCameraStart?.Invoke();

        CachePlayerReference();
        DisablePlayerInputForCinematic();

        if (hidePlayerUntilCinematicEnds && player != null)
            player.gameObject.SetActive(false);

        List<RoutePoint> route = BuildIntroRoute();
        float speed = Mathf.Max(0.01f, introCameraSpeed);

        for (int i = 0; i < route.Count; i++)
        {
            RoutePoint routePoint = route[i];
            Vector3 target = routePoint.position;
            while (cam != null && Vector3.Distance(cam.transform.position, target) > 0.01f)
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            if (cam != null)
                cam.transform.position = target;

            if (routePoint.isMiddle)
                InvokeMiddlePointEvents(routePoint.middleIndex);
        }

        OnCameraFinish?.Invoke();

        if (hidePlayerUntilCinematicEnds && player != null)
            player.gameObject.SetActive(true);

        RestorePlayerInputAfterCinematic();

        if (screenManager != null)
            screenManager.SyncCameraStateToCurrentPosition();

        if (screenManager != null)
            screenManager.enabled = true;
    }

    private void CachePlayerReference()
    {
        if (player == null)
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) player = playerGo.transform;
        }

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
#if ENABLE_INPUT_SYSTEM
            playerInput = player.GetComponent<PlayerInput>();
#endif
        }
    }

    private void DisablePlayerInputForCinematic()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLayerInputEnabled(false);
            layerInputWasDisabledByCinematic = true;
        }

        if (playerMovement != null)
        {
            restoreCanMove = playerMovement.canMove;
            playerMovement.canMove = false;
        }

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
        {
            restorePlayerInput = playerInput.enabled;
            playerInput.enabled = false;
        }
#endif
    }

    private void RestorePlayerInputAfterCinematic()
    {
        if (layerInputWasDisabledByCinematic && GameManager.Instance != null)
        {
            GameManager.Instance.SetLayerInputEnabled(true);
            layerInputWasDisabledByCinematic = false;
        }

        if (playerMovement != null)
            playerMovement.canMove = restoreCanMove;

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
            playerInput.enabled = restorePlayerInput;
#endif
    }

    private void InvokeMiddlePointEvents(int middleIndex)
    {
        OnCameraReachMiddlePoint?.Invoke();

        if (middleIndex < 0)
            return;

        if (OnCameraReachMiddlePointByIndex == null)
            return;

        if (middleIndex >= OnCameraReachMiddlePointByIndex.Count)
            return;

        OnCameraReachMiddlePointByIndex[middleIndex]?.Invoke();
    }

    private List<RoutePoint> BuildIntroRoute()
    {
        List<RoutePoint> route = new List<RoutePoint>();
        if (cam == null)
            return route;

        Vector3 a = introPointA.position;
        a.z = cam.transform.position.z;
        route.Add(new RoutePoint(a, false, -1));

        if (introExtraPoints != null)
        {
            for (int i = 0; i < introExtraPoints.Length; i++)
            {
                if (introExtraPoints[i] == null) continue;
                Vector3 p = introExtraPoints[i].position;
                p.z = cam.transform.position.z;
                route.Add(new RoutePoint(p, true, i));
            }
        }

        Vector3 b = introPointB != null ? introPointB.position : initialCameraStartPos;
        b.z = cam.transform.position.z;
        route.Add(new RoutePoint(b, false, -1));

        return route;
    }

    public void SetCameraOrthographicSize(float size)
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (size <= 0f) return;

        cam.orthographic = true;
        cam.orthographicSize = size;
    }

    public void SetCameraSize1_8()
    {
        SetCameraOrthographicSize(1.8f);
    }

    public void SetCameraSize5()
    {
        SetCameraOrthographicSize(5f);
    }
}
