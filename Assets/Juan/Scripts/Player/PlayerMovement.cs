using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tile Movement")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float wallCheckRadius = 0.1f;

    [Header("Layer Visuals")]
    [SerializeField] private GameObject capa1;
    [SerializeField] private GameObject capa2;
    [SerializeField] private GameObject capa3;
    [SerializeField] private GameObject capa4;

    [Header("Layer Mask Animation")]
    [SerializeField] private bool playMaskAnimationOnLayerChange = true;
    [SerializeField] private string maskTriggerName = "mask";
    [Tooltip("Tiempo a esperar antes de mostrar/ocultar la máscara. 0 = espera 1 frame.")]
    [SerializeField] private float maskVisualDelay = 0f;
    
    [Header("Postprocess Config")]
    [SerializeField] private postprocessConfig postprocessConfig;

    private Vector2 targetPosition;
    private bool isMoving;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private float currentMoveSpeed;

    private PlayerInputReader playerInputReader;
    private PlayerAnimation playerAnimation;
    private PlayerLayerVisuals playerLayerVisuals;
    private GridCollision gridCollision;


    public bool canMove = true;

    public void DisableMovementForDeath()
    {
        canMove = false;
        isMoving = false;
        targetPosition = rb != null ? rb.position : (Vector2)transform.position;
        currentMoveSpeed = moveSpeed;

        if (playerAnimation != null)
            playerAnimation.SetChildrenSpriteRenderersEnabled(false);
    }

    public void DisableMovementForWin()
    {
        canMove = false;
        isMoving = false;
        targetPosition = rb != null ? rb.position : (Vector2)transform.position;
        currentMoveSpeed = moveSpeed;
    }

    public void EnableMovement()
    {
        canMove = true;

        if (playerAnimation != null)
            playerAnimation.SetChildrenSpriteRenderersEnabled(true);

        if (playerLayerVisuals != null)
            playerLayerVisuals.ApplyLayerVisuals();
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerLayerVisuals = GetComponent<PlayerLayerVisuals>();
        gridCollision = GetComponent<GridCollision>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        targetPosition = rb.position;
        currentMoveSpeed = moveSpeed;

        if (playerAnimation != null)
            playerAnimation.SetMaskTriggerName(maskTriggerName);

        if (playerLayerVisuals != null)
        {
            playerLayerVisuals.Configure(
                capa1,
                capa2,
                capa3,
                capa4,
                postprocessConfig,
                playMaskAnimationOnLayerChange,
                maskVisualDelay,
                playerAnimation);
        }
    }

    private void Update()
    {
        if (!canMove || isMoving)
            return;

        if (playerInputReader == null)
            return;

        if (playerInputReader.ConsumeMove(out MoveDirection direction))
        {
            TryMove(direction);
        }
    }

    private void TryMove(MoveDirection direction)
    {
        Vector2 vectorDirection = GetDirectionVector(direction);
        if (vectorDirection == Vector2.zero)
            return;

        Vector2 nextPosition = rb.position + vectorDirection * tileSize;
        if (gridCollision != null && gridCollision.IsWallAtPosition(nextPosition))
            return;

        Box box = gridCollision != null ? gridCollision.GetBoxAtPosition(nextPosition) : null;
        if (box != null)
        {
            bool pushed = box.TryPush(vectorDirection, tileSize, moveSpeed * 0.5f);
            if (!pushed)
                return;
            currentMoveSpeed = moveSpeed * 0.5f;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }

        targetPosition = nextPosition;
        isMoving = true;

        if (playerAnimation != null)
            playerAnimation.TriggerMoveAnimation();

        if (playerAnimation != null)
        {
            if (direction == MoveDirection.Right) playerAnimation.ApplyFlipX(true);
            else if (direction == MoveDirection.Left) playerAnimation.ApplyFlipX(false);
        }
    }



    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, currentMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if ((targetPosition - newPosition).sqrMagnitude <= 0.0001f)
        {
            rb.MovePosition(targetPosition);
            isMoving = false;
            currentMoveSpeed = moveSpeed;
        }
    }

    private static Vector2 GetDirectionVector(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return Vector2.up;
            case MoveDirection.Down:
                return Vector2.down;
            case MoveDirection.Left:
                return Vector2.left;
            case MoveDirection.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }
}
