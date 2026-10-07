using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [Header("Starting Economy")]
    [SerializeField] private int startingCoins = 250;

    private int currentCoins;

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        currentCoins = startingCoins;
    }

    public bool CanAfford(int cost)
    {
        return currentCoins >= cost;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (!CanAfford(amount))
            return false;

        currentCoins -= amount;

        Debug.Log($"Spent {amount} coins. Remaining: {currentCoins}");

        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        currentCoins += amount;

        Debug.Log($"Added {amount} coins. Total: {currentCoins}");
    }
}