using UnityEngine;

public class TowerCombat : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Targeting")]
    [SerializeField] private float targetScanInterval = 0.15f;

    private EnemyHealth currentTarget;

    private float nextAttackTime;
    private float nextTargetScanTime;

    public enum TowerType
    {
        Archer,
        Cannon,
        Ice
    }

    [SerializeField] private TowerType towerType;

    [Header("Ice Effect")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 2f;

    private void Awake()
    {
        attackRange = Mathf.Max(0f, attackRange);
        attackDamage = Mathf.Max(0, attackDamage);
        attackCooldown = Mathf.Max(0.01f, attackCooldown);
        targetScanInterval = Mathf.Max(0.05f, targetScanInterval);
    }

    private void Update()
    {
        // -------------------------------------------------
        // 1. Make sure the current target is still usable.
        // -------------------------------------------------

        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
        }

        // -------------------------------------------------
        // 2. Periodically search for a target.
        // -------------------------------------------------

        if (currentTarget == null && Time.time >= nextTargetScanTime)
        {
            nextTargetScanTime = Time.time + targetScanInterval;

            currentTarget = FindTarget();
        }

        // -------------------------------------------------
        // 3. No target -> nothing to attack.
        // -------------------------------------------------

        if (currentTarget == null)
            return;

        // -------------------------------------------------
        // 4. Respect attack cooldown.
        // -------------------------------------------------

        if (Time.time < nextAttackTime)
            return;

        // -------------------------------------------------
        // 5. Attack.
        // -------------------------------------------------

        currentTarget.TakeDamage(attackDamage);

        if (towerType == TowerType.Ice)
        {
            EnemyMovement movement = currentTarget.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.ApplySlow(slowMultiplier, slowDuration);
            }
        }

        nextAttackTime = Time.time + attackCooldown;

        // -------------------------------------------------
        // 6. Target may have died from this attack.
        // -------------------------------------------------

        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
        }

        // -------------------------------------------------
        // 7. Attack sound.
        // -------------------------------------------------

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerAttack();
        }
    }

    private EnemyHealth FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);

        EnemyHealth closestTarget = null;

        float closestDistanceSqr = float.PositiveInfinity;

        foreach (Collider candidateCollider in colliders)
        {
            if (candidateCollider == null)
                continue;

            EnemyHealth candidate = candidateCollider.GetComponentInParent<EnemyHealth>();

            if (!IsTargetValid(candidate))
                continue;

            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestTarget = candidate;
            }
        }

        return closestTarget;
    }

    private bool IsTargetValid(EnemyHealth target)
    {
        if (target == null)
            return false;

        if (target.CurrentHealth <= 0)
            return false;

        float rangeSqr = attackRange * attackRange;

        float distanceSqr = (target.transform.position - transform.position).sqrMagnitude;

        return distanceSqr <= rangeSqr;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}