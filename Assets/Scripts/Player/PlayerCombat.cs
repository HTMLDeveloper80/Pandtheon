using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private float attackReachY = 1.25f;

    private PlayerMovement movement;
    private PlayerStats stats;

    private Transform currentTarget;
    private EnemyHealth targetEnemy;

    private bool autoAttackActive;
    private float nextAttackTime;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!autoAttackActive)
            return;

        if (targetEnemy == null || targetEnemy.IsDead)
        {
            ClearTarget();
            return;
        }

        TryApproachOrAttack();
    }

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
        if (movement == null || stats == null || currentTarget == null || targetEnemy == null)
            return;

        Vector2 playerPos = movement.CurrentPosition();
        Vector2 targetPos = currentTarget.position;

        float horizontalDistance = Mathf.Abs(targetPos.x - playerPos.x);
        float verticalDistance = Mathf.Abs(targetPos.y - playerPos.y);

        if (horizontalDistance <= attackRange && verticalDistance <= attackReachY)
        {
            movement.StopMovement(false);

            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                nextAttackTime = Time.time + GetAttackInterval();
            }
        }
        else
        {
            float direction = Mathf.Sign(targetPos.x - playerPos.x);
            if (Mathf.Abs(direction) < 0.01f)
                direction = transform.localScale.x >= 0f ? 1f : -1f;

            Vector3 desiredPos = new Vector3(targetPos.x - direction * attackRange * 0.85f, playerPos.y, 0f);
            movement.MoveTo(desiredPos);
        }

        if (!targetEnemy.IsDead)
            movement.UpdateMarkerPosition(currentTarget.position);
    }

    private float GetAttackInterval()
    {
        float finalAttackRate = attackRate;
        if (stats != null)
            finalAttackRate += stats.attributes.GetAttackSpeed();

        return 1f / Mathf.Max(0.01f, finalAttackRate);
    }

    private void PerformAttack()
    {
        if (targetEnemy == null || targetEnemy.IsDead || stats == null)
            return;

        int damage = stats.TotalDamage;
        bool isCrit = Random.value < stats.CritChance / 100f;

        if (isCrit)
            damage = Mathf.RoundToInt(damage * stats.CritDamageMultiplier);

        targetEnemy.TakeDamage(damage);
    }

    private void ClearTarget()
    {
        autoAttackActive = false;
        currentTarget = null;
        targetEnemy = null;

        if (movement != null)
            movement.ClearMarker();
    }
}
