using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerClickController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerNavigator navigator;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerWallet playerWallet;

    [Header("Layer masks")]
    [SerializeField] private LayerMask npcMask;
    [SerializeField] private LayerMask ladderMask;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private LayerMask platformMask;
    [SerializeField] private LayerMask mapTransitionMask;

    [Header("Click")]
    [SerializeField] private float clickRadius = 0.08f;

    private Camera cachedCamera;

    private void Awake()
    {
        cachedCamera = Camera.main;

        if (navigator == null) navigator = GetComponent<PlayerNavigator>();
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerWallet == null) playerWallet = GetComponent<PlayerWallet>();
    }

    private void Update()
    {
        if (navigator != null && navigator.IsBusy) return;

        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (cachedCamera == null) cachedCamera = Camera.main;
        if (cachedCamera == null) return;

        Vector3 world3 = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
        world3.z = 0f;
        Vector2 world = world3;

        // 1) NPC
        if (TryNpc(world)) return;

        // 2) LADDER (priorytet nad platform¹)
        if (TryLadder(world)) return;

        // 2.5) MAP TRANSITION SIGN
        if (TryMapTransition(world)) return;

        // 3) ENEMY
        if (TryEnemy(world)) return;

        // 4) PICKUP
        if (TryPickup(world)) return;

        // 5) PLATFORM MOVE
        TryPlatformMove(world);
    }

    private bool TryNpc(Vector2 world)
    {
        Collider2D hit = FindClickable(world, npcMask);
        if (hit == null) return false;

        var npc = hit.GetComponentInParent<NpcQuestGiver>() ?? hit.GetComponent<NpcQuestGiver>();
        if (npc == null) return false;

        if (combat != null) combat.CancelCombat();
        npc.Interact(playerStats, playerWallet);
        return true;
    }

    private bool TryLadder(Vector2 world)
    {
        Collider2D hit = FindClickable(world, ladderMask);
        if (hit == null) return false;

        LadderClickTarget ladder =
            hit.GetComponentInParent<LadderClickTarget>() ??
            hit.GetComponent<LadderClickTarget>();

        if (ladder == null || navigator == null) return false;

        if (combat != null) combat.CancelCombat();
        navigator.NavigateViaLadder(ladder);
        return true;
    }

    private bool TryEnemy(Vector2 world)
    {
        Collider2D hit = FindClickable(world, enemyMask);
        if (hit == null) return false;

        EnemyHealth enemy =
            hit.GetComponentInParent<EnemyHealth>() ??
            hit.GetComponent<EnemyHealth>() ??
            hit.GetComponentInChildren<EnemyHealth>();

        if (enemy == null || combat == null) return false;

        combat.SetTarget(enemy);
        return true;
    }

    private bool TryPickup(Vector2 world)
    {
        Collider2D hit = FindClickable(world, pickupMask);
        if (hit == null) return false;

        PickupItem pickup =
            hit.GetComponentInParent<PickupItem>() ??
            hit.GetComponent<PickupItem>() ??
            hit.GetComponentInChildren<PickupItem>();

        if (pickup == null) return false;

        if (combat != null) combat.CancelCombat();
        pickup.TryPickup();
        return true;
    }

    private void TryPlatformMove(Vector2 world)
    {
        if (navigator == null) return;

        Collider2D hit = FindClickable(world, platformMask);
        if (hit == null) return;

        PlatformNode targetPlatform =
            hit.GetComponentInParent<PlatformNode>() ??
            hit.GetComponent<PlatformNode>();

        if (targetPlatform == null) return;

        float clickedX = Mathf.Clamp(world.x, hit.bounds.min.x, hit.bounds.max.x);

        if (combat != null) combat.CancelCombat();
        navigator.NavigateToPlatform(targetPlatform, clickedX);
    }

    private bool TryMapTransition(Vector2 world)
    {
        Collider2D hit = FindClickable(world, mapTransitionMask);
        if (hit == null) return false;

        MapTransitionSign sign =
            hit.GetComponentInParent<MapTransitionSign>() ??
            hit.GetComponent<MapTransitionSign>() ??
            hit.GetComponentInChildren<MapTransitionSign>();

        if (sign == null) return false;
        if (!sign.CanEnter()) return true; // klik zu¿yty, ale warunki nieprzejœcia

        if (combat != null) combat.CancelCombat();
        sign.Interact();
        return true;
    }

    private Collider2D FindClickable(Vector2 world, LayerMask mask)
    {
        if (mask.value != 0)
        {
            Collider2D maskedHit = Physics2D.OverlapPoint(world, mask);
            if (maskedHit == null)
                maskedHit = Physics2D.OverlapCircle(world, clickRadius, mask);

            if (maskedHit != null)
                return maskedHit;
        }

        Collider2D hit = Physics2D.OverlapPoint(world);
        if (hit == null)
            hit = Physics2D.OverlapCircle(world, clickRadius);

        return hit;
    }
}
