using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private float attackReachY = 1.25f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Slash effect")]
    [SerializeField] private float slashOriginOffsetY = 0.15f;
    [SerializeField] private float slashDuration = 0.16f;
    [SerializeField] private float slashWidth = 0.08f;
    [SerializeField, Range(6, 40)] private int slashSegments = 18;
    [SerializeField] private Color slashColor = Color.white;
    [SerializeField] private int slashSortingOrder = 25;

    private static Material sharedSlashMaterial;

    private readonly HashSet<EnemyHealth> enemiesHitThisAttack =
        new HashSet<EnemyHealth>();

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

        if (enemyMask.value == 0)
            enemyMask = LayerMask.GetMask("Enemy");
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
        if (movement == null || stats == null ||
            currentTarget == null || targetEnemy == null)
        {
            return;
        }

        Vector2 playerPos = movement.CurrentPosition();
        Vector2 targetPos = currentTarget.position;

        float horizontalDistance = Mathf.Abs(targetPos.x - playerPos.x);
        float verticalDistance = Mathf.Abs(targetPos.y - playerPos.y);

        if (horizontalDistance <= attackRange &&
            verticalDistance <= attackReachY)
        {
            movement.StopMovement(false);
            FaceTarget(targetPos.x - playerPos.x);

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
                direction = GetFacingDirection();

            Vector3 desiredPos = new Vector3(
                targetPos.x - direction * attackRange * 0.85f,
                playerPos.y,
                0f);

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

        float facingDirection = GetFacingDirection();
        Vector2 attackOrigin = (Vector2)transform.position +
            Vector2.up * slashOriginOffsetY;

        DamageEnemiesInSlash(attackOrigin, facingDirection, damage);
        StartCoroutine(ShowSlashEffect(attackOrigin, facingDirection));
    }

    private void DamageEnemiesInSlash(
        Vector2 attackOrigin,
        float facingDirection,
        int damage)
    {
        Vector2 boxCenter = attackOrigin +
            Vector2.right * facingDirection * attackRange * 0.5f;

        Vector2 boxSize = new Vector2(
            Mathf.Max(0.01f, attackRange),
            Mathf.Max(0.01f, attackReachY * 2f));

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCenter,
            boxSize,
            0f,
            enemyMask);

        enemiesHitThisAttack.Clear();

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>() ??
                hit.GetComponent<EnemyHealth>() ??
                hit.GetComponentInChildren<EnemyHealth>();

            if (enemy == null || enemy.IsDead ||
                !enemiesHitThisAttack.Add(enemy))
            {
                continue;
            }

            Vector2 enemyPosition = enemy.transform.position;
            Vector2 fromOrigin = enemyPosition - attackOrigin;

            float forwardDistance = fromOrigin.x * facingDirection;
            if (forwardDistance < -0.01f || forwardDistance > attackRange)
                continue;

            float normalizedX = forwardDistance / Mathf.Max(attackRange, 0.01f);
            float normalizedY = fromOrigin.y / Mathf.Max(attackReachY, 0.01f);

            if (normalizedX * normalizedX +
                normalizedY * normalizedY > 1f)
            {
                continue;
            }

            enemy.TakeDamage(damage);
        }
    }

    private IEnumerator ShowSlashEffect(
        Vector2 attackOrigin,
        float facingDirection)
    {
        GameObject slashObject = new GameObject("PlayerSlash");
        LineRenderer line = slashObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = slashSegments + 1;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.sortingOrder = slashSortingOrder;
        line.sharedMaterial = GetSlashMaterial();
        line.textureMode = LineTextureMode.Stretch;
        line.widthMultiplier = slashWidth;
        line.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0.15f),
            new Keyframe(0.2f, 0.75f),
            new Keyframe(0.5f, 1f),
            new Keyframe(0.8f, 0.75f),
            new Keyframe(1f, 0.15f));

        for (int i = 0; i <= slashSegments; i++)
        {
            float progress = (float)i / slashSegments;
            float angle = Mathf.Lerp(-90f, 90f, progress) * Mathf.Deg2Rad;

            Vector3 point = attackOrigin + new Vector2(
                Mathf.Cos(angle) * attackRange * facingDirection,
                Mathf.Sin(angle) * attackReachY);

            point.z = transform.position.z - 0.1f;
            line.SetPosition(i, point);
        }

        float elapsed = 0f;
        while (elapsed < slashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / slashDuration);
            Color fadedColor = slashColor;
            fadedColor.a *= alpha;

            line.startColor = fadedColor;
            line.endColor = fadedColor;

            yield return null;
        }

        Destroy(slashObject);
    }

    private static Material GetSlashMaterial()
    {
        if (sharedSlashMaterial != null)
            return sharedSlashMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("UI/Default");

        sharedSlashMaterial = new Material(shader)
        {
            name = "RuntimePlayerSlashMaterial"
        };

        return sharedSlashMaterial;
    }

    private void FaceTarget(float horizontalDifference)
    {
        if (Mathf.Abs(horizontalDifference) < 0.01f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(horizontalDifference) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private float GetFacingDirection()
    {
        return transform.localScale.x >= 0f ? 1f : -1f;
    }

    private void ClearTarget()
    {
        autoAttackActive = false;
        currentTarget = null;
        targetEnemy = null;

        if (movement != null)
            movement.ClearMarker();
    }

    private void OnDrawGizmosSelected()
    {
        float facingDirection = GetFacingDirection();
        Vector2 origin = (Vector2)transform.position +
            Vector2.up * slashOriginOffsetY;

        Gizmos.color = new Color(1f, 1f, 1f, 0.8f);

        Vector3 previousPoint = origin + new Vector2(
            0f,
            -attackReachY);

        const int previewSegments = 24;
        for (int i = 1; i <= previewSegments; i++)
        {
            float progress = (float)i / previewSegments;
            float angle = Mathf.Lerp(-90f, 90f, progress) * Mathf.Deg2Rad;

            Vector3 point = origin + new Vector2(
                Mathf.Cos(angle) * attackRange * facingDirection,
                Mathf.Sin(angle) * attackReachY);

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(
            origin + Vector2.down * attackReachY,
            origin + Vector2.up * attackReachY);
    }
}
