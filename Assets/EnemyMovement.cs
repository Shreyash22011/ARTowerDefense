using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 0.2f;
    public int damageToBase = 25;

    private int currentWaypoint = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypoint];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
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
                    FindFirstObjectByType<BaseHealth>();

                if (baseHealth != null)
                {
                    baseHealth.TakeDamage(damageToBase);
                }

                Destroy(gameObject);
            }
        }
    }
}