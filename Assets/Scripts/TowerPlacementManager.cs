using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private GameObject icePrefab;

    [Header("Tower Costs")]
    [SerializeField] private int archerCost = 100;
    [SerializeField] private int cannonCost = 150;
    [SerializeField] private int iceCost = 200;

    [Header("Placement")]
    [Tooltip("Maximum horizontal distance from the player's tap to a BuildPoint.")]
    [SerializeField] private float placementSnapDistance = 0.04f;

    private GameObject selectedTowerPrefab;
    private int selectedTowerCost;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "[TowerPlacementManager] Main Camera not found."
            );
        }
    }

    // =========================================================
    // TOWER SELECTION
    // =========================================================

    public void SelectArcher()
    {
        selectedTowerPrefab = archerPrefab;
        selectedTowerCost = archerCost;

        Debug.Log("Archer selected");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void SelectCannon()
    {
        selectedTowerPrefab = cannonPrefab;
        selectedTowerCost = cannonCost;

        Debug.Log("Cannon selected");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void SelectIce()
    {
        selectedTowerPrefab = icePrefab;
        selectedTowerCost = iceCost;

        Debug.Log("Ice selected");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    // =========================================================
    // TOWER PLACEMENT
    // =========================================================

    private void Update()
    {
        // No tower selected.
        if (selectedTowerPrefab == null)
            return;

        // No camera available.
        if (mainCamera == null)
            return;

        Vector2 screenPosition;

        // =====================================================
        // Android: Touch input
        // =====================================================

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
        }

        // =====================================================
        // Laptop / Unity Editor: Mouse input
        // =====================================================

        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition =
                Mouse.current.position.ReadValue();
        }

        // No valid input this frame.
        else
        {
            return;
        }

        // =====================================================
        // Ray from camera through the screen position
        // =====================================================

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // -----------------------------------------------------
        // 1. Raycast against the board.
        // -----------------------------------------------------

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(
                "[TowerPlacementManager] Placement rejected: " +
                "raycast did not hit anything."
            );

            return;
        }

        // -----------------------------------------------------
        // 2. Make sure the player actually tapped/clicked board.
        // -----------------------------------------------------

        if (!IsGameBoardHit(hit))
        {
            Debug.Log(
                "[TowerPlacementManager] Placement rejected: " +
                "tap/click was not on the GameBoard."
            );

            return;
        }

        // -----------------------------------------------------
        // 3. Find the build manager belonging to this board.
        // -----------------------------------------------------

        BoardBuildManager boardBuildManager =
            hit.collider.GetComponentInParent<BoardBuildManager>();

        if (boardBuildManager == null)
        {
            Debug.LogError(
                "[TowerPlacementManager] Placement failed: " +
                "BoardBuildManager was not found on the board."
            );

            return;
        }

        // -----------------------------------------------------
        // 4. Find the nearest available legal BuildPoint.
        // -----------------------------------------------------

        TowerBuildPoint buildPoint =
            FindNearestAvailableBuildPoint(
                hit.point,
                boardBuildManager
            );

        if (buildPoint == null)
        {
            Debug.Log(
                "[TowerPlacementManager] Placement rejected: " +
                "no available BuildPoint is close enough to the tap."
            );

            return;
        }

        // -----------------------------------------------------
        // 5. Instantiate tower at the BuildPoint.
        // -----------------------------------------------------

        GameObject tower = Instantiate(
            selectedTowerPrefab,
            buildPoint.transform.position,
            buildPoint.transform.rotation
        );

        if (tower == null)
        {
            Debug.LogError(
                "[TowerPlacementManager] Tower instantiation failed."
            );

            return;
        }

        // -----------------------------------------------------
        // 6. Mark the BuildPoint as occupied.
        // -----------------------------------------------------

        buildPoint.SetOccupied(true);

        Debug.Log(
            $"[TowerPlacementManager] Tower placed successfully: " +
            $"{selectedTowerPrefab.name} at {buildPoint.name}"
        );

        // -----------------------------------------------------
        // 7. Play placement audio.
        // -----------------------------------------------------

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTowerPlace();

        // -----------------------------------------------------
        // 8. Clear tower selection.
        // -----------------------------------------------------

        selectedTowerPrefab = null;
    }
    
    // =========================================================
    // BOARD VALIDATION
    // =========================================================

    private bool IsGameBoardHit(RaycastHit hit)
    {
        if (hit.collider == null)
            return false;

        // Direct GameBoard tag.
        if (hit.collider.CompareTag("GameBoard"))
            return true;

        // GameBoard tag on the root object.
        if (hit.collider.transform.root.CompareTag("GameBoard"))
            return true;

        return false;
    }

    // =========================================================
    // BUILD POINT SEARCH
    // =========================================================

    private TowerBuildPoint FindNearestAvailableBuildPoint(
        Vector3 hitPosition,
        BoardBuildManager boardBuildManager)
    {
        TowerBuildPoint nearestPoint = null;

        float closestDistanceSqr =
            placementSnapDistance * placementSnapDistance;

        if (boardBuildManager.BuildPoints == null ||
            boardBuildManager.BuildPoints.Length == 0)
        {
            Debug.LogWarning(
                "[TowerPlacementManager] Board has no BuildPoints."
            );

            return null;
        }

        foreach (TowerBuildPoint point in boardBuildManager.BuildPoints)
        {
            if (point == null)
                continue;

            // -------------------------------------------------
            // Already occupied.
            // -------------------------------------------------

            if (point.IsOccupied)
                continue;

            // -------------------------------------------------
            // Compare horizontal distance only.
            //
            // Y differences should not affect whether the
            // player's tap is close enough to a build point.
            // -------------------------------------------------

            Vector2 hitXZ = new Vector2(
                hitPosition.x,
                hitPosition.z
            );

            Vector2 pointXZ = new Vector2(
                point.transform.position.x,
                point.transform.position.z
            );

            float distanceSqr =
                (hitXZ - pointXZ).sqrMagnitude;

            // -------------------------------------------------
            // Point is outside the allowed snap radius.
            // -------------------------------------------------

            if (distanceSqr > closestDistanceSqr)
                continue;

            // -------------------------------------------------
            // This is the closest valid point so far.
            // -------------------------------------------------

            closestDistanceSqr = distanceSqr;
            nearestPoint = point;
        }

        return nearestPoint;
    }

    // =========================================================
    // PUBLIC ACCESSORS
    // =========================================================

    public bool HasSelectedTower()
    {
        return selectedTowerPrefab != null;
    }

    public GameObject GetSelectedTower()
    {
        return selectedTowerPrefab;
    }

    public int GetSelectedTowerCost()
    {
        return selectedTowerCost;
    }
}