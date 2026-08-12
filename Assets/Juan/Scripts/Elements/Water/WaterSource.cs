using UnityEngine;

public class WaterSource : MonoBehaviour
{
    [Header("Water Source")]
    [SerializeField] private bool active = true;
    [SerializeField] private Vector2Int outletOffset = Vector2Int.down;

    [Header("Flow Timing")]
    [SerializeField] private float growthStepInterval = 0.18f;
    [SerializeField] private float drainStepInterval = 0.12f;

    private WaterFlow waterFlow;
    private GridManager gridManager;
    private Vector2Int sourceCell;

    public bool IsActive => active;
    public Vector2Int SourceCell => sourceCell;
    public Vector2Int OutletCell => sourceCell + GetNormalizedOutletOffset();

    private void Awake()
    {
        gridManager = GridManager.Instance;
        EnsureFlow();
    }

    private void Start()
    {
        RefreshGridState();
        ApplyActiveState(active);
    }

    private void OnEnable()
    {
        RefreshGridState();
        ApplyActiveState(active);
    }

    private void OnDisable()
    {
        if (waterFlow != null)
            waterFlow.SetSourceActive(false);
    }

    private void OnDestroy()
    {
        if (waterFlow != null)
            waterFlow.SetSourceActive(false);
    }

    public void Activate()
    {
        ApplyActiveState(true);
    }

    public void Deactivate()
    {
        ApplyActiveState(false);
    }

    public void SetActive(bool isActive)
    {
        ApplyActiveState(isActive);
    }

    public void ApplyActiveState(bool isActive)
    {
        active = isActive;
        RefreshGridState();
        EnsureFlow();

        if (waterFlow != null)
        {
            waterFlow.Configure(this, sourceCell, GetNormalizedOutletOffset(), growthStepInterval, drainStepInterval);
            waterFlow.SetSourceActive(active);
        }
    }

    public Vector3 GetSourceWorldPosition()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return transform.position;

        return gridManager.GridToWorld(sourceCell);
    }

    public Vector3 GetOutletWorldPosition()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return transform.position;

        return gridManager.GridToWorld(OutletCell);
    }

    private void EnsureFlow()
    {
        if (waterFlow != null)
            return;

        GameObject flowObject = new GameObject($"WaterFlow_{name}");
        flowObject.transform.position = Vector3.zero;
        waterFlow = flowObject.AddComponent<WaterFlow>();
    }

    private void RefreshGridState()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return;

        sourceCell = gridManager.WorldToGrid(transform.position);
    }

    private Vector2Int GetNormalizedOutletOffset()
    {
        if (outletOffset == Vector2Int.zero)
            return Vector2Int.down;

        if (Mathf.Abs(outletOffset.x) >= Mathf.Abs(outletOffset.y))
        {
            if (outletOffset.x > 0)
                return Vector2Int.right;

            if (outletOffset.x < 0)
                return Vector2Int.left;
        }

        if (outletOffset.y > 0)
            return Vector2Int.up;

        return Vector2Int.down;
    }
}