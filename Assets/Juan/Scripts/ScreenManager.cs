using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Camera Movement")]
    [SerializeField] private float cameraMoveSpeed = 8f;

    private Vector3 targetCameraPos;
    private bool isMovingCamera;

    private float screenWidth;
    private float screenHeight;

    private void Start()
    {
        CalculateScreenSize();
        targetCameraPos = cam.transform.position;
    }

    private void CalculateScreenSize()
    {
        // Tamaño REAL visible de la cámara
        screenHeight = cam.orthographicSize * 2f;
        screenWidth = screenHeight * cam.aspect;
    }

    private void Update()
    {
        // Por si cambia resolución / aspecto
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
        targetCameraPos += new Vector3(
            direction.x * screenWidth,
            direction.y * screenHeight,
            0f
        );

        isMovingCamera = true;
    }
}
