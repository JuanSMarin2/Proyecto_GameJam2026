using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public enum GridAnchor
    {
        Auto,
        Center,
        Left,
        Bottom,
        BottomLeft
    }

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;

    [Header("Obstacle Checks")]
    [SerializeField] private GridCollision gridCollision;

    private readonly Dictionary<Vector2Int, List<GridObject>> occupiedCells = new Dictionary<Vector2Int, List<GridObject>>();
    private readonly Dictionary<GridObject, List<Vector2Int>> occupiedCellsByObject = new Dictionary<GridObject, List<Vector2Int>>();

    public float CellSize => cellSize;

    public GridCollision GridCollision => gridCollision;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        return transform.position + worldOffset + new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
    }

    public Vector3 GridToWorld(Vector2Int cell, Vector2Int size, GridAnchor anchor)
    {
        Vector2 anchorOffset = GetAnchorOffsetCells(size, anchor);
        return GridToWorld(cell) + new Vector3(anchorOffset.x * cellSize, anchorOffset.y * cellSize, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 relativePosition = worldPosition - transform.position - worldOffset;
        return new Vector2Int(
            Mathf.RoundToInt(relativePosition.x / cellSize),
            Mathf.RoundToInt(relativePosition.y / cellSize));
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition, Vector2Int size, GridAnchor anchor)
    {
        Vector2 anchorOffset = GetAnchorOffsetCells(size, anchor);
        Vector3 adjustedWorldPosition = worldPosition - new Vector3(anchorOffset.x * cellSize, anchorOffset.y * cellSize, 0f);
        return WorldToGrid(adjustedWorldPosition);
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        return occupiedCells.TryGetValue(cell, out List<GridObject> occupants) && HasQueryableOccupant(occupants, null);
    }

    public bool IsAreaOccupied(Vector2Int originCell, Vector2Int size)
    {
        Vector2Int normalizedSize = NormalizeSize(size);

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                if (IsCellOccupied(originCell + new Vector2Int(x, y)))
                    return true;
            }
        }

        return false;
    }

    public bool IsCellOccupied(Vector2Int cell, GridObject ignoreObject)
    {
        if (!occupiedCells.TryGetValue(cell, out List<GridObject> occupants) || occupants == null)
            return false;

        return HasQueryableOccupant(occupants, ignoreObject);
    }

    public bool IsAreaOccupied(Vector2Int originCell, Vector2Int size, GridObject ignoreObject)
    {
        Vector2Int normalizedSize = NormalizeSize(size);

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                Vector2Int cell = originCell + new Vector2Int(x, y);
                if (IsCellOccupied(cell, ignoreObject))
                    return true;
            }
        }

        return false;
    }

    public bool TryGetObjectAtCell(Vector2Int cell, out GridObject gridObject)
    {
        if (occupiedCells.TryGetValue(cell, out List<GridObject> occupants) && occupants != null)
        {
            for (int i = 0; i < occupants.Count; i++)
            {
                GridObject occupant = occupants[i];
                if (occupant != null && occupant.IsQueryableInGrid)
                {
                    gridObject = occupant;
                    return true;
                }
            }
        }

        gridObject = null;
        return false;
    }

    public GridObject GetObjectAtCell(Vector2Int cell)
    {
        TryGetObjectAtCell(cell, out GridObject gridObject);
        return gridObject;
    }

    public bool TryGetObjectAtArea(Vector2Int originCell, Vector2Int size, out GridObject gridObject)
    {
        Vector2Int normalizedSize = NormalizeSize(size);

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                Vector2Int cell = originCell + new Vector2Int(x, y);
                if (TryGetObjectAtCell(cell, out gridObject))
                    return true;
            }
        }

        gridObject = null;
        return false;
    }

    public bool IsObstacleBlocked(Vector2Int cell)
    {
        return gridCollision != null && gridCollision.IsObstacleAtCell(cell);
    }

    public bool IsObstacleBlocked(Vector2Int originCell, Vector2Int size)
    {
        return gridCollision != null && gridCollision.IsObstacleAtArea(originCell, size);
    }

    public bool IsAreaBlockedByObjects(Vector2Int originCell, Vector2Int size, GridObject ignoreObject = null)
    {
        Vector2Int normalizedSize = NormalizeSize(size);

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                if (IsCellBlockedByObjects(originCell + new Vector2Int(x, y), ignoreObject))
                    return true;
            }
        }

        return false;
    }

    public bool IsBlocked(Vector2Int cell, GridObject ignoreObject = null)
    {
        return IsObstacleBlocked(cell) || IsCellBlockedByObjects(cell, ignoreObject);
    }

    public bool IsBlocked(Vector2Int originCell, Vector2Int size, GridObject ignoreObject = null)
    {
        return IsObstacleBlocked(originCell, size) || IsAreaBlockedByObjects(originCell, size, ignoreObject);
    }

    public void RegisterObject(GridObject gridObject, Vector2Int cell, Vector2Int size)
    {
        if (gridObject == null)
            return;

        UnregisterObject(gridObject);

        List<Vector2Int> cells = GetCellsForArea(cell, size);
        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int occupiedCell = cells[i];
            if (!occupiedCells.TryGetValue(occupiedCell, out List<GridObject> cellOccupants) || cellOccupants == null)
            {
                cellOccupants = new List<GridObject>();
                occupiedCells[occupiedCell] = cellOccupants;
            }

            if (!cellOccupants.Contains(gridObject))
                cellOccupants.Add(gridObject);
        }

        occupiedCellsByObject[gridObject] = cells;
        gridObject.Cell = cell;
    }

    public void UnregisterObject(GridObject gridObject)
    {
        if (gridObject == null)
            return;

        if (!occupiedCellsByObject.TryGetValue(gridObject, out List<Vector2Int> cells))
            return;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2Int cell = cells[i];
            if (occupiedCells.TryGetValue(cell, out List<GridObject> cellOccupants) && cellOccupants != null)
            {
                cellOccupants.Remove(gridObject);
                if (cellOccupants.Count == 0)
                    occupiedCells.Remove(cell);
            }
        }

        occupiedCellsByObject.Remove(gridObject);
    }

    public bool TryMoveObject(GridObject gridObject, Vector2Int newCell, Vector2Int size)
    {
        if (gridObject == null)
            return false;

        if (IsBlocked(newCell, size, gridObject))
            return false;

        UnregisterObject(gridObject);
        RegisterObject(gridObject, newCell, size);
        gridObject.Cell = newCell;
        return true;
    }

    private static Vector2Int NormalizeSize(Vector2Int size)
    {
        return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
    }

    public static GridAnchor ResolveAnchor(Vector2Int size, GridAnchor anchor)
    {
        Vector2Int normalizedSize = NormalizeSize(size);

        if (anchor != GridAnchor.Auto)
            return anchor;

        bool hasHorizontalSpan = normalizedSize.x > 1;
        bool hasVerticalSpan = normalizedSize.y > 1;

        if (!hasHorizontalSpan && !hasVerticalSpan)
            return GridAnchor.Center;

        if (hasHorizontalSpan && !hasVerticalSpan)
            return GridAnchor.Left;

        if (!hasHorizontalSpan && hasVerticalSpan)
            return GridAnchor.Bottom;

        return GridAnchor.BottomLeft;
    }

    public static Vector2 GetAnchorOffsetCells(Vector2Int size, GridAnchor anchor)
    {
        Vector2Int normalizedSize = NormalizeSize(size);
        GridAnchor resolvedAnchor = ResolveAnchor(normalizedSize, anchor);

        float centerOffsetX = (normalizedSize.x - 1) * 0.5f;
        float centerOffsetY = (normalizedSize.y - 1) * 0.5f;

        switch (resolvedAnchor)
        {
            case GridAnchor.Center:
                return new Vector2(centerOffsetX, centerOffsetY);
            case GridAnchor.Left:
                return new Vector2(centerOffsetX, 0f);
            case GridAnchor.Bottom:
                return new Vector2(0f, centerOffsetY);
            case GridAnchor.BottomLeft:
                return Vector2.zero;
            default:
                return new Vector2(centerOffsetX, centerOffsetY);
        }
    }

    private static List<Vector2Int> GetCellsForArea(Vector2Int originCell, Vector2Int size)
    {
        Vector2Int normalizedSize = NormalizeSize(size);
        List<Vector2Int> cells = new List<Vector2Int>(normalizedSize.x * normalizedSize.y);

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                cells.Add(originCell + new Vector2Int(x, y));
            }
        }

        return cells;
    }

    private bool IsCellBlockedByObjects(Vector2Int cell, GridObject ignoreObject)
    {
        if (!occupiedCells.TryGetValue(cell, out List<GridObject> occupants) || occupants == null)
            return false;

        for (int i = 0; i < occupants.Count; i++)
        {
            GridObject occupant = occupants[i];
            if (occupant == null || occupant == ignoreObject || !occupant.IsQueryableInGrid)
                continue;

            if (occupant.BlocksMovement)
                return true;
        }

        return false;
    }

    private static bool HasQueryableOccupant(List<GridObject> occupants, GridObject ignoreObject)
    {
        if (occupants == null)
            return false;

        for (int i = 0; i < occupants.Count; i++)
        {
            GridObject occupant = occupants[i];
            if (occupant == null || occupant == ignoreObject || !occupant.IsQueryableInGrid)
                continue;

            return true;
        }

        return false;
    }
}