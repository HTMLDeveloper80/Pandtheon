using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float chaseSpeedMultiplier = 1.25f;

    [Header("Ground checks")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float wallCheckDistance = 0.2f;

    [Header("Patrol")]
    [SerializeField] private bool keepNearSpawn = true;
    [SerializeField] private float patrolRadius = 3f;
    public Vector2 moveDurationRange = new Vector2(2f, 5f);
    public Vector2 idleDurationRange = new Vector2(1f, 3f);
    public float chanceDirectionChance = 0.5f;

    [Header("Chase")]
    [SerializeField] private bool chasePlayer = true;
    [SerializeField] private float detectRange = 4f;
    [SerializeField] private float loseRange = 6f;
    [SerializeField] private float stopDistanceFromPlayer = 0.75f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Contact damage")]
    public int contactDamage = 1;
    [SerializeField] private bool useEnemyDataDamage = true;
    [SerializeField] private float contactDamageCooldown = 1f;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        bodyCollider = GetComponent<Collider2D>();

        rb.freezeRotation = true;

        if (obstacleLayer.value == 0)
            obstacleLayer = groundLayer;
    }

    private void Start()
    {
        spawnPosition = rb.position;
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
            return;
        }

        if (isMoving)
            Patrol();
    }

    private void UpdateTarget()
    {
        if (!chasePlayer)
        {
            playerTarget = null;
            return;
        }

        if (playerTarget != null)
        {
            float distance = Vector2.Distance(transform.position, playerTarget.position);
            if (distance > loseRange)
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
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);
            if (hit != null)
                return hit.GetComponentInParent<PlayerStats>();
        }

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player == null)
            return null;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= detectRange ? player : null;
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

        Move(direction, moveSpeed);
    }

    private void ChasePlayer()
    {
        if (playerTarget == null)
            return;

        float dx = playerTarget.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= stopDistanceFromPlayer)
            return;

        int direction = dx > 0f ? 1 : -1;
        FaceDirection(direction);

        if (!CanMove(direction))
            return;

        Move(direction, moveSpeed * chaseSpeedMultiplier);
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

        RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, groundCheckDistance, groundLayer);
        if (groundHit.collider == null)
            return false;

        Vector2 wallOrigin = bodyCollider != null ? bodyCollider.bounds.center : transform.position;
        LayerMask wallMask = obstacleLayer.value != 0 ? obstacleLayer : groundLayer;
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * direction, wallCheckDistance, wallMask);
        return wallHit.collider == null;
    }

    private bool IsLeavingPatrolArea(int direction)
    {
        if (!keepNearSpawn || patrolRadius <= 0f)
            return false;

        float nextX = rb.position.x + direction * moveSpeed * Time.fixedDeltaTime;
        return Mathf.Abs(nextX - spawnPosition.x) > patrolRadius;
    }

    private void PickNewPatrolAction()
    {
        actionTimer = 0f;
        isMoving = Random.value < 0.65f;

        if (isMoving)
        {
            currentActionTime = Random.Range(moveDurationRange.x, moveDurationRange.y);

            if (Random.value < chanceDirectionChance)
                Flip();
        }
        else
        {
            currentActionTime = Random.Range(idleDurationRange.x, idleDurationRange.y);
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
        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null)
            return;

        if (Time.time < nextContactDamageTime)
            return;

        int damage = contactDamage;
        if (useEnemyDataDamage && stats != null && stats.Data != null && stats.Data.damage > 0)
            damage = stats.Data.damage;

        player.TakeDamage(damage);
        nextContactDamageTime = Time.time + Mathf.Max(0.05f, contactDamageCooldown);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        Gizmos.color = Color.cyan;
        Vector3 center = Application.isPlaying ? (Vector3)spawnPosition : transform.position;
        Gizmos.DrawLine(center + Vector3.left * patrolRadius, center + Vector3.right * patrolRadius);
    }
}
