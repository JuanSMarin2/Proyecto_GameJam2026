using UnityEngine;
using UnityEngine.Tilemaps;

public class GridCollision : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Vector3 worldQueryOffset = Vector3.zero;

    public bool IsObstacleAtCell(Vector2Int cell)
    {
        if (obstacleTilemap == null)
            return false;

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
            return false;

        Vector3 worldPosition = gridManager.GridToWorld(cell) + worldQueryOffset;
        Vector3Int tileCell = obstacleTilemap.WorldToCell(worldPosition);
        return obstacleTilemap.HasTile(tileCell);
    }

    public bool IsObstacleAtArea(Vector2Int originCell, Vector2Int size)
    {
        Vector2Int normalizedSize = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));

        for (int x = 0; x < normalizedSize.x; x++)
        {
            for (int y = 0; y < normalizedSize.y; y++)
            {
                if (IsObstacleAtCell(originCell + new Vector2Int(x, y)))
                    return true;
            }
        }

        return false;
    }
}