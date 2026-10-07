using UnityEngine;

public class TowerBuildPoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
    }
}