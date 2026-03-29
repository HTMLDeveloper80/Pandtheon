using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    public GameObject enemyPrefab;
    public EnemyData enemyData;
    public float respawnDelay = 30f;

    private GameObject currentEnemy;
    private float respawnTimer;

    void Start()
    {
        SpawnEnemy();
    }

    void Update()
    {
        if (currentEnemy == null)
        {
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= respawnDelay)
            {
                SpawnEnemy();
                respawnTimer = 0f;
            }
        }
    }

    void SpawnEnemy()
    {
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        EnemyHealth enemyHealth = currentEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.respawner = this;
        }

        EnemyStats stats = currentEnemy.GetComponent<EnemyStats>();
        if (stats != null && enemyData != null) 
        { 
            stats.data = enemyData;
        }
    }

    public void OnEnemyKilled()
    {
        currentEnemy = null;
    }
}
