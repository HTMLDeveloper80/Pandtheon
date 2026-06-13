using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private EnemyStats stats;
    private Collider2D bodyCollider;
    private Transform playerTarget;
    private Vector2 spawnPosition;

    private bool movingRight = true;
    private bool isMoving;
    private float currentActionTime;
    private float actionTimer;
    private float nextContactDamageTime;

    private EnemyData Data => stats != null ? stats.Data : null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        bodyCollider = GetComponent<Collider2D>();

        rb.freezeRotation = true;

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Platform");

        if (obstacleLayer.value == 0)
            obstacleLayer = groundLayer;

        if (playerLayer.value == 0)
            playerLayer = LayerMask.GetMask("Player");
    }

    private void Start()
    {
        if (Data == null)
        {
            Debug.LogError($"{name}: EnemyMovement requires EnemyData from EnemyRespawner.");
            enabled = false;
            return;
        }

        spawnPosition = rb.position;
        FaceDirection(Data.startsMovingRight ? 1 : -1);
        PickNewPatrolAction();
    }

    private void Update()
    {
        UpdateTarget();

        if (playerTarget != null)
            return;

        actionTimer += Time.deltaTime;
        if (actionTimer >= currentActionTime)
            PickNewPatrolAction();
    }

    private void FixedUpdate()
    {
        if (playerTarget != null)
        {
            ChasePlayer();
            TryDealContactDamageToTarget();
            return;
        }

        if (isMoving)
            Patrol();
    }

    private void UpdateTarget()
    {
        if (!Data.chasePlayer)
        {
            playerTarget = null;
            return;
        }

        if (playerTarget != null)
        {
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            if (distance > Data.loseRange)
                playerTarget = null;

            return;
        }

        PlayerStats player = FindPlayerInRange();
        if (player != null)
            playerTarget = player.transform;
    }

    private PlayerStats FindPlayerInRange()
    {
        if (playerLayer.value != 0)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, Data.detectRange, playerLayer);
            if (hit != null)
                return hit.GetComponentInParent<PlayerStats>();
        }

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player == null)
            return null;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= Data.detectRange ? player : null;
    }

    private void Patrol()
    {
        int direction = movingRight ? 1 : -1;

        if (!CanMove(direction) || IsLeavingPatrolArea(direction))
        {
            Flip();
            PickNewPatrolAction();
            return;
        }

        Move(direction, Data.moveSpeed);
    }

    private void ChasePlayer()
    {
        if (playerTarget == null)
            return;

        float dx = playerTarget.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= Data.stopDistanceFromPlayer)
            return;

        int direction = dx > 0f ? 1 : -1;
        FaceDirection(direction);

        if (!CanMove(direction))
            return;

        Move(direction, Data.moveSpeed * Data.chaseSpeedMultiplier);
    }

    private void Move(int direction, float speed)
    {
        Vector2 next = rb.position + new Vector2(direction * speed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(next);
    }

    private bool CanMove(int direction)
    {
        Vector2 groundOrigin = groundCheck != null
            ? groundCheck.position
            : transform.position + new Vector3(direction * 0.45f, -0.2f, 0f);

        RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, Data.groundCheckDistance, groundLayer);
        if (groundHit.collider == null)
            return false;

        Vector2 wallOrigin = bodyCollider != null ? bodyCollider.bounds.center : transform.position;
        LayerMask wallMask = obstacleLayer.value != 0 ? obstacleLayer : groundLayer;
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * direction, Data.wallCheckDistance, wallMask);
        return wallHit.collider == null;
    }

    private bool IsLeavingPatrolArea(int direction)
    {
        if (!Data.keepNearSpawn || Data.patrolRadius <= 0f)
            return false;

        float nextX = rb.position.x + direction * Data.moveSpeed * Time.fixedDeltaTime;
        return Mathf.Abs(nextX - spawnPosition.x) > Data.patrolRadius;
    }

    private void PickNewPatrolAction()
    {
        actionTimer = 0f;
        isMoving = Random.value < Data.patrolMoveChance;

        if (isMoving)
        {
            currentActionTime = Random.Range(Data.moveDurationRange.x, Data.moveDurationRange.y);

            if (Random.value < Data.directionChangeChance)
                Flip();
        }
        else
        {
            currentActionTime = Random.Range(Data.idleDurationRange.x, Data.idleDurationRange.y);
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        FaceDirection(movingRight ? 1 : -1);
    }

    private void FaceDirection(int direction)
    {
        if (direction == 0)
            return;

        movingRight = direction > 0;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (movingRight ? 1f : -1f);
        transform.localScale = localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void TryDealContactDamage(Collider2D other)
    {
        if (Data == null)
            return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null)
            return;

        TryDealContactDamage(player);
    }

    private void TryDealContactDamageToTarget()
    {
        if (Data == null || playerTarget == null || bodyCollider == null)
            return;

        PlayerStats player = playerTarget.GetComponent<PlayerStats>();
        if (player == null)
            player = playerTarget.GetComponentInParent<PlayerStats>();

        if (player == null)
            return;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
            return;

        ColliderDistance2D distance = bodyCollider.Distance(playerCollider);
        if (!distance.isOverlapped)
            return;

        TryDealContactDamage(player);
    }

    private void TryDealContactDamage(PlayerStats player)
    {
        if (Time.time < nextContactDamageTime)
            return;

        player.TakeDamage(Data.damage);
        nextContactDamageTime = Time.time + Data.contactDamageCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        EnemyStats enemyStats = stats != null ? stats : GetComponent<EnemyStats>();
        EnemyData data = enemyStats != null ? enemyStats.Data : null;
        if (data == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.loseRange);

        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? (Vector3)spawnPosition : transform.position;
        Gizmos.DrawLine(center + Vector3.left * data.patrolRadius, center + Vector3.right * data.patrolRadius);
    }
}
