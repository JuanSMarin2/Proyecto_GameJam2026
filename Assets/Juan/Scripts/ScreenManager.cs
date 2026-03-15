using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Tile Camera View (Projection)")]
    [SerializeField] private bool enforceTileView = true;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private int visibleTilesX = 18;
    [SerializeField] private int visibleTilesY = 10;

    [Header("Camera Movement")]
    [SerializeField] private float cameraMoveSpeed = 8f;

    private Vector3 targetCameraPos;
    private bool isMovingCamera;

    private float screenWidth;
    private float screenHeight;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        ApplyTileProjectionIfNeeded();
        CalculateScreenSize();

        // Snap temprano (antes de Start) para evitar frame de glitch al cargar escena.
        if (CheckPointManager.TryGetCheckpointCameraSnapPosition(out Vector3 snapPosition))
        {
            snapPosition.z = cam != null ? cam.transform.position.z : snapPosition.z;

            if (cam != null)
                cam.transform.position = snapPosition;

            targetCameraPos = snapPosition;
            isMovingCamera = false;
        }
    }

    private void OnEnable()
    {
        CheckPointManager.OnPlayerRespawnedAtCheckpoint += HandlePlayerRespawnedAtCheckpoint;
    }

    private void OnDisable()
    {
        CheckPointManager.OnPlayerRespawnedAtCheckpoint -= HandlePlayerRespawnedAtCheckpoint;
    }

    private void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        ApplyTileProjectionIfNeeded();
        CalculateScreenSize();
        targetCameraPos = cam.transform.position;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void CalculateScreenSize()
    {
        if (cam == null) return;

        // Tamaño REAL visible de la cámara (en unidades de mundo)
        // Usamos pixelRect para que el cálculo sea correcto si hay letterbox/pillarbox.
        Rect pr = cam.pixelRect;
        float aspect = (pr.height > 0f) ? (pr.width / pr.height) : cam.aspect;

        screenHeight = cam.orthographicSize * 2f;
        screenWidth = screenHeight * aspect;
    }

    private void ApplyTileProjectionIfNeeded()
    {
        if (!enforceTileView || cam == null) return;
        if (visibleTilesX <= 0 || visibleTilesY <= 0 || tileSize <= 0f) return;

        // Forzamos cámara ortográfica y tamaño vertical exacto.
        cam.orthographic = true;
        cam.orthographicSize = (visibleTilesY * tileSize) * 0.5f;

        // Para que sea EXACTAMENTE visibleTilesX x visibleTilesY sin deformar,
        // necesitamos mantener el aspect ratio del viewport en visibleTilesX/visibleTilesY.
        float targetAspect = visibleTilesX / (float)visibleTilesY;
        float windowAspect = (Screen.height > 0) ? (Screen.width / (float)Screen.height) : targetAspect;

        if (windowAspect < targetAspect)
        {
            // Pantalla más "estrecha" que el target: letterbox (barras arriba/abajo)
            float rectHeight = windowAspect / targetAspect;
            float rectY = (1f - rectHeight) * 0.5f;
            cam.rect = new Rect(0f, rectY, 1f, rectHeight);
        }
        else
        {
            // Pantalla más "ancha" que el target: pillarbox (barras izquierda/derecha)
            float rectWidth = targetAspect / windowAspect;
            float rectX = (1f - rectWidth) * 0.5f;
            cam.rect = new Rect(rectX, 0f, rectWidth, 1f);
        }
    }

    private void Update()
    {
        if (cam == null) return;

        // Por si cambia resolución / aspecto
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyTileProjectionIfNeeded();
        }

        CalculateScreenSize();

        if (isMovingCamera)
        {
            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                targetCameraPos,
                Time.deltaTime * cameraMoveSpeed
            );

            if (Vector3.Distance(cam.transform.position, targetCameraPos) < 0.01f)
            {
                cam.transform.position = targetCameraPos;
                isMovingCamera = false;
            }
        }
        else
        {
            if (player != null)
                CheckPlayerExit();
        }
    }

    private void CheckPlayerExit()
    {
        Vector3 camPos = cam.transform.position;
        Vector3 playerPos = player.position;

        float halfW = screenWidth * 0.5f;
        float halfH = screenHeight * 0.5f;

        if (playerPos.y > camPos.y + halfH)
            MoveCamera(Vector2.up);
        else if (playerPos.y < camPos.y - halfH)
            MoveCamera(Vector2.down);
        else if (playerPos.x > camPos.x + halfW)
            MoveCamera(Vector2.right);
        else if (playerPos.x < camPos.x - halfW)
            MoveCamera(Vector2.left);
    }

    private void MoveCamera(Vector2 direction)
    {


        SoundManager.PlaySound(SoundType.CambioDePantalla);

        targetCameraPos += new Vector3(
            direction.x * screenWidth,
            direction.y * screenHeight,
            0f
        );

        isMovingCamera = true;
    }

    private void HandlePlayerRespawnedAtCheckpoint(Transform respawnedPlayer)
    {
        if (respawnedPlayer == null) return;

        // Si el player serializado no está asignado o cambió de instancia, actualizamos referencia.
        if (player == null || player != respawnedPlayer)
            player = respawnedPlayer;
    }

    public void SyncCameraStateToCurrentPosition()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        targetCameraPos = cam.transform.position;
        isMovingCamera = false;
    }

    private void OnValidate()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        if (!Application.isPlaying)
        {
            ApplyTileProjectionIfNeeded();
            CalculateScreenSize();
        }
    }
}
