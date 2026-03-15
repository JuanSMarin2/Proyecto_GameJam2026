using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ScreenCameraIntroCinematic : MonoBehaviour
{
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

    private Vector3 initialCameraStartPos;
    private bool shouldRunThisScene;

    private PlayerMovement playerMovement;
    private bool restoreCanMove;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private bool restorePlayerInput;
#endif

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (screenManager == null) screenManager = GetComponent<ScreenManager>();

        if (cam != null)
            initialCameraStartPos = cam.transform.position;

        shouldRunThisScene = ShouldPlayIntroCinematic() && !CheckPointManager.TryGetCheckpointCameraSnapPosition(out _);
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
            return;

        StartCoroutine(PlayIntroCinematic());
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

        CachePlayerReference();
        DisablePlayerInputForCinematic();

        if (hidePlayerUntilCinematicEnds && player != null)
            player.gameObject.SetActive(false);

        List<Vector3> route = BuildIntroRoute();
        float speed = Mathf.Max(0.01f, introCameraSpeed);

        for (int i = 0; i < route.Count; i++)
        {
            Vector3 target = route[i];
            while (cam != null && Vector3.Distance(cam.transform.position, target) > 0.01f)
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, target, speed * Time.deltaTime);
                yield return null;
            }

            if (cam != null)
                cam.transform.position = target;
        }

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
        if (playerMovement != null)
            playerMovement.canMove = restoreCanMove;

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
            playerInput.enabled = restorePlayerInput;
#endif
    }

    private List<Vector3> BuildIntroRoute()
    {
        List<Vector3> route = new List<Vector3>();
        if (cam == null)
            return route;

        Vector3 a = introPointA.position;
        a.z = cam.transform.position.z;
        route.Add(a);

        if (introExtraPoints != null)
        {
            for (int i = 0; i < introExtraPoints.Length; i++)
            {
                if (introExtraPoints[i] == null) continue;
                Vector3 p = introExtraPoints[i].position;
                p.z = cam.transform.position.z;
                route.Add(p);
            }
        }

        Vector3 b = introPointB != null ? introPointB.position : initialCameraStartPos;
        b.z = cam.transform.position.z;
        route.Add(b);

        return route;
    }
}
