using UnityEngine;

public class BoardBuildManager : MonoBehaviour
{
    [SerializeField] private TowerBuildPoint[] buildPoints;

    public TowerBuildPoint[] BuildPoints => buildPoints;

    private void Awake()
    {
        if (buildPoints == null || buildPoints.Length == 0)
        {
            buildPoints = GetComponentsInChildren<TowerBuildPoint>();

            Debug.Log(
                $"[BoardBuildManager] Found {buildPoints.Length} build points.",
                this
            );
        }
    }
}