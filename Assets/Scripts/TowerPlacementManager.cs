using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

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
    [SerializeField] private float towerHeight = 0.1f;

    private GameObject selectedTowerPrefab;
    private int selectedTowerCost;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

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

    private void Update()
    {
        if (selectedTowerPrefab == null)
            return;

        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("GameBoard") ||
                hit.collider.transform.root.CompareTag("GameBoard"))
            {
                Vector3 position = hit.point;

                position.y += towerHeight;

                GameObject tower = Instantiate(
                    selectedTowerPrefab,
                    position,
                    Quaternion.identity
                );

                Debug.Log("Tower placed: " + selectedTowerPrefab.name);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayTowerPlace();

                selectedTowerPrefab = null;
            }
        }
    }

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
