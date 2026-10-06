using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Material healthBarMaterial;

    private int currentHealth;
    private Transform healthBar;

    private void Start()
    {
        currentHealth = maxHealth;

        CreateHealthBar();
        UpdateHealthBar();
    }

    private void CreateHealthBar()
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);

        bar.name = "BaseHealthBar";

        bar.transform.SetParent(transform);

        bar.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        bar.transform.localRotation = Quaternion.identity;
        bar.transform.localScale = new Vector3(0.8f, 0.08f, 0.08f);

        Renderer renderer = bar.GetComponent<Renderer>();

        if (healthBarMaterial != null)
        {
            renderer.material = healthBarMaterial;
        }
        else
        {
            Debug.LogError("Health Bar Material is not assigned!");
        }

        healthBar = bar.transform;

        Destroy(bar.GetComponent<Collider>());
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();

        Debug.Log("BASE HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER!");
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        float healthPercent = (float)currentHealth / maxHealth;

        healthBar.localScale = new Vector3(
            0.8f * healthPercent,
            0.08f,
            0.08f
        );
    }
}