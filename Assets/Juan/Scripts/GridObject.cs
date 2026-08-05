using UnityEngine;

public class GridObject : MonoBehaviour
{
    public Vector2Int Cell;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private GridManager.GridAnchor gridAnchor = GridManager.GridAnchor.Auto;

    public Vector2Int Size => new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
    public GridManager.GridAnchor Anchor => gridAnchor;

    private GridManager gridManager;
    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private bool hasBeenInitialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void Start()
    {
        InitializeFromWorldPosition();
    }

    private void OnEnable()
    {
        if (hasBeenInitialized && gridManager != null)
            gridManager.RegisterObject(this, Cell, Size);
    }

    private void OnDisable()
    {
        if (hasBeenInitialized && gridManager != null)
            gridManager.UnregisterObject(this);
    }

    private void OnDestroy()
    {
        if (hasBeenInitialized && gridManager != null)
            gridManager.UnregisterObject(this);
    }

    public void InitializeFromWorldPosition()
    {
        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError($"GridObject: no existe GridManager en la escena para {name}.", this);
            return;
        }

        Cell = gridManager.WorldToGrid(transform.position, Size, Anchor);
        gridManager.RegisterObject(this, Cell, Size);
        SnapToCell();
        hasBeenInitialized = true;
    }

    public bool TrySetCell(Vector2Int newCell)
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return false;

        return gridManager.TryMoveObject(this, newCell, Size);
    }

    public void SnapToCell()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return;

        Vector3 worldPosition = gridManager.GridToWorld(Cell, Size, Anchor);
        transform.position = worldPosition;

        if (rb != null)
            rb.position = new Vector2(worldPosition.x, worldPosition.y);
    }

    public Vector3 GetWorldPosition()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return transform.position;

        return gridManager.GridToWorld(Cell, Size, Anchor);
    }

    public bool BlocksMovement
    {
        get
        {
            return CompareTag("Wall") && HasActiveCollider();
        }
    }

    public bool IsQueryableInGrid
    {
        get { return HasActiveCollider(); }
    }

    private bool HasActiveCollider()
    {
        if (colliders == null || colliders.Length == 0)
            return false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider2D = colliders[i];
            if (collider2D == null)
                continue;

            if (collider2D.enabled && collider2D.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }
}