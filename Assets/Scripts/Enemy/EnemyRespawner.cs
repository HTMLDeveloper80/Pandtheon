using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyRespawner : MonoBehaviour
{
    private static readonly Dictionary<string, double> respawnEndTimes =
        new Dictionary<string, double>();

    [Header("Respawn Settings")]
    public GameObject enemyPrefab;
    public EnemyData enemyData;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool parentSpawnedEnemyToSpawner = false;

    private GameObject currentEnemy;
    private Coroutine respawnRoutine;
    private bool hasStartedSpawning;
    private string runtimeSpawnerId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        respawnEndTimes.Clear();
    }

    private void Awake()
    {
        runtimeSpawnerId = BuildSpawnerId();
    }

    private void Start()
    {
        if (!spawnOnStart)
            return;

        hasStartedSpawning = true;

        if (TryGetRemainingCooldown(out float remainingTime))
        {
            respawnRoutine = StartCoroutine(
                RespawnAfterRemainingTime(remainingTime));
            return;
        }

        SpawnEnemy();
    }

    private void Update()
    {
        if (hasStartedSpawning &&
            currentEnemy == null &&
            respawnRoutine == null)
        {
            BeginRespawnCooldown();
        }
    }

    public void SpawnEnemy()
    {
        if (currentEnemy != null)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                $"{name}: Enemy prefab is not assigned on EnemyRespawner.");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogWarning(
                $"{name}: EnemyData is not assigned on EnemyRespawner.");
            return;
        }

        hasStartedSpawning = true;
        respawnEndTimes.Remove(runtimeSpawnerId);

        Transform parent = parentSpawnedEnemyToSpawner ? transform : null;
        currentEnemy = Instantiate(
            enemyPrefab,
            transform.position,
            transform.rotation,
            parent);

        EnemyHealth enemyHealth = currentEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.Initialize(enemyData, this);
        else
            Debug.LogWarning(
                $"{name}: Spawned enemy has no EnemyHealth component.");
    }

    public void OnEnemyKilled()
    {
        currentEnemy = null;
        BeginRespawnCooldown();
    }

    private void BeginRespawnCooldown()
    {
        if (respawnRoutine != null || !isActiveAndEnabled)
            return;

        float respawnDelay = enemyData != null
            ? Mathf.Max(0f, enemyData.respawnDelay)
            : 0f;

        if (!respawnEndTimes.TryGetValue(runtimeSpawnerId, out double endTime))
        {
            endTime = Time.realtimeSinceStartupAsDouble + respawnDelay;
            respawnEndTimes[runtimeSpawnerId] = endTime;
        }

        float remainingTime = Mathf.Max(
            0f,
            (float)(endTime - Time.realtimeSinceStartupAsDouble));

        respawnRoutine = StartCoroutine(
            RespawnAfterRemainingTime(remainingTime));
    }

    private bool TryGetRemainingCooldown(out float remainingTime)
    {
        remainingTime = 0f;

        if (!respawnEndTimes.TryGetValue(runtimeSpawnerId, out double endTime))
            return false;

        remainingTime = Mathf.Max(
            0f,
            (float)(endTime - Time.realtimeSinceStartupAsDouble));

        if (remainingTime > 0f)
            return true;

        respawnEndTimes.Remove(runtimeSpawnerId);
        return false;
    }

    private IEnumerator RespawnAfterRemainingTime(float remainingTime)
    {
        if (remainingTime > 0f)
            yield return new WaitForSecondsRealtime(remainingTime);

        respawnRoutine = null;
        SpawnEnemy();
    }

    private string BuildSpawnerId()
    {
        Scene scene = gameObject.scene;
        StringBuilder path = new StringBuilder();
        Transform current = transform;

        while (current != null)
        {
            path.Insert(
                0,
                $"/{current.name}[{current.GetSiblingIndex()}]");
            current = current.parent;
        }

        return $"{scene.name}:{path}";
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.loseRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + Vector3.left * enemyData.patrolRadius,
            transform.position + Vector3.right * enemyData.patrolRadius
        );
    }
}
