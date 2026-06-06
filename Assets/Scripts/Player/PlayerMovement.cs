using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float stopDistance = 0.08f;      // jak blisko celu uznajemy, że doszliśmy
    public bool faceMoveDirection = true;

    [Header("Input (optional)")]
    public bool enableTapToMove = false;     // <-- zostaw false, bo klik ogarnie PlayerNavigator
    public float clickRadius = 0.35f;
    public float standOffsetY = 0.33f;       // wysokość "stania" nad platformą

    [Header("Marker")]
    public GameObject targetMarkerPrefab;
    public float markerOffsetY = 0.2f;
    public bool destroyMarkerOnArrive = true;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving;

    private GameObject currentMarker;

    [Header("Anti-stuck")]
    public float stuckTimeToStop = 0.25f;
    public float stuckMinDeltaX = 0.0005f;

    private float stuckTimer = 0f;
    private float lastX;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }


    private void Start()
    {
        targetPosition = rb.position;
        isMoving = false;
        lastX = rb.position.x;
    }

    private void Update()
    {
        // Klik-move tylko jeśli świadomie włączysz
        if (!enableTapToMove)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return;

            Vector3 tapWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tapWorld.z = 0f;

            Vector2 tapPos2D = new Vector2(tapWorld.x, tapWorld.y);
            Collider2D hit = Physics2D.OverlapCircle(tapPos2D, clickRadius);

            if (hit == null)
                return;

            // Jeśli klikniesz w platformę -> cel na górnej krawędzi + offset
            // Jeśli w cokolwiek innego -> po prostu idź w punkt kliknięcia (możesz to ograniczyć warstwami później)
            float targetX = tapWorld.x;
            float targetY = tapWorld.y;

            // Jeżeli masz platformy z colliderem: ustaw "na górze" bounds
            if (hit.bounds.size.y > 0.01f)
            {
                targetX = Mathf.Clamp(tapWorld.x, hit.bounds.min.x, hit.bounds.max.x);
                targetY = hit.bounds.max.y + standOffsetY;
            }

            MoveTo(new Vector3(targetX, targetY, 0f));
            SetMarkerAt(new Vector3(targetX, targetY, 0f));
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        Vector2 current = rb.position;
        Vector2 next = Vector2.MoveTowards(current, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        // --- Anti-stuck: jeśli collider blokuje ruch, po chwili przestań iść ---
        float deltaX = Mathf.Abs(rb.position.x - lastX);
        if (deltaX < stuckMinDeltaX)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        lastX = rb.position.x;

        if (stuckTimer >= stuckTimeToStop)
        {
            isMoving = false;
            stuckTimer = 0f;

            if (destroyMarkerOnArrive)
                ClearMarker();
        }


        if (faceMoveDirection)
        {
            float dx = targetPosition.x - current.x;
            if (Mathf.Abs(dx) > 0.001f)
                transform.localScale = new Vector3(Mathf.Sign(dx) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        float dist = Vector2.Distance(next, targetPosition);
        if (dist <= stopDistance)
        {
            isMoving = false;

            if (destroyMarkerOnArrive)
                ClearMarker();
        }
    }

    // ===== API używane przez inne skrypty (PlayerCombat / PlayerNavigator) =====

    public void MoveTo(Vector3 worldPos)
    {
        targetPosition = new Vector2(worldPos.x, rb.position.y);
        isMoving = true;

        stuckTimer = 0f;
        lastX = rb.position.x;
    }



    public void StopMovement(bool clearMarker = true)
    {
        isMoving = false;
        stuckTimer = 0f;

        if (clearMarker && destroyMarkerOnArrive)
            ClearMarker();
    }


    public bool IsMoving()
    {
        return isMoving;
    }

    public Vector2 CurrentPosition()
    {
        return rb.position;
    }

    public Rigidbody2D Rigidbody => rb;

    public void SetMarkerAt(Vector3 worldPos)
    {
        if (targetMarkerPrefab == null)
            return;

        ClearMarker();

        Vector3 markerPos = new Vector3(worldPos.x, worldPos.y + markerOffsetY, 0f);
        currentMarker = Instantiate(targetMarkerPrefab, markerPos, Quaternion.identity);
    }

    public void UpdateMarkerPosition(Vector3 worldPos)
    {
        if (currentMarker == null)
            return;

        currentMarker.transform.position = new Vector3(worldPos.x, worldPos.y + markerOffsetY, 0f);
    }

    public void ClearMarker()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
