using UnityEngine;

public class MoveObstacle : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform[] nodes;
    [SerializeField] private bool closed = false;

    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float reachDistance = 0.05f;
    [SerializeField] private float stopDuration = 0.5f;

    private int currentNodeIndex = 0;
    private int direction = 1; // 1 = adelante, -1 = atrás
    private float stopTimer = 0f;
    private bool isStopping = false;

    private void Start()
    {
        if (nodes == null || nodes.Length < 2)
        {
            Debug.LogError("NodeMover necesita al menos 2 nodos.");
            enabled = false;
            return;
        }

        transform.position = nodes[0].position;
    }

    private void Update()
    {
        if (isStopping)
        {
            HandleStop();
        }
        else
        {
            MoveTowardsNode();
        }
    }

    private void MoveTowardsNode()
    {
        Transform targetNode = nodes[currentNodeIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetNode.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetNode.position) <= reachDistance)
        {
            isStopping = true;
            stopTimer = stopDuration;
        }
    }

    private void HandleStop()
    {
        stopTimer -= Time.deltaTime;

        if (stopTimer <= 0f)
        {
            isStopping = false;
            SelectNextNode();
        }
    }

    private void SelectNextNode()
    {
        if (closed)
        {
            // Loop: 1 → 2 → 3 → 1
            currentNodeIndex = (currentNodeIndex + 1) % nodes.Length;
        }
        else
        {
            // Ping-pong: 1 → 2 → 3 → 2
            if (currentNodeIndex == nodes.Length - 1)
                direction = -1;
            else if (currentNodeIndex == 0)
                direction = 1;

            currentNodeIndex += direction;
        }
    }

   

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Length < 2) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < nodes.Length - 1; i++)
        {
            Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
        }

        if (closed)
        {
            Gizmos.DrawLine(nodes[nodes.Length - 1].position, nodes[0].position);
        }
    }
}
