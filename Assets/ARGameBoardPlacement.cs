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

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    private void Update()
    {
        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        if (raycastManager.Raycast(
            touchPosition,
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

            // Hide and remove all detected AR planes
            if (planeManager != null)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }

                planeManager.enabled = false;
            }

            // Stop further placement
            enabled = false;
        }
    }
}