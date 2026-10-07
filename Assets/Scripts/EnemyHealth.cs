using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private static readonly bool RuntimeDebug = true;

    private BoardGameManager waveManager;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (RuntimeDebug)
            Debug.Log($"[EnemyHealth DEBUG] {name} took {damage} damage. " +
                      $"Health={currentHealth}/{maxHealth}", this);

        if (currentHealth == 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");

        if (waveManager != null)
            waveManager.EnemyFinished();

        Destroy(gameObject);
    }

    public void SetWaveManager(BoardGameManager manager)
    {
        waveManager = manager;
    }
}
