using System.Collections.Generic;
using UnityEngine;

public class WaterFlow : MonoBehaviour
{
    private sealed class WaterCellState
    {
        public int SupportCount;
        public WaterVisual Visual;
        public bool PendingRemoval;
        public Vector2Int FlowDirection;
        public bool IsFlowing;
        public bool IsHead;
    }

    private static readonly Dictionary<Vector2Int, WaterCellState> cellStates = new Dictionary<Vector2Int, WaterCellState>();
    private static Transform sharedVisualRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cellStates.Clear();
        sharedVisualRoot = null;
    }

    [Header("Visuals")]
    [SerializeField] private WaterVisual waterVisualPrefab;

    private readonly List<Vector2Int> pathCells = new List<Vector2Int>();
    private readonly List<Vector2Int> drainingCells = new List<Vector2Int>();
    private readonly HashSet<Vector2Int> pathLookup = new HashSet<Vector2Int>();

    private WaterSource source;
    private GridManager gridManager;
    private Vector2Int sourceCell;
    private Vector2Int outletCell;
    private Vector2Int outletDirection = Vector2Int.down;
    private float growthStepInterval = 0.18f;
    private float drainStepInterval = 0.12f;
    [SerializeField] private float playerPushSpeed = 3.5f;
    private bool sourceActive;
    private float stepTimer;

    public void Configure(WaterSource owner, Vector2Int ownerSourceCell, Vector2Int ownerOutletDirection, float growthInterval, float drainInterval)
    {
        source = owner;
        sourceCell = ownerSourceCell;
        outletDirection = ownerOutletDirection == Vector2Int.zero ? Vector2Int.down : ownerOutletDirection;
        outletCell = sourceCell + outletDirection;
        growthStepInterval = Mathf.Max(0.01f, growthInterval);
        drainStepInterval = Mathf.Max(0.01f, drainInterval);
    }

    public void SetSourceActive(bool isActive)
    {
        if (sourceActive == isActive)
        {
            stepTimer = 0f;
            return;
        }

        sourceActive = isActive;
        stepTimer = 0f;

        if (!sourceActive)
            MoveActivePathToDrainQueue();
    }

    private void Update()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return;

        ReconcileActivePathWithWorld();

        if (!sourceActive && pathCells.Count == 0 && drainingCells.Count == 0)
            return;

        float stepInterval = sourceActive ? growthStepInterval : drainStepInterval;
        stepTimer += Time.deltaTime;

        while (stepTimer >= stepInterval)
        {
            stepTimer -= stepInterval;

            if (sourceActive)
                TryGrowOneCell();
            else
                TryDrainOneCell();

            if (!sourceActive && pathCells.Count == 0 && drainingCells.Count == 0)
                break;
        }

        UpdateFlowVisualsAndPushPlayer();
    }

    public bool HasWaterAt(Vector2Int cell)
    {
        return cellStates.ContainsKey(cell);
    }

    public static bool TryGetWaterVisual(Vector2Int cell, out WaterVisual visual)
    {
        if (cellStates.TryGetValue(cell, out WaterCellState state) && state != null && state.Visual != null)
        {
            visual = state.Visual;
            return true;
        }

        visual = null;
        return false;
    }

    public static bool HasWater(Vector2Int cell)
    {
        return cellStates.ContainsKey(cell);
    }

    private void TryGrowOneCell()
    {
        Vector2Int currentHead = pathCells.Count > 0 ? pathCells[pathCells.Count - 1] : outletCell;
        if (!TryGetNextCell(currentHead, out Vector2Int nextCell))
            return;

        Vector3 startWorld = pathCells.Count > 0 ? GetCellWorld(currentHead) : GetSourceWorld();
        AddActiveCell(nextCell, startWorld, pathCells.Count == 0);
    }

    private void TryDrainOneCell()
    {
        if (drainingCells.Count == 0)
            return;

        int lastIndex = drainingCells.Count - 1;
        Vector2Int removedCell = drainingCells[lastIndex];
        drainingCells.RemoveAt(lastIndex);

        if (!cellStates.TryGetValue(removedCell, out WaterCellState state) || state == null)
            return;

        state.SupportCount = Mathf.Max(0, state.SupportCount - 1);

        if (state.SupportCount == 0)
        {
            if (state.Visual != null && !state.PendingRemoval)
            {
                state.PendingRemoval = true;
                Vector2Int capturedCell = removedCell;
                state.Visual.BeginRemoval(() => OnVisualRemovalCompleted(capturedCell));
            }

            if (pathCells.Count > 0)
                RefreshHeadVisual(pathCells[pathCells.Count - 1]);

            return;
        }

        if (pathCells.Count > 0)
            RefreshHeadVisual(pathCells[pathCells.Count - 1]);
    }

    private void OnVisualRemovalCompleted(Vector2Int cell)
    {
        if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
            return;

        if (state.SupportCount == 0 && state.PendingRemoval)
            cellStates.Remove(cell);
    }

    private void AddActiveCell(Vector2Int cell, Vector3 startWorld, bool isFirstCell)
    {
        if (drainingCells.Contains(cell))
            drainingCells.Remove(cell);

        pathCells.Add(cell);
        pathLookup.Add(cell);

        WaterCellState state = GetOrCreateState(cell);
        state.SupportCount++;
        state.IsFlowing = true;
        state.PendingRemoval = false;

        Vector3 targetWorld = GetCellWorld(cell);

        if (state.Visual == null)
        {
            state.Visual = CreateVisual(cell, startWorld, targetWorld, true, state.SupportCount == 1 && IsCurrentHead(cell));
        }
        else
        {
            state.Visual.CancelRemoval();
            state.Visual.SetCell(cell, targetWorld, true);
        }

        if (pathCells.Count > 1)
            RefreshVisualBody(pathCells[pathCells.Count - 2]);

        UpdateVisualState(cell, state, IsCurrentHead(cell));

        if (isFirstCell)
            RefreshHeadVisual(cell);
    }

    private void RefreshHeadVisual(Vector2Int cell)
    {
        if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
            return;

        UpdateVisualState(cell, state, true);
    }

    private void RefreshVisualBody(Vector2Int cell)
    {
        if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
            return;

        UpdateVisualState(cell, state, false);
    }

    private void UpdateVisualState(Vector2Int cell, WaterCellState state, bool isHead)
    {
        if (state.Visual == null || state.PendingRemoval)
            return;

        state.IsHead = isHead;

        if (isHead)
            state.Visual.SetHead(true);
        else
            state.Visual.SetHead(false);

        state.Visual.SetFlowDirection(GetFlowDirectionForCell(cell));
        state.Visual.SetFlowActive(state.IsFlowing);
    }

    private bool TryGetNextCell(Vector2Int currentCell, out Vector2Int nextCell)
    {
        Vector2Int[] offsets = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int candidate = currentCell + offsets[i];
            if (CanOccupyCell(candidate))
            {
                nextCell = candidate;
                return true;
            }
        }

        nextCell = currentCell;
        return false;
    }

    private bool CanOccupyCell(Vector2Int cell)
    {
        if (pathLookup.Contains(cell))
            return false;

        if (HasWater(cell))
            return false;

        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return false;

        if (IsCellBlockedByWaterRules(cell))
            return false;

        return true;
    }

    private WaterCellState GetOrCreateState(Vector2Int cell)
    {
        if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
        {
            state = new WaterCellState();
            cellStates[cell] = state;
        }

        return state;
    }

    private WaterVisual CreateVisual(Vector2Int cell, Vector3 startWorld, Vector3 targetWorld, bool animate, bool isHead)
    {
        Transform parent = GetSharedVisualRoot();
        WaterVisual visualInstance;

        if (waterVisualPrefab != null)
        {
            visualInstance = Instantiate(waterVisualPrefab, parent);
        }
        else
        {
            GameObject visualObject = new GameObject($"WaterVisual_{cell.x}_{cell.y}");
            visualObject.transform.SetParent(parent, false);
            visualInstance = visualObject.AddComponent<WaterVisual>();
        }

        visualInstance.Initialize(cell, startWorld, targetWorld, animate, isHead);
        return visualInstance;
    }

    private static Transform GetSharedVisualRoot()
    {
        if (sharedVisualRoot != null)
            return sharedVisualRoot;

        GameObject rootObject = new GameObject("WaterVisuals");
        sharedVisualRoot = rootObject.transform;
        return sharedVisualRoot;
    }

    private Vector3 GetCellWorld(Vector2Int cell)
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return Vector3.zero;

        return gridManager.GridToWorld(cell);
    }

    private Vector3 GetSourceWorld()
    {
        if (source != null)
            return source.GetSourceWorldPosition();

        return GetCellWorld(sourceCell);
    }

    private void MoveActivePathToDrainQueue()
    {
        for (int i = pathCells.Count - 1; i >= 0; i--)
        {
            Vector2Int cell = pathCells[i];
            if (!drainingCells.Contains(cell))
                drainingCells.Add(cell);
        }

        pathLookup.Clear();
        pathCells.Clear();
    }

    private void ReconcileActivePathWithWorld()
    {
        if (pathCells.Count == 0)
            return;

        int firstBlockedIndex = -1;

        for (int i = 0; i < pathCells.Count; i++)
        {
            if (!IsActivePathCellStillValid(pathCells[i]))
            {
                firstBlockedIndex = i;
                break;
            }
        }

        if (firstBlockedIndex < 0)
            return;

        for (int i = pathCells.Count - 1; i >= firstBlockedIndex; i--)
        {
            Vector2Int cell = pathCells[i];
            if (!drainingCells.Contains(cell))
                drainingCells.Add(cell);

            if (cellStates.TryGetValue(cell, out WaterCellState state) && state != null)
                state.IsFlowing = false;

            pathLookup.Remove(cell);
        }

        pathCells.RemoveRange(firstBlockedIndex, pathCells.Count - firstBlockedIndex);
    }

    private bool IsActivePathCellStillValid(Vector2Int cell)
    {
        if (IsCellBlockedByWaterRules(cell))
            return false;

        return true;
    }

    private void UpdateFlowVisualsAndPushPlayer()
    {
        for (int i = 0; i < pathCells.Count; i++)
        {
            Vector2Int cell = pathCells[i];
            if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
                continue;

            Vector2Int flowDirection = GetFlowDirectionForCell(cell);
            state.FlowDirection = flowDirection;
            state.IsFlowing = true;
            state.PendingRemoval = false;

            if (state.Visual != null)
            {
                state.Visual.SetFlowDirection(flowDirection);
                state.Visual.SetFlowActive(true);
                state.Visual.SetHead(i == pathCells.Count - 1);
            }

            if (flowDirection == Vector2Int.zero)
                continue;

            if (!gridManager.TryGetObjectAtCell(cell, out GridObject occupant) || occupant == null)
                continue;

            PlayerMovement playerMovement = occupant.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.TryPushFromWater(flowDirection, playerPushSpeed);
        }

        for (int i = 0; i < drainingCells.Count; i++)
        {
            Vector2Int cell = drainingCells[i];
            if (!cellStates.TryGetValue(cell, out WaterCellState state) || state == null)
                continue;

            state.IsFlowing = false;
            state.FlowDirection = Vector2Int.zero;

            if (state.Visual != null)
            {
                state.Visual.SetFlowDirection(Vector2Int.zero);
                state.Visual.SetFlowActive(false);
                state.Visual.SetHead(false);
            }
        }
    }

    private Vector2Int GetFlowDirectionForCell(Vector2Int cell)
    {
        int activeIndex = pathCells.IndexOf(cell);
        if (activeIndex >= 0)
        {
            if (activeIndex < pathCells.Count - 1)
                return NormalizeDirection(pathCells[activeIndex + 1] - cell);

            if (sourceActive && TryGetNextExpansionCell(cell, out Vector2Int nextCell))
                return NormalizeDirection(nextCell - cell);

            if (pathCells.Count > 1)
                return NormalizeDirection(cell - pathCells[activeIndex - 1]);

            return outletDirection;
        }

        int drainingIndex = drainingCells.IndexOf(cell);
        if (drainingIndex >= 0)
            return Vector2Int.zero;

        return Vector2Int.zero;
    }

    private bool TryGetNextExpansionCell(Vector2Int currentCell, out Vector2Int nextCell)
    {
        Vector2Int[] offsets = new Vector2Int[]
        {
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int candidate = currentCell + offsets[i];
            if (CanOccupyCell(candidate))
            {
                nextCell = candidate;
                return true;
            }
        }

        nextCell = currentCell;
        return false;
    }

    private static Vector2Int NormalizeDirection(Vector2Int direction)
    {
        if (direction.x > 0)
            return Vector2Int.right;

        if (direction.x < 0)
            return Vector2Int.left;

        if (direction.y > 0)
            return Vector2Int.up;

        if (direction.y < 0)
            return Vector2Int.down;

        return Vector2Int.zero;
    }

    private bool IsCurrentHead(Vector2Int cell)
    {
        return pathCells.Count > 0 && pathCells[pathCells.Count - 1] == cell;
    }

    private bool IsCellBlockedByWaterRules(Vector2Int cell)
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
            return false;

        if (gridManager.IsObstacleBlocked(cell))
            return true;

        if (!gridManager.TryGetObjectAtCell(cell, out GridObject occupant) || occupant == null)
            return false;

        if (occupant.CompareTag("Box"))
            return true;

        return occupant.BlocksMovement;
    }
}