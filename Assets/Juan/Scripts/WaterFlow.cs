using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterFlow : MonoBehaviour
{
    [Header("Water Flow")]
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField, Min(0f)] private float pushForce = 2f;
    [SerializeField, Min(0f)] private float blockedReleaseTime = 0.5f;

    [Header("Audio")]
    [SerializeField] private SoundType enterOrExitSound = SoundType.Agua;
    [SerializeField] private SoundType flowMovementSound = SoundType.Agua;

    private readonly HashSet<PlayerMovement> players = new HashSet<PlayerMovement>();
    private readonly HashSet<PlayerMovement> releasedPlayers = new HashSet<PlayerMovement>();
    private readonly HashSet<PlayerMovement> playersWithFlowMovementSound = new HashSet<PlayerMovement>();
    private readonly HashSet<Box> boxes = new HashSet<Box>();
    private readonly Dictionary<PlayerMovement, float> blockedTimes = new Dictionary<PlayerMovement, float>();

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        Vector2 flowDirection = direction.normalized;
        if (flowDirection == Vector2.zero)
            return;

        UpdatePlayers(flowDirection);
        UpdateBoxes(flowDirection);
    }

    private void UpdatePlayers(Vector2 flowDirection)
    {
        PlayerMovement[] activePlayers = new PlayerMovement[players.Count];
        players.CopyTo(activePlayers);

        for (int i = 0; i < activePlayers.Length; i++)
        {
            PlayerMovement player = activePlayers[i];
            if (player == null)
            {
                players.Remove(player);
                releasedPlayers.Remove(player);
                playersWithFlowMovementSound.Remove(player);
                continue;
            }

            bool moved = player.MoveByWater(flowDirection, pushForce, out bool blockedByObstacle);
            if (moved)
            {
                blockedTimes[player] = 0f;
                if (playersWithFlowMovementSound.Add(player))
                    SoundManager.PlaySound(flowMovementSound);
                continue;
            }

            if (blockedByObstacle)
            {
                releasedPlayers.Add(player);
                RemovePlayer(player);
                continue;
            }

            float blockedTime = blockedTimes.TryGetValue(player, out float elapsed) ? elapsed : 0f;
            blockedTime += Time.fixedDeltaTime;
            blockedTimes[player] = blockedTime;

            if (blockedTime >= blockedReleaseTime)
            {
                releasedPlayers.Add(player);
                RemovePlayer(player);
            }
        }
    }

    private void UpdateBoxes(Vector2 flowDirection)
    {
        Box[] activeBoxes = new Box[boxes.Count];
        boxes.CopyTo(activeBoxes);

        for (int i = 0; i < activeBoxes.Length; i++)
        {
            Box box = activeBoxes[i];
            if (box == null)
            {
                boxes.Remove(box);
                continue;
            }

            if (box.IsMoving)
                continue;

            box.MoveByWater(flowDirection, pushForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterObject(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        RegisterObject(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            RemovePlayer(player);
            releasedPlayers.Remove(player);
        }

        Box box = other.GetComponentInParent<Box>();
        if (box != null)
            boxes.Remove(box);
    }

    private void RegisterObject(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null && !releasedPlayers.Contains(player) && players.Add(player))
        {
            blockedTimes[player] = 0f;
            player.BeginWaterFlow();
            SoundManager.PlaySound(enterOrExitSound);
        }

        Box box = other.GetComponentInParent<Box>();
        if (box != null)
            boxes.Add(box);
    }

    private void RemovePlayer(PlayerMovement player)
    {
        if (!players.Remove(player))
            return;

        blockedTimes.Remove(player);
        playersWithFlowMovementSound.Remove(player);
        if (player != null)
        {
            player.EndWaterFlow();
            SoundManager.PlaySound(enterOrExitSound);
        }
    }
}
