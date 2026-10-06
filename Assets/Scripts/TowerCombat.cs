using UnityEngine;

public class TowerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 1f;

    private EnemyHealth currentTarget;
    private float nextAttackTime;
    private float nextDebugScanTime;

    // TEMPORARY PHASE 1B RUNTIME DEBUG: remove after diagnosis.
    private static readonly bool RuntimeDebug = true;

    private void Awake()
    {
        attackRange = Mathf.Max(0f, attackRange);
        attackDamage = Mathf.Max(0, attackDamage);
        attackCooldown = Mathf.Max(0.01f, attackCooldown);

        if (RuntimeDebug)
        {
            Debug.Log($"[TowerCombat DEBUG] Archer initialized at {transform.position}. " +
                      $"Range={attackRange}, Damage={attackDamage}, Cooldown={attackCooldown}, " +
                      $"WorldScale={transform.lossyScale}", this);
        }
    }

    private void Update()
    {
        if (RuntimeDebug && Time.time >= nextDebugScanTime)
        {
            nextDebugScanTime = Time.time + 1f;
            Debug.Log($"[TowerCombat DEBUG] Archer scan at {transform.position}. " +
                      $"ActiveTarget={(currentTarget == null ? "none" : currentTarget.name)}", this);
        }

        if (!IsTargetValid(currentTarget))
        {
            if (RuntimeDebug && currentTarget != null)
                Debug.Log($"[TowerCombat DEBUG] Target lost/destroyed: {currentTarget.name}", this);

            currentTarget = FindTarget();
        }

        if (currentTarget == null || Time.time < nextAttackTime)
            return;

        if (RuntimeDebug)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            Debug.Log($"[TowerCombat DEBUG] Attack performed on {currentTarget.name}. " +
                      $"Distance={distance}, Damage={attackDamage}, " +
                      $"HealthBefore={currentTarget.CurrentHealth}", this);
        }

        currentTarget.TakeDamage(attackDamage);
        nextAttackTime = Time.time + attackCooldown;

        if (RuntimeDebug && currentTarget != null)
        {
            Debug.Log($"[TowerCombat DEBUG] Damage dealt to {currentTarget.name}. " +
                      $"HealthAfter={currentTarget.CurrentHealth}", this);
        }

        if (!IsTargetValid(currentTarget))
            currentTarget = null;
    }

    private EnemyHealth FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        EnemyHealth closestTarget = null;
        float closestDistanceSqr = float.PositiveInfinity;

        if (RuntimeDebug)
            Debug.Log($"[TowerCombat DEBUG] OverlapSphere detected {colliders.Length} colliders", this);

        foreach (Collider candidateCollider in colliders)
        {
            if (candidateCollider == null)
                continue;

            EnemyHealth candidate = candidateCollider.GetComponentInParent<EnemyHealth>();

            if (RuntimeDebug && candidate != null)
            {
                float candidateDistance = Vector3.Distance(transform.position, candidate.transform.position);
                Debug.Log($"[TowerCombat DEBUG] EnemyHealth candidate={candidate.name}, " +
                          $"Distance={candidateDistance}, Health={candidate.CurrentHealth}", this);
            }

            if (!IsTargetValid(candidate))
                continue;

            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestTarget = candidate;
                closestDistanceSqr = distanceSqr;
            }
        }

        if (RuntimeDebug && closestTarget != null)
            Debug.Log($"[TowerCombat DEBUG] Target acquired: {closestTarget.name}", this);

        return closestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (!RuntimeDebug)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private bool IsTargetValid(EnemyHealth target)
    {
        if (target == null || target.CurrentHealth <= 0)
            return false;

        float rangeSqr = attackRange * attackRange;
        return (target.transform.position - transform.position).sqrMagnitude <= rangeSqr;
    }
}
