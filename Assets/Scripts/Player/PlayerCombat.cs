using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    public float attackRange = 1.5f;
    public float attackRate = 1.0f;

    private float nextAttackTime = 0f;

    private PlayerMovement movement;
    private PlayerStats stats;
    private Transform currentTarget;
    private EnemyHealth targetEnemy;

    private bool isAttacking = false;   // 🔥 blokuje ruch w czasie cooldownu
    private bool autoAttackActive = false;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                EnemyHealth foundEnemy = hit.collider.GetComponent<EnemyHealth>() ??
                                         hit.collider.GetComponentInParent<EnemyHealth>() ??
                                         hit.collider.GetComponentInChildren<EnemyHealth>();

                if (foundEnemy != null)
                {
                    currentTarget = foundEnemy.transform;
                    targetEnemy = foundEnemy;

                    autoAttackActive = true;
                    isAttacking = false;

                    movement.SetMarkerAt(currentTarget.position);
                    TryApproachOrAttack();
                }
                else
                {
                    autoAttackActive = false;
                    currentTarget = null;
                    targetEnemy = null;
                }
            }
            else
            {
                autoAttackActive = false;
                currentTarget = null;
                targetEnemy = null;
            }
        }

        // 🔥 Gracz stoi i czeka cooldown → zero logiki ruchu
        if (isAttacking)
            return;

        if (autoAttackActive && targetEnemy != null && !targetEnemy.IsDead)
        {
            TryApproachOrAttack();
        }
        else
        {
            autoAttackActive = false;
            currentTarget = null;
            targetEnemy = null;
        }
    }

    private void TryApproachOrAttack()
    {
        if (currentTarget == null || targetEnemy == null)
            return;

        float dist = Vector2.Distance(movement.CurrentPosition(), currentTarget.position);

        float finalAttackRate = attackRate + stats.attributes.GetAttackSpeed();

        if (dist <= attackRange)
        {
            movement.StopMovement();

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + (1f / finalAttackRate);
            }
            else
            {
                // 🔥 Cały cooldown postać stoi
                isAttacking = true;
                Invoke(nameof(ResetAttackState), nextAttackTime - Time.time);
            }
        }
        else
        {
            // 🔥 Ruch tylko jeśli NIE jesteśmy w cooldownie
            Vector3 direction = (currentTarget.position - (Vector3)movement.CurrentPosition()).normalized;
            Vector3 desiredPos = currentTarget.position - direction * attackRange * 0.9f;
            movement.MoveTo(desiredPos);
        }

        if (!targetEnemy.IsDead)
            movement.UpdateMarkerPosition(currentTarget.position);
    }

    private void Attack()
    {
        if (targetEnemy == null || targetEnemy.IsDead)
            return;

        int damage = stats.TotalDamage;
        bool isCrit = Random.value < (stats.CritChance / 100f);

        if (isCrit)
        {
            float critMulti = stats.CritDamageMultiplier;
            damage = Mathf.RoundToInt(damage * critMulti);
            Debug.Log($"Critical Hit! {damage} dmg!");
        }
        else
        {
            Debug.Log($"Hit for {damage} dmg");
        }

        targetEnemy.TakeDamage(stats.TotalDamage);

        // 🔥 zablokuj ruch do końca cooldownu
        isAttacking = true;

        float finalAttackRate = attackRange + stats.attributes.GetAttackSpeed();
        Invoke(nameof(ResetAttackState), 1f / finalAttackRate);
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }
}
