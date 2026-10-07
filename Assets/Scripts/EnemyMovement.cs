using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 0.2f;
    public int damageToBase = 25;

    private int currentWaypoint = 0;

    private float speedMultiplier = 1f;
    private float slowEndTime;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypoint];

        float currentSpeed = speed;

        if (Time.time < slowEndTime)
        {
            currentSpeed *= speedMultiplier;
        }
        else
        {
            speedMultiplier = 1f;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            currentSpeed * Time.deltaTime
        );

        Vector3 direction = target.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.02f)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                BaseHealth baseHealth =
                    FindAnyObjectByType<BaseHealth>();

                if (baseHealth != null)
                {
                    baseHealth.TakeDamage(damageToBase);

                    BoardGameManager waveManager = FindFirstObjectByType<BoardGameManager>();

                    if (waveManager != null)
                        waveManager.EnemyFinished();

                    Destroy(gameObject);
                }

                Destroy(gameObject);
            }
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp(multiplier, 0.1f, 1f);

        speedMultiplier = Mathf.Min(speedMultiplier, multiplier);

        slowEndTime = Mathf.Max(slowEndTime, Time.time + duration);
    }
}
