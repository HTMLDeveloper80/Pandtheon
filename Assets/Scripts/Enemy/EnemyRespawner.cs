using System.Collections;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    public GameObject enemyPrefab;
    public EnemyData enemyData;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool parentSpawnedEnemyToSpawner = false;

    private GameObject currentEnemy;
    private Coroutine respawnRoutine;
    private bool hasStartedSpawning;

    private void Start()
    {
        if (spawnOnStart)
            SpawnEnemy();
    }

    private void Update()
    {
        if (hasStartedSpawning && currentEnemy == null && respawnRoutine == null)
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    public void SpawnEnemy()
    {
        if (currentEnemy != null)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name}: Enemy prefab is not assigned on EnemyRespawner.");
            return;
        }

        if (enemyData == null)
        {
            Debug.LogWarning($"{name}: EnemyData is not assigned on EnemyRespawner.");
            return;
        }

        hasStartedSpawning = true;

        Transform parent = parentSpawnedEnemyToSpawner ? transform : null;
        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation, parent);

        EnemyHealth enemyHealth = currentEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.Initialize(enemyData, this);
        else
            Debug.LogWarning($"{name}: Spawned enemy has no EnemyHealth component.");
    }

    public void OnEnemyKilled()
    {
        currentEnemy = null;

        if (respawnRoutine == null && isActiveAndEnabled)
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        float respawnDelay = enemyData != null ? enemyData.respawnDelay : 0f;
        yield return new WaitForSeconds(respawnDelay);
        respawnRoutine = null;
        SpawnEnemy();
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
