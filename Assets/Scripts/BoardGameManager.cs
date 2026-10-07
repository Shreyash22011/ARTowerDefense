using UnityEngine;

public class BoardGameManager : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 2;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float timeBetweenWaves = 3f;

    private Transform[] waypoints;

    private int currentWave = 0;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;

    private bool waveInProgress;
    private bool gameEnded;

    [Header("Wave Reward")]
    [SerializeField] private int coinsPerWave = 50;

    private EconomyManager economyManager;

    private void Start()
    {
        waypoints = new Transform[]
        {
            transform.Find("Waypoint_Start"),
            transform.Find("Waypoint_1"),
            transform.Find("Waypoint_2"),
            transform.Find("Waypoint_End")
        };

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogError(
                    $"Missing waypoint at index {i}. " +
                    "Check BoardBase waypoint names."
                );
            }
        }

        economyManager = FindFirstObjectByType<EconomyManager>();

        if (economyManager == null)
        {
            Debug.LogError("EconomyManager not found.");
        }

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (gameEnded)
            return;

        if (currentWave >= totalWaves)
        {
            WinGame();
            return;
        }

        currentWave++;
        enemiesSpawned = 0;
        enemiesAlive = 0;
        waveInProgress = true;

        Debug.Log($"WAVE {currentWave} STARTED");

        InvokeRepeating(
            nameof(SpawnEnemy),
            0f,
            spawnInterval
        );
    }

    private void SpawnEnemy()
    {
        if (gameEnded)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned.");
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        if (enemiesSpawned >= enemiesPerWave)
        {
            CancelInvoke(nameof(SpawnEnemy));
            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            waypoints[0].position,
            Quaternion.identity
        );

        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

        if (movement != null)
            movement.waypoints = waypoints;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (health != null)
            health.SetWaveManager(this);

        enemiesSpawned++;
        enemiesAlive++;

        Debug.Log(
            $"Wave {currentWave}: " +
            $"{enemiesSpawned}/{enemiesPerWave} spawned"
        );
    }

    public void EnemyFinished()
    {
        if (gameEnded)
            return;

        enemiesAlive--;

        if (enemiesAlive < 0)
            enemiesAlive = 0;

        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        if (!waveInProgress)
            return;

        if (enemiesSpawned < enemiesPerWave)
            return;

        if (enemiesAlive > 0)
            return;

        waveInProgress = false;

        Debug.Log($"WAVE {currentWave} COMPLETED");

        if (economyManager != null)
        {
            economyManager.AddCoins(coinsPerWave);

            Debug.Log(
                $"Wave {currentWave} reward: +{coinsPerWave} coins"
            );
        }

        if (currentWave >= totalWaves)
        {
            WinGame();
            return;
        }

        Invoke(
            nameof(StartNextWave),
            timeBetweenWaves
        );
    }

    private void WinGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        Debug.Log("GAME WON!");
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public int CurrentWave => currentWave;

    public void LoseGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        CancelInvoke(nameof(SpawnEnemy));
        CancelInvoke(nameof(StartNextWave));

        Debug.Log("GAME LOST!");
    }
}