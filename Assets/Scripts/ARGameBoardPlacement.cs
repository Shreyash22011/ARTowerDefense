using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

public class ARGameBoardPlacement : MonoBehaviour
{
    [SerializeField]
    private GameObject gameBoardPrefab;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    private void Update()
    {
        Vector2 screenPosition;

        // --------------------------------------------------
        // Android: Touch input
        // --------------------------------------------------
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
        }

        // --------------------------------------------------
        // Laptop / Unity Editor: Mouse input
        // --------------------------------------------------
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition =
                Mouse.current.position.ReadValue();
        }

        // No valid input this frame
        else
        {
            return;
        }

        // --------------------------------------------------
        // AR Raycast
        // --------------------------------------------------
        if (raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Place the board
            Instantiate(
                gameBoardPrefab,
                hitPose.position + Vector3.up * 0.01f,
                Quaternion.identity
            );

            // Hide detected AR planes
            if (planeManager != null)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }

                planeManager.enabled = false;
            }

            // Prevent placing another board
            enabled = false;
        }
    }
}