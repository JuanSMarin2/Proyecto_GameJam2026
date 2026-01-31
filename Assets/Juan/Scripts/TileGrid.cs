using UnityEngine;

public class TileGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private bool showGrid = true;
    [SerializeField] private float tileSize = 0.2f;
    [SerializeField] private int halfWidthTiles = 10;
    [SerializeField] private int halfHeightTiles = 10;
    [SerializeField] private Color gridColor = new Color(0.2f, 0.9f, 0.6f, 0.35f);
    [SerializeField] private Color axisColor = new Color(1f, 0.2f, 0.2f, 0.6f);

    private void OnDrawGizmos()
    {
        if (!showGrid || player == null) return;

        Vector3 basePos = player.position;

        float minX = basePos.x - halfWidthTiles * tileSize - tileSize * 0.5f;
        float maxX = basePos.x + halfWidthTiles * tileSize + tileSize * 0.5f;
        float minY = basePos.y - halfHeightTiles * tileSize - tileSize * 0.5f;
        float maxY = basePos.y + halfHeightTiles * tileSize + tileSize * 0.5f;

        Gizmos.color = gridColor;
        for (int i = -halfWidthTiles; i <= halfWidthTiles; i++)
        {
            float x = basePos.x + (i + 0.5f) * tileSize;
            Gizmos.DrawLine(new Vector3(x, minY, basePos.z), new Vector3(x, maxY, basePos.z));
        }

        for (int j = -halfHeightTiles; j <= halfHeightTiles; j++)
        {
            float y = basePos.y + (j + 0.5f) * tileSize;
            Gizmos.DrawLine(new Vector3(minX, y, basePos.z), new Vector3(maxX, y, basePos.z));
        }

        Gizmos.color = axisColor;
        float xLeft = basePos.x - tileSize * 0.5f;
        float xRight = basePos.x + tileSize * 0.5f;
        float yBottom = basePos.y - tileSize * 0.5f;
        float yTop = basePos.y + tileSize * 0.5f;

        Gizmos.DrawLine(new Vector3(xLeft, yBottom, basePos.z), new Vector3(xLeft, yTop, basePos.z));
        Gizmos.DrawLine(new Vector3(xRight, yBottom, basePos.z), new Vector3(xRight, yTop, basePos.z));
        Gizmos.DrawLine(new Vector3(xLeft, yBottom, basePos.z), new Vector3(xRight, yBottom, basePos.z));
        Gizmos.DrawLine(new Vector3(xLeft, yTop, basePos.z), new Vector3(xRight, yTop, basePos.z));

        Gizmos.DrawWireSphere(basePos, tileSize * 0.15f);
    }
}
