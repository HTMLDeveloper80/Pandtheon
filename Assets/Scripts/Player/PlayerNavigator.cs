using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerNavigator : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovement movement;
    public PlayerPlatformSensor platformSensor;

    [Header("Click detect")]
    public LayerMask platformMask;
    public LayerMask ladderMask;
    public float clickRadius = 0.05f;

    [Header("Move / arrive")]
    public float arriveX = 0.12f;

    [Header("Jump")]
    public float jumpDuration = 0.6f;
    public float jumpHeight = 1.2f;
    public bool lockInputDuringSequence = true;

    [Header("Obstacles")]
    public LayerMask obstacleMask;
    public float obstaclePadding = 0.06f;

    [Header("Drop")]
    public float dropDuration = 0.25f;

    [Header("Ladder")]
    public float ladderDuration = 0.6f;

    private bool isBusy;

    private void Reset()
    {
        movement = GetComponent<PlayerMovement>();
        platformSensor = GetComponentInChildren<PlayerPlatformSensor>();
    }

    private void Update()
    {
        if (movement == null || platformSensor == null) return;
        if (isBusy && lockInputDuringSequence) return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverUI())
            return;

        Vector3 tapWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tapWorld.z = 0f;

        // 1) najpierw klik w drabinę
        Collider2D ladderHit = Physics2D.OverlapPoint(tapWorld, ladderMask);
        if (ladderHit == null)
            ladderHit = Physics2D.OverlapCircle(tapWorld, clickRadius, ladderMask);

        if (ladderHit != null)
        {
            LadderClickTarget ladder =
                ladderHit.GetComponentInParent<LadderClickTarget>() ??
                ladderHit.GetComponent<LadderClickTarget>();

            if (ladder != null)
            {
                HandleLadderClick(ladder);
                return;
            }
        }

        // 2) jak nie drabina, to normalna platforma
        Collider2D hit = Physics2D.OverlapPoint(tapWorld, platformMask);
        if (hit == null)
            hit = Physics2D.OverlapCircle(tapWorld, clickRadius, platformMask);

        if (hit == null) return;

        PlatformNode targetPlatform =
            hit.GetComponentInParent<PlatformNode>() ?? hit.GetComponent<PlatformNode>();

        if (targetPlatform == null) return;

        float clickedX = Mathf.Clamp(tapWorld.x, hit.bounds.min.x, hit.bounds.max.x);
        clickedX = ClampXByObstacles(clickedX);

        NavigateToPlatform(targetPlatform, clickedX);
    }

    public void NavigateToPlatform(PlatformNode targetPlatform, float targetX)
    {
        PlatformNode currentPlatform = platformSensor.CurrentPlatform;
        if (currentPlatform == null)
        {
            Debug.LogWarning("[Navigator] CurrentPlatform == null.");
            return;
        }

        // ta sama platforma
        if (currentPlatform == targetPlatform)
        {
            Vector3 dest = new Vector3(targetX, movement.Rigidbody.position.y, 0f);
            movement.MoveTo(dest);
            movement.SetMarkerAt(dest);
            return;
        }

        // znajdź ścieżkę przez wiele platform
        List<PlatformNode.JumpLink> path = FindPath(currentPlatform, targetPlatform);
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"[Navigator] Brak ścieżki z {currentPlatform.name} -> {targetPlatform.name}");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(FollowPath(path, targetX));
    }

    private void HandleLadderClick(LadderClickTarget ladder)
    {
        PlatformNode currentPlatform = platformSensor.CurrentPlatform;
        if (currentPlatform == null)
        {
            Debug.LogWarning("[Navigator] CurrentPlatform == null.");
            return;
        }

        if (!ladder.TryGetTargetFrom(currentPlatform, out PlatformNode targetPlatform))
        {
            Debug.LogWarning($"[Navigator] Kliknięta drabina nie pasuje. Current={currentPlatform.name}, bottom={ladder.bottomPlatform?.name}, top={ladder.topPlatform?.name}");
            return;
        }

        float targetX = ClampXByObstacles(ladder.transform.position.x);
        NavigateToPlatform(targetPlatform, targetX);
    }

    private List<PlatformNode.JumpLink> FindPath(PlatformNode start, PlatformNode goal)
    {
        if (start == null || goal == null)
            return null;

        Queue<PlatformNode> queue = new Queue<PlatformNode>();
        HashSet<PlatformNode> visited = new HashSet<PlatformNode>();
        Dictionary<PlatformNode, PlatformNode> cameFrom = new Dictionary<PlatformNode, PlatformNode>();
        Dictionary<PlatformNode, PlatformNode.JumpLink> cameByLink = new Dictionary<PlatformNode, PlatformNode.JumpLink>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            PlatformNode current = queue.Dequeue();

            if (current == goal)
                break;

            if (current.jumpLinks == null)
                continue;

            foreach (var link in current.jumpLinks)
            {
                if (link == null)
                    continue;

                if (link.toPlatform == null || link.jumpStart == null || link.jumpLand == null)
                    continue;

                PlatformNode next = link.toPlatform;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                queue.Enqueue(next);

                cameFrom[next] = current;
                cameByLink[next] = link;
            }
        }

        if (!visited.Contains(goal))
            return null;

        List<PlatformNode.JumpLink> path = new List<PlatformNode.JumpLink>();
        PlatformNode step = goal;

        while (step != start)
        {
            if (!cameByLink.TryGetValue(step, out var link))
                return null;

            path.Add(link);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }

    private IEnumerator FollowPath(List<PlatformNode.JumpLink> path, float finalTargetX)
    {
        isBusy = true;

        try
        {
            for (int i = 0; i < path.Count; i++)
            {
                bool isLast = (i == path.Count - 1);
                float xAfterThisLink = isLast ? finalTargetX : path[i].jumpLand.position.x;

                yield return StartCoroutine(JumpSequence(path[i], xAfterThisLink));
            }
        }
        finally
        {
            isBusy = false;
        }
    }

    private IEnumerator JumpSequence(PlatformNode.JumpLink link, float finalX)
    {
        float startX = link.jumpStart.position.x;

        // dojście do startu linka
        Vector3 goStart = new Vector3(startX, movement.Rigidbody.position.y, 0f);
        movement.MoveTo(goStart);
        movement.SetMarkerAt(goStart);

        float timeout = 2f;
        float t = 0f;
        float stuckTimer = 0f;
        float prevX = movement.Rigidbody.position.x;

        while (true)
        {
            t += Time.deltaTime;

            float currentX = movement.Rigidbody.position.x;
            float dx = Mathf.Abs(currentX - startX);

            if (dx <= arriveX) break;
            if (!movement.IsMoving()) break;

            if (Mathf.Abs(currentX - prevX) < 0.0005f) stuckTimer += Time.deltaTime;
            else stuckTimer = 0f;

            prevX = currentX;

            if (stuckTimer >= 0.25f) break;
            if (t >= timeout) break;

            yield return null;
        }

        movement.StopMovement();

        Rigidbody2D rb = movement.Rigidbody;
        rb.position = new Vector2(startX, rb.position.y);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        Vector3 land = link.jumpLand.position;

        if (link.dropOnly)
        {
            yield return StartCoroutine(DropFromEdge(link, finalX));
            yield break;
        }

        if (link.useLadder)
        {
            yield return StartCoroutine(LadderClimb(link));
            rb.position = new Vector2(rb.position.x, rb.position.y + 0.02f);
        }
        else
        {
            yield return StartCoroutine(JumpArc(land));
            rb.position = new Vector2(rb.position.x, rb.position.y + 0.02f);
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        Vector3 final = new Vector3(finalX, rb.position.y, 0f);
        movement.MoveTo(final);
        movement.SetMarkerAt(final);
    }

    private IEnumerator JumpArc(Vector3 landPoint)
    {
        Rigidbody2D rb = movement.Rigidbody;
        Vector2 from = rb.position;
        Vector2 to = new Vector2(landPoint.x, landPoint.y);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            float tt = Mathf.Clamp01(t);

            Vector2 pos = Vector2.Lerp(from, to, tt);
            float baseHeight = Mathf.Abs(to.y - from.y);
            float dynamicHeight = baseHeight + jumpHeight;
            float hump = 4f * dynamicHeight * tt * (1f - tt);
            pos.y += hump;

            rb.MovePosition(pos);
            rb.rotation = 0f;

            yield return null;
        }

        rb.MovePosition(to);
        rb.rotation = 0f;
    }

    private IEnumerator LadderClimb(PlatformNode.JumpLink link)
    {
        Rigidbody2D rb = movement.Rigidbody;
        Collider2D playerCol = GetComponent<Collider2D>();
        Collider2D sensorCol = platformSensor != null ? platformSensor.GetComponent<Collider2D>() : null;

        bool oldPlayerEnabled = playerCol != null && playerCol.enabled;
        bool oldSensorEnabled = sensorCol != null && sensorCol.enabled;

        if (platformSensor != null)
            platformSensor.suspendDetection = true;

        if (playerCol != null)
            playerCol.enabled = false;

        if (sensorCol != null)
            sensorCol.enabled = false;

        try
        {
            Vector2 from = new Vector2(link.jumpStart.position.x, rb.position.y);
            Vector2 to = new Vector2(link.jumpLand.position.x, link.jumpLand.position.y);

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.rotation = 0f;
            rb.position = from;

            float duration = Mathf.Max(0.01f, ladderDuration);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float tt = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

                float x = Mathf.Lerp(from.x, to.x, tt);
                float y = Mathf.Lerp(from.y, to.y, tt);

                rb.position = new Vector2(x, y);
                yield return null;
            }

            rb.position = to;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.rotation = 0f;
        }
        finally
        {
            if (playerCol != null)
                playerCol.enabled = oldPlayerEnabled;

            if (sensorCol != null)
                sensorCol.enabled = oldSensorEnabled;

            if (platformSensor != null)
            {
                platformSensor.ForcePlatform(link.toPlatform);
                platformSensor.suspendDetection = false;
            }
        }
    }

    private IEnumerator DropFromEdge(PlatformNode.JumpLink link, float finalX)
    {
        Rigidbody2D rb = movement.Rigidbody;

        // 1) wyjdź minimalnie poza krawędź
        if (link.dropExitPoint != null)
        {
            Vector3 exit = link.dropExitPoint.position;
            movement.MoveTo(new Vector3(exit.x, rb.position.y, 0f));
            movement.SetMarkerAt(new Vector3(exit.x, rb.position.y, 0f));

            float timeout = 1f;
            float t = 0f;

            while (movement.IsMoving())
            {
                t += Time.deltaTime;

                if (Mathf.Abs(rb.position.x - exit.x) <= arriveX)
                    break;

                if (t >= timeout)
                    break;

                yield return null;
            }

            movement.StopMovement();
            rb.position = new Vector2(link.dropExitPoint.position.x, rb.position.y);
        }

        // 2) spadanie tylko z fizyki
        PlatformNode startPlatform = platformSensor.CurrentPlatform;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 3f;

        float timeoutLeave = 1f;
        float leaveTimer = 0f;

        while (platformSensor.CurrentPlatform == startPlatform && leaveTimer < timeoutLeave)
        {
            leaveTimer += Time.deltaTime;
            rb.rotation = 0f;
            yield return null;
        }

        float timeoutLand = 2f;
        float landTimer = 0f;

        while ((platformSensor.CurrentPlatform == null || platformSensor.CurrentPlatform == startPlatform) && landTimer < timeoutLand)
        {
            landTimer += Time.deltaTime;
            rb.rotation = 0f;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        Vector3 final = new Vector3(finalX, rb.position.y, 0f);
        movement.MoveTo(final);
        movement.SetMarkerAt(final);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private float ClampXByObstacles(float desiredX)
    {
        float y = movement.Rigidbody.position.y;
        Vector2 from = new Vector2(movement.Rigidbody.position.x, y);
        Vector2 to = new Vector2(desiredX, y);

        float dir = Mathf.Sign(to.x - from.x);
        if (Mathf.Abs(to.x - from.x) < 0.001f)
            return desiredX;

        float dist = Mathf.Abs(to.x - from.x);
        RaycastHit2D hit = Physics2D.Raycast(from, new Vector2(dir, 0f), dist, obstacleMask);

        if (hit.collider == null)
            return desiredX;

        float stopX = hit.point.x - dir * obstaclePadding;
        return stopX;
    }
}