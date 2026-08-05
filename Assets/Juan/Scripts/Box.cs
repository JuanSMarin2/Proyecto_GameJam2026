using UnityEngine;

[RequireComponent(typeof(GridObject))]
public class Box : MonoBehaviour
{
    [Header("Box Movement")]
    private Rigidbody2D rb;
    private GridObject gridObject;
    private GridManager gridManager;
    private Vector2 targetPosition;
    private float moveSpeed;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gridObject = GetComponent<GridObject>();
        gridManager = GridManager.Instance;
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    private void Start()
    {
        SyncTargetPositionToCell();
    }

    public bool TryPush(Vector2Int direction, float speed)
    {
        if (isMoving) return false;

        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null || gridObject == null)
            return false;

        Vector2Int destinationCell = gridObject.Cell + direction;
        if (gridManager.IsBlocked(destinationCell, gridObject.Size, gridObject)) return false;

        // Regla: la caja no puede terminar en una casilla que esté justo arriba
        // o justo abajo de una pared (adyacente verticalmente a 1 tile).
        Vector2Int boxSize = gridObject.Size;
        for (int x = 0; x < boxSize.x; x++)
        {
            for (int y = 0; y < boxSize.y; y++)
            {
                Vector2Int occupiedCell = destinationCell + new Vector2Int(x, y);
                if (gridManager.IsObstacleBlocked(occupiedCell + Vector2Int.up) || gridManager.IsObstacleBlocked(occupiedCell + Vector2Int.down))
                    return false;
            }
        }

        if (gridManager.TryGetObjectAtArea(destinationCell, gridObject.Size, out GridObject occupant) && occupant != null && occupant != gridObject)
            return false;

        if (!gridObject.TrySetCell(destinationCell))
            return false;

        targetPosition = gridManager.GridToWorld(destinationCell, gridObject.Size, gridObject.Anchor);
        moveSpeed = Mathf.Max(0.01f, speed);
        SoundManager.PlaySound(SoundType.BloqueMoviendose);
        isMoving = true;
        Debug.Log("Box is Moving");
        return true;
    }

    public bool TryPush(Vector2 direction, float tileSize, float speed)
    {
        Vector2Int gridDirection = new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
        return TryPush(gridDirection, speed);
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 next = Vector2.MoveTowards(current, targetPosition, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, transform.position.z);

        if ((targetPosition - next).sqrMagnitude <= 0.0001f)
        {
            if (rb != null) rb.MovePosition(targetPosition);
            else transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            isMoving = false;
        }
    }

    private void SyncTargetPositionToCell()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null || gridObject == null)
        {
            targetPosition = rb != null ? rb.position : targetPosition;
            return;
        }

        Vector3 worldPosition = gridObject.GetWorldPosition();
        targetPosition = new Vector2(worldPosition.x, worldPosition.y);
    }
}
