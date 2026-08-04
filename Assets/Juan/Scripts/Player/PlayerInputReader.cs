using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput playerInput;
    private bool hasPendingMove;
    private MoveDirection pendingMove = MoveDirection.None;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public bool ConsumeMove(out MoveDirection direction)
    {
        if (hasPendingMove && pendingMove != MoveDirection.None)
        {
            direction = pendingMove;
            hasPendingMove = false;
            pendingMove = MoveDirection.None;
            return true;
        }

        direction = MoveDirection.None;
        return false;
    }

    public void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        MoveDirection direction = ToMoveDirection(move);

        if (direction == MoveDirection.None)
            return;

        pendingMove = direction;
        hasPendingMove = true;
    }

    private static MoveDirection ToMoveDirection(Vector2 move)
    {
        if (move.sqrMagnitude < 0.01f)
            return MoveDirection.None;

        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            return move.x > 0f ? MoveDirection.Right : MoveDirection.Left;

        return move.y > 0f ? MoveDirection.Up : MoveDirection.Down;
    }
}