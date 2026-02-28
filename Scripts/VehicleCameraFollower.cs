using UnityEngine;

namespace Marus.CameraControl
{
    /// <summary>
    /// Makes the camera follow and look at a target vehicle.
    /// Attach this to the MainCamera.
    /// </summary>
    public class VehicleCameraFollower : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The vehicle to follow (e.g., Dextra GameObject)")]
        public Transform target;

        [Header("Camera Offset")]
        [Tooltip("Offset from vehicle in vehicle's local space")]
        public Vector3 positionOffset = new Vector3(0f, 0.7f, -2f);

        [Header("Camera Rotation")]
        [Tooltip("Camera rotation in degrees")]
        public Vector3 rotationOffset = new Vector3(25f, 0f, 0f);

        [Header("Follow Settings")]
        [Tooltip("Smooth follow speed (0 = instant, higher = slower)")]
        public float smoothSpeed = 5f;

        [Tooltip("Update in LateUpdate for smoother following")]
        public bool useLateUpdate = true;

        void Start()
        {
            Debug.Log("VehicleCameraFollower: Starting...");
            
            if (target == null)
            {
                Debug.Log("VehicleCameraFollower: No target assigned, searching for 'Dextra'...");
                var dextra = GameObject.Find("Dextra");
                if (dextra != null)
                {
                    target = dextra.transform;
                    Debug.Log($"VehicleCameraFollower: Auto-found target '{dextra.name}' at position {dextra.transform.position}");
                }
                else
                {
                    Debug.LogError("VehicleCameraFollower: No target assigned and could not find 'Dextra'");
                }
            }
            else
            {
                Debug.Log($"VehicleCameraFollower: Target already assigned: '{target.name}'");
            }
        }

        void LateUpdate()
        {
            if (useLateUpdate)
            {
                UpdateCameraPosition();
            }
        }

        void Update()
        {
            if (!useLateUpdate)
            {
                UpdateCameraPosition();
            }
        }

        void UpdateCameraPosition()
        {
            if (target == null)
            {
                Debug.LogWarning("VehicleCameraFollower: Target is null in UpdateCameraPosition");
                return;
            }

            // Calculate desired position in world space
            Vector3 desiredPosition = target.TransformPoint(positionOffset);

            // Smooth follow
            if (smoothSpeed > 0)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = desiredPosition;
            }

            // Apply rotation offset relative to vehicle's forward direction
            Quaternion targetRotation = target.rotation * Quaternion.Euler(rotationOffset);
            
            if (smoothSpeed > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
