using System.Collections;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    public GameObject enemyPrefab;
    public EnemyData enemyData;
    public float respawnDelay = 30f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool parentSpawnedEnemyToSpawner = false;

    private GameObject currentEnemy;
    private Coroutine respawnRoutine;
    private bool hasStartedSpawning;

    private void Start()
    {
        if (spawnOnStart)
        {
            hasStartedSpawning = true;
            SpawnEnemy();
        }
    }

    private void Update()
    {
        if (hasStartedSpawning && currentEnemy == null && respawnRoutine == null)
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    public void SpawnEnemy()
    {
        hasStartedSpawning = true;

        if (currentEnemy != null)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name}: Enemy prefab is not assigned on EnemyRespawner.");
            return;
        }

        Transform parent = parentSpawnedEnemyToSpawner ? transform : null;
        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation, parent);

        EnemyHealth enemyHealth = currentEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.Initialize(enemyData, this);
        else
            Debug.LogWarning($"{name}: Spawned enemy has no EnemyHealth component.");

        EnemyStats stats = currentEnemy.GetComponent<EnemyStats>();
        if (stats != null && enemyData != null)
            stats.ApplyData(enemyData);
    }

    public void OnEnemyKilled()
    {
        currentEnemy = null;

        if (respawnRoutine == null && isActiveAndEnabled)
            respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, respawnDelay));
        respawnRoutine = null;
        SpawnEnemy();
    }
}
