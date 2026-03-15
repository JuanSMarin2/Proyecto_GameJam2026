using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalBossManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private List<Transform> nodes;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Enrage")]
    [SerializeField] private int enrageHealthThreshold = 4;
    [SerializeField] private float enragedMoveSpeed = 7f;

    [Header("State")]
    [SerializeField] private bool isActive = false;

    [Header("Health")]
    [SerializeField] private int maxBossHealth = 10;
    [SerializeField] private Image healthBar;
    [SerializeField] private float damageFlashDuration = 0.15f;

    [Header("References")]
    [SerializeField] private BossAttacks bossAttacks;
    [SerializeField] private GameObject victoryObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private int currentBossHealth;
    private Transform currentTarget;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float baseMoveSpeed;

    private void ApplyDifficultySettings()
    {
        if (DifficultySelectManager.Instance == null)
            return;

        DifficultySelectManager.BossDifficultySettings settings = DifficultySelectManager.Instance.GetCurrentBossSettings();
        if (settings == null)
            return;

        maxBossHealth = Mathf.Max(1, settings.bossHealth);

        if (bossAttacks != null)
            bossAttacks.SetBaseLaunchForce(settings.bossLaunchForce);
    }

    private void Start()
    {
        ApplyDifficultySettings();
        currentBossHealth = maxBossHealth;

        baseMoveSpeed = moveSpeed;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (animator == null)
            animator = GetComponent<Animator>();

        UpdateHealthBar();

        UpdateEnrageState();

        if (isActive)
        {
            StartCoroutine(BossRoutine());
        }
    }

    private void TriggerAnim(string triggerName)
    {
        if (animator == null)
            return;

        animator.SetTrigger(triggerName);
    }

    #region MOVEMENT

    private IEnumerator BossRoutine()
    {
        while (isActive)
        {
            SelectRandomNode();

            if (!isActive)
                yield break;

            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > 0.1f)
                TriggerAnim("Movimiento");

            while (Vector3.Distance(transform.position, currentTarget.position) > 0.1f)
            {
                if (!isActive)
                    yield break;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget.position,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            // One call per movement: BossAttacks handles Rock cumulative chance and other attack chance.
            if (!isActive)
                yield break;

            TriggerAnim("Ataque");
            bossAttacks.LaunchAttack();

            float waitTime = Random.Range(3f, 5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SelectRandomNode()
    {
        if (nodes.Count == 0) return;

        int randomIndex = Random.Range(0, nodes.Count);
        currentTarget = nodes[randomIndex];
    }

    #endregion

    #region DAMAGE

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
            return;

        if (other.CompareTag("Box"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    private void TakeDamage(int amount)
    {
        currentBossHealth -= amount;
        currentBossHealth = Mathf.Clamp(currentBossHealth, 0, maxBossHealth);

        TriggerAnim("Herida");

        UpdateHealthBar();
        UpdateEnrageState();
        StartCoroutine(DamageFlash());

        if (currentBossHealth <= 0)
        {
            BossDefeat();
        }
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.fillAmount = (float)currentBossHealth / maxBossHealth;
    }

    private void BossDefeat()
    {
        TriggerAnim("Muerte");
        isActive = false;

        if (bossAttacks != null)
            bossAttacks.SetCanAttack(false);

        StartCoroutine(WaitForScene());


    }

    private void UpdateEnrageState()
    {
        bool enraged = currentBossHealth <= enrageHealthThreshold;
        moveSpeed = enraged ? enragedMoveSpeed : baseMoveSpeed;

        if (bossAttacks != null)
            bossAttacks.SetEnraged(enraged);
    }

    private IEnumerator WaitForScene()
    {

        yield return new WaitForSeconds(3f);
        victoryObject.SetActive(true);
    }

    #endregion
}