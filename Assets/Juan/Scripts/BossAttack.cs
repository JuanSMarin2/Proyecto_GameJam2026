using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttacks : MonoBehaviour
{
    [Header("Origins")]
    [SerializeField] private Transform wallOriginRight;
    [SerializeField] private Transform wallOriginLeft;

    [Header("Pool")]
    [SerializeField] private List<DamageWall> damageWalls; 

    [Header("Settings")]
    [SerializeField] private float launchForce = 10f;

    [Header("Arrows")]
    [SerializeField] private List<Transform> bows;
    [SerializeField] private List<Rigidbody2D> arrows;
    [SerializeField] private float arrowForce = 15f;

    [Header("Enrage")]
    [SerializeField] private float enragedWallForceBonus = 5f;
    [SerializeField] private float enragedArrowForceBonus = 5f;

    [Header("Rock")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private List<Transform> rockNodes;

    [Header("Rock Chance")]
    [SerializeField, Range(0f, 1f)] private float rockChanceBase = 0.2f;
    [SerializeField, Range(0f, 1f)] private float rockChanceIncreaseOnMiss = 0.1f;
    [SerializeField, Range(0f, 1f)] private float otherAttackChancePerMove = 0.9f;

    private GameObject currentRock;
    private float rockChanceCurrent;

    private float baseWallLaunchForce;
    private float baseArrowForce;
    private bool isEnraged;

    private bool forceLowWallsNext;

    private void Awake()
    {
        rockChanceCurrent = rockChanceBase;

        baseWallLaunchForce = launchForce;
        baseArrowForce = arrowForce;
        ApplyEnrageForces(false);
    }

    public void SetEnraged(bool enraged)
    {
        if (isEnraged == enraged) return;
        isEnraged = enraged;
        ApplyEnrageForces(isEnraged);
    }

    private void ApplyEnrageForces(bool enraged)
    {
        launchForce = baseWallLaunchForce + (enraged ? enragedWallForceBonus : 0f);
        arrowForce = baseArrowForce + (enraged ? enragedArrowForceBonus : 0f);
    }


    public void LaunchAttack()
    {
        // Rock is not part of the random pool: it has its own cumulative chance per movement.
        if (Random.value <= rockChanceCurrent)
        {
            if (TrySpawnRock())
            {
                rockChanceCurrent = rockChanceBase;
                return;
            }
        }

        rockChanceCurrent = Mathf.Clamp01(rockChanceCurrent + rockChanceIncreaseOnMiss);

        if (Random.value > otherAttackChancePerMove)
            return;

        int randomAttack = Random.Range(0, 3);
        switch (randomAttack)
        {
            case 0:
                StartCoroutine(OneWall());
                break;
            case 1:
                TwoWalls();
                break;
            case 2:
                Arrows();
                break;
        }
    }

    #region ONE WALL

    private IEnumerator OneWall()
    {
        bool requireLowWallsThisCall = forceLowWallsNext;
        bool launchedAnyWallThisCall = false;

        bool useRight = Random.value > 0.5f;
        Transform origin = useRight ? wallOriginRight : wallOriginLeft;
        Vector2 direction = useRight ? Vector2.left : Vector2.right;

        int firstIndex = requireLowWallsThisCall
            ? GetRandomAvailableWallIndexInRange(0, 7)
            : GetRandomAvailableWallIndexInRange(0, damageWalls != null ? damageWalls.Count - 1 : -1);
        if (firstIndex < 0)
            yield break;
        launchedAnyWallThisCall |= LaunchWall(firstIndex, origin, direction);
        if (requireLowWallsThisCall && launchedAnyWallThisCall)
            forceLowWallsNext = false;

       
        if (Random.value <= 0.8f)
        {
            yield return new WaitForSeconds(2f);

            int secondIndex = requireLowWallsThisCall
                ? GetRandomAvailableWallIndexOppositeParityInRange(firstIndex, 0, 7)
                : GetRandomAvailableWallIndexOppositeParityInRange(firstIndex, 0, damageWalls != null ? damageWalls.Count - 1 : -1);
            if (secondIndex >= 0)
                launchedAnyWallThisCall |= LaunchWall(secondIndex, origin, direction);
            if (requireLowWallsThisCall && launchedAnyWallThisCall)
                forceLowWallsNext = false;
        }
    }

    #endregion

    #region TWO WALLS

    private void TwoWalls()
    {
        bool requireLowWallsThisCall = forceLowWallsNext;
        bool launchedAnyWallThisCall = false;

        int firstIndex = requireLowWallsThisCall
            ? GetRandomAvailableWallIndexInRange(0, 7)
            : GetRandomAvailableWallIndexInRange(0, damageWalls != null ? damageWalls.Count - 1 : -1);
        if (firstIndex < 0)
            return;

        int secondIndex = requireLowWallsThisCall
            ? GetRandomAvailableWallIndexOppositeParityInRange(firstIndex, 0, 7)
            : GetRandomAvailableWallIndexOppositeParityInRange(firstIndex, 0, damageWalls != null ? damageWalls.Count - 1 : -1);

        launchedAnyWallThisCall |= LaunchWall(firstIndex, wallOriginRight, Vector2.left);
        if (secondIndex >= 0)
            launchedAnyWallThisCall |= LaunchWall(secondIndex, wallOriginLeft, Vector2.right);

        if (requireLowWallsThisCall && launchedAnyWallThisCall)
            forceLowWallsNext = false;
    }

    #endregion

    #region HELPERS

    private bool LaunchWall(int index, Transform origin, Vector2 direction)
    {
        if (damageWalls == null) return false;
        if (index < 0 || index >= damageWalls.Count) return false;

        DamageWall wall = damageWalls[index];
        if (wall == null) return false;
        if (wall.IsMoving) return false;

        wall.transform.position = GetWallSpawnPosition(index, origin);
        wall.Launch(direction, launchForce);

        if (IsHighWallIndex(index))
            forceLowWallsNext = true;

        return true;
    }

    private bool IsHighWallIndex(int index)
    {
        return index >= 8 && index <= 14;
    }

    private Vector3 GetWallSpawnPosition(int wallIndex, Transform origin)
    {
        Vector3 pos = origin != null ? origin.position : Vector3.zero;

        // 0-7: origin height
        // 8-11: same origin X/Z, but Y = -20.2
        // 12-14: same origin X/Z, but Y = -13.6
        if (wallIndex >= 8 && wallIndex <= 11)
            pos.y = -20.2f;
        else if (wallIndex >= 12 && wallIndex <= 14)
            pos.y = -13.6f;

        return pos;
    }

    private int GetRandomIndex()
    {
        return Random.Range(0, damageWalls.Count);
    }

    private int GetRandomAvailableWallIndexInRange(int minIndexInclusive, int maxIndexInclusive)
    {
        if (damageWalls == null || damageWalls.Count == 0)
            return -1;

        minIndexInclusive = Mathf.Clamp(minIndexInclusive, 0, damageWalls.Count - 1);
        maxIndexInclusive = Mathf.Clamp(maxIndexInclusive, 0, damageWalls.Count - 1);
        if (minIndexInclusive > maxIndexInclusive)
            return -1;

        List<int> validIndexes = new List<int>();

        for (int i = minIndexInclusive; i <= maxIndexInclusive; i++)
        {
            DamageWall wall = damageWalls[i];
            if (wall == null) continue;
            if (wall.IsMoving) continue;
            validIndexes.Add(i);
        }

        if (validIndexes.Count == 0)
            return -1;

        return validIndexes[Random.Range(0, validIndexes.Count)];
    }

    private int GetRandomAvailableWallIndexOppositeParityInRange(int referenceIndex, int minIndexInclusive, int maxIndexInclusive)
    {
        if (damageWalls == null || damageWalls.Count == 0)
            return -1;
        if (referenceIndex < 0 || referenceIndex >= damageWalls.Count)
            return -1;

        minIndexInclusive = Mathf.Clamp(minIndexInclusive, 0, damageWalls.Count - 1);
        maxIndexInclusive = Mathf.Clamp(maxIndexInclusive, 0, damageWalls.Count - 1);
        if (minIndexInclusive > maxIndexInclusive)
            return -1;

        int desiredParity = 1 - (Mathf.Abs(referenceIndex) % 2); // if reference is even -> 1 (odd), if odd -> 0 (even)
        List<int> validIndexes = new List<int>();

        for (int i = minIndexInclusive; i <= maxIndexInclusive; i++)
        {
            if (i == referenceIndex) continue;
            if ((i % 2) != desiredParity) continue;
            DamageWall wall = damageWalls[i];
            if (wall == null) continue;
            if (wall.IsMoving) continue;
            validIndexes.Add(i);
        }

        if (validIndexes.Count == 0)
            return -1;

        return validIndexes[Random.Range(0, validIndexes.Count)];
    }

    #endregion

    private void Arrows()
    {
        int arrowCount = Mathf.Min(bows.Count, arrows.Count);

        for (int i = 0; i < arrowCount; i++)
        {
            Rigidbody2D arrow = arrows[i];

            // Reset velocidad
            arrow.linearVelocity = Vector2.zero;

           
            arrow.transform.position = bows[i].position;

           
            arrow.linearVelocity = Vector2.down * arrowForce;
        }
    }

    private bool TrySpawnRock()
    {
        // Si ya hay una roca viva, no hacer nada
        if (currentRock != null)
            return false;

        if (rockPrefab == null || rockNodes == null || rockNodes.Count == 0)
            return false;

        int randomIndex = Random.Range(0, rockNodes.Count);

        currentRock = Instantiate(
            rockPrefab,
            rockNodes[randomIndex].position,
            Quaternion.identity
        );

        // Detectar cuando se destruya
        StartCoroutine(CheckRockDestroyed());

        return true;
    }

    private IEnumerator CheckRockDestroyed()
    {
        // Espera hasta que el objeto sea destruido
        while (currentRock != null)
        {
            yield return null;
        }

        currentRock = null;
    }

}