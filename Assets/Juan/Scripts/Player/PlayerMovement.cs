using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Tile Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float waterPushSpeed = 3.5f;

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
    private float currentMoveSpeed;

    private GridObject gridObject;
    private GridManager gridManager;
    private PlayerInputReader playerInputReader;
    private PlayerAnimation playerAnimation;
    private PlayerLayerVisuals playerLayerVisuals;


    public bool canMove = true;

    public void DisableMovementForDeath()
    {
        canMove = false;
        isMoving = false;
        targetPosition = rb != null ? rb.position : targetPosition;
        currentMoveSpeed = moveSpeed;

        if (playerAnimation != null)
            playerAnimation.SetChildrenSpriteRenderersEnabled(false);
    }

    public void DisableMovementForWin()
    {
        canMove = false;
        isMoving = false;
        targetPosition = rb != null ? rb.position : targetPosition;
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
        gridObject = GetComponent<GridObject>();
        playerInputReader = GetComponent<PlayerInputReader>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerLayerVisuals = GetComponent<PlayerLayerVisuals>();
        gridManager = GridManager.Instance;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
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

    private void Start()
    {
        RefreshTargetPositionFromGrid();
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

    public bool TryPushFromWater(Vector2Int cellDirection)
    {
        return TryMove(cellDirection, false, false, waterPushSpeed);
    }

    public bool TryPushFromWater(Vector2Int cellDirection, float pushSpeed)
    {
        return TryMove(cellDirection, false, false, pushSpeed);
    }

    private void TryMove(MoveDirection direction)
    {
        Vector2Int cellDirection = GetDirectionVector(direction);
        if (cellDirection == Vector2Int.zero)
            return;

        TryMove(cellDirection, true, true, moveSpeed);
    }

    private bool TryMove(Vector2Int cellDirection, bool allowBoxPush, bool updateFacing, float requestedSpeed)
    {
        if (cellDirection == Vector2Int.zero)
            return false;

        if (!canMove || isMoving)
            return false;

        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null || gridObject == null)
            return false;

        Vector2Int currentCell = gridObject.Cell;
        Vector2Int destinationCell = currentCell + cellDirection;

        if (gridManager.IsBlocked(destinationCell, gridObject.Size, gridObject))
            return false;

        if (gridManager.TryGetObjectAtArea(destinationCell, gridObject.Size, out GridObject occupant) && occupant != null && occupant != gridObject)
        {
            if (!allowBoxPush)
                return false;

            Box box = occupant.GetComponent<Box>();
            if (box == null)
                return false;

            bool pushed = box.TryPush(cellDirection, moveSpeed * 0.5f);
            if (!pushed)
                return false;

            currentMoveSpeed = moveSpeed * 0.5f;
        }
        else
        {
            currentMoveSpeed = Mathf.Max(0.01f, requestedSpeed);
        }

        if (!gridObject.TrySetCell(destinationCell))
            return false;

        targetPosition = gridManager.GridToWorld(destinationCell, gridObject.Size, gridObject.Anchor);
        isMoving = true;

        if (playerAnimation != null)
            playerAnimation.TriggerMoveAnimation();

        if (updateFacing && playerAnimation != null)
        {
            if (cellDirection == Vector2Int.right) playerAnimation.ApplyFlipX(true);
            else if (cellDirection == Vector2Int.left) playerAnimation.ApplyFlipX(false);
        }

        return true;
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

    private void RefreshTargetPositionFromGrid()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
        {
            targetPosition = rb != null ? rb.position : targetPosition;
            return;
        }

        if (gridObject != null)
        {
            Vector3 worldPosition = gridObject.GetWorldPosition();
            targetPosition = new Vector2(worldPosition.x, worldPosition.y);
        }
        else if (rb != null)
        {
            targetPosition = rb.position;
        }
    }

    private static Vector2Int GetDirectionVector(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return Vector2Int.up;
            case MoveDirection.Down:
                return Vector2Int.down;
            case MoveDirection.Left:
                return Vector2Int.left;
            case MoveDirection.Right:
                return Vector2Int.right;
            default:
                return Vector2Int.zero;
        }
    }
}
