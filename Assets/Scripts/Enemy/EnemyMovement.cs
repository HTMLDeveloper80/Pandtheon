using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Ruch")]
    private float moveSpeed = 1f;
    public Transform groundCheck;          
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;          

    private bool movingRight = true;
    private bool isMoving = false;
    private float currentActionTime = 0f;
    private float actionTimer = 0f;

    [Header("Random behaviour")]
    public Vector2 moveDurationRange = new Vector2(2f, 5f);
    public Vector2 idleDurationRange = new Vector2(2f, 5f);
    public float chanceDirectionChance = 0.5f;

    [Header("Atak")]
    public int contactDamage = 1;

    private void Start()
    {
        PickNewAction();
    }

    void Update()
    {
        actionTimer += Time.deltaTime;

        if (actionTimer >= currentActionTime)
        {
            PickNewAction();
        }

        if (isMoving)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * (movingRight ? 1 : -1));

            RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

            if (groundInfo.collider == false)
            {
                Flip();
                PickNewAction();
            }
        }
    }

    private void PickNewAction()
    {
        actionTimer = 0f;

        bool willMove = Random.value < 0.5f;

        if (willMove)
        {
            isMoving = true;
            currentActionTime = Random.Range(moveDurationRange.x, moveDurationRange.y);

            if (Random.value < chanceDirectionChance)
            {
                Flip();
            }
        }
        else
        {
            isMoving = false;
            currentActionTime = Random.Range(idleDurationRange.x, idleDurationRange.y);
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;  // obrót sprite’a
        transform.localScale = localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger zadzia³a³ z: " + collision.name);

        PlayerStats ps = collision.GetComponent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(contactDamage);
        }
    }
}
