using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRate = 1.0f; // ataki / sekunda

    private float nextAttackTime = 0f;

    private PlayerMovement movement;
    private PlayerStats stats;

    private Transform currentTarget;
    private EnemyHealth targetEnemy;

    private bool autoAttackActive = false;
    private bool isAttackCooldown = false;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!autoAttackActive) return;
        if (targetEnemy == null || targetEnemy.IsDead)
        {
            ClearTarget();
            return;
        }

        if (isAttackCooldown) return;

        TryApproachOrAttack();
    }

    /// <summary>
    /// Wywołuj z zewnętrznego click-routera po kliknięciu enemy.
    /// </summary>
    public void SetTarget(EnemyHealth enemy)
    {
        if (enemy == null || enemy.IsDead)
        {
            ClearTarget();
            return;
        }

        currentTarget = enemy.transform;
        targetEnemy = enemy;
        autoAttackActive = true;
        isAttackCooldown = false;

        if (movement != null)
            movement.SetMarkerAt(currentTarget.position);

        TryApproachOrAttack();
    }

    public void CancelCombat()
    {
        ClearTarget();
    }

    private void TryApproachOrAttack()
    {
        if (movement == null || stats == null || currentTarget == null || targetEnemy == null) return;

        float dist = Vector2.Distance(movement.CurrentPosition(), currentTarget.position);
        float finalAttackRate = Mathf.Max(0.01f, attackRate + stats.attributes.GetAttackSpeed());

        if (dist <= attackRange)
        {
            movement.StopMovement();

            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                nextAttackTime = Time.time + (1f / finalAttackRate);

                isAttackCooldown = true;
                Invoke(nameof(ResetAttackCooldown), 1f / finalAttackRate);
            }
        }
        else
        {
            Vector3 direction = (currentTarget.position - (Vector3)movement.CurrentPosition()).normalized;
            Vector3 desiredPos = currentTarget.position - direction * attackRange * 0.9f;
            movement.MoveTo(desiredPos);
        }

        if (!targetEnemy.IsDead)
            movement.UpdateMarkerPosition(currentTarget.position);
    }

    private void PerformAttack()
    {
        if (targetEnemy == null || targetEnemy.IsDead || stats == null) return;

        int damage = stats.TotalDamage;
        bool isCrit = Random.value < (stats.CritChance / 100f);

        if (isCrit)
            damage = Mathf.RoundToInt(damage * stats.CritDamageMultiplier);

        targetEnemy.TakeDamage(damage);
    }

    private void ResetAttackCooldown()
    {
        isAttackCooldown = false;
    }

    private void ClearTarget()
    {
        autoAttackActive = false;
        currentTarget = null;
        targetEnemy = null;
        isAttackCooldown = false;

        CancelInvoke(nameof(ResetAttackCooldown));

        if (movement != null)
            movement.ClearMarker();
    }
}