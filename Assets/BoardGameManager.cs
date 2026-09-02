using UnityEngine;

public class BoardGameManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnDelay = 2f;

    private Transform[] waypoints;

    private void Start()
    {
        waypoints = new Transform[]
        {
            transform.Find("Waypoint_Start"),
            transform.Find("Waypoint_1"),
            transform.Find("Waypoint_2"),
            transform.Find("Waypoint_End")
        };

        InvokeRepeating(nameof(SpawnEnemy), 2f, spawnDelay);
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned!");
            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            waypoints[0].position,
            Quaternion.identity
        );

        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.waypoints = waypoints;
        }
    }
}