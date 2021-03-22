﻿using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// A simple and dumb camera script that can be controlled using WASD and the mouse.
/// </summary>
[RequireComponent(typeof(Camera))] 
public class CameraController : MonoBehaviour
{
    public float linSpeed = 2f;
    public float rotSpeed = 110f;

    public bool simForwardInput = false;
    public bool _requireRMBToMove = false;

    Vector2 _lastMousePos = -Vector2.one;
    bool _dragging = false;
    Camera _camera;

    public float _fixedDt = 1 / 60f;

    Transform _targetTransform;
    AgentManager agentManager;

    [System.Serializable]
    class DebugFields
    {
        [Tooltip("Disables the XR occlusion mesh for debugging purposes. Only works with legacy XR.")]
        public bool disableOcclusionMesh = false;

        [Tooltip("Sets the XR occlusion mesh scale. Useful for debugging refractions. Only works with legacy XR."), Range(1f, 2f)]
        public float occlusionMeshScale = 1f;
    }

    [SerializeField] DebugFields _debug = new DebugFields();

    void Awake()
    {
        _targetTransform = transform;
        _camera = gameObject.GetComponent<Camera>();
        agentManager = GameObject.FindObjectOfType<AgentManager>();

        // We cannot change the Camera's transform when XR is enabled. This is not an issue with the new XR plugin.
        if (XRSettings.enabled)
        {
            // Disable XR temporarily so we can change the transform of the camera.
            XRSettings.enabled = false;
            // The VR camera is moved in local space, so we can move the camera if we move its parent we create instead.
            var parent = new GameObject("VRCameraOffset");
            parent.transform.parent = _targetTransform.parent;
            // Copy the transform over to the parent.
            parent.transform.position = _targetTransform.position;
            parent.transform.rotation = _targetTransform.rotation;
            // Parent camera to offset and reset transform. Scale changes slightly in editor so we will reset that too.
            _targetTransform.parent = parent.transform;
            _targetTransform.localPosition = Vector3.zero;
            _targetTransform.localRotation = Quaternion.identity;
            _targetTransform.localScale = Vector3.one;
            // We want to manipulate this transform.
            _targetTransform = parent.transform;
            XRSettings.enabled = true;

            // Seems like the best place to put this for now. Most XR debugging happens using this component.
            XRSettings.useOcclusionMesh = !_debug.disableOcclusionMesh;
            XRSettings.occlusionMaskScale = _debug.occlusionMeshScale;
        }
    }

    void Update()
    {

        float dt = Time.deltaTime;
        if (_fixedDt > 0f)
            dt = _fixedDt;

        UpdateMovement(dt);

        // These aren't useful and can break for XR hardware.
        if (!XRSettings.enabled || XRSettings.loadedDeviceName == "MockHMD")
        {
            UpdateDragging(dt);
            UpdateKillRoll();
        }

        if (XRSettings.enabled)
        {
            // Check if property has changed.
            if (XRSettings.useOcclusionMesh == _debug.disableOcclusionMesh)
            {
                XRSettings.useOcclusionMesh = !_debug.disableOcclusionMesh;
            }

            XRSettings.occlusionMaskScale = _debug.occlusionMeshScale;
        }
    }

    void UpdateMovement(float dt)
    {

        if (agentManager.activeAgent != gameObject)
            return;

        if (!Input.GetMouseButton(0) && _requireRMBToMove) return;
        
        if (Input.GetKey(KeyCode.R)) // if reset view
        {
            var parent = gameObject.transform.parent;
            _targetTransform.position = parent.position;
            _targetTransform.rotation = parent.rotation;
            _targetTransform.localScale = parent.localScale;
            return;
        }

        float forward = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        if (simForwardInput)
        {
            forward = 1f;
        }

        _targetTransform.position += linSpeed * _targetTransform.forward * forward * dt;
        var speed = linSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 3f;
        }

        _targetTransform.position += speed * _targetTransform.forward * forward * dt;
        //_transform.position += linSpeed * _transform.right * Input.GetAxis( "Horizontal" ) * dt;
        _targetTransform.position += linSpeed * _targetTransform.up * (Input.GetKey(KeyCode.E) ? 1 : 0) * dt;
        _targetTransform.position -= linSpeed * _targetTransform.up * (Input.GetKey(KeyCode.Q) ? 1 : 0) * dt;
        _targetTransform.position -= linSpeed * _targetTransform.right * (Input.GetKey(KeyCode.A) ? 1 : 0) * dt;
        _targetTransform.position += linSpeed * _targetTransform.right * (Input.GetKey(KeyCode.D) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.up * (Input.GetKey(KeyCode.E) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.up * (Input.GetKey(KeyCode.Q) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.right * (Input.GetKey(KeyCode.A) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.right * (Input.GetKey(KeyCode.D) ? 1 : 0) * dt;

        {
            float rotate = 0f;
            rotate += (Input.GetKey(KeyCode.X) ? 1 : 0);
            rotate -= (Input.GetKey(KeyCode.Y) ? 1 : 0);
            rotate *= 5f;
            Vector3 ea = _targetTransform.eulerAngles;
            ea.y += 0.1f * rotSpeed * rotate * dt;
            _targetTransform.eulerAngles = ea;
        }
    }

    void UpdateDragging(float dt)
    {
        Vector2 mousePos;
        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        if (!_dragging && Input.GetMouseButtonDown(1) && !Crest.OceanDebugGUI.OverGUI(mousePos))
        {
            _dragging = true;
            _lastMousePos = mousePos;
        }
        if (_dragging && Input.GetMouseButtonUp(1))
        {
            _dragging = false;
            _lastMousePos = -Vector2.one;
        }

        if (_dragging)
        {
            Vector2 delta = mousePos - _lastMousePos;

            Vector3 ea = _targetTransform.eulerAngles;
            ea.x += -0.1f * rotSpeed * delta.y * dt;
            ea.y += 0.1f * rotSpeed * delta.x * dt;
            _targetTransform.eulerAngles = ea;

            _lastMousePos = mousePos;
        }
    }

    void UpdateKillRoll()
    {
        Vector3 ea = _targetTransform.eulerAngles;
        ea.z = 0f;
        transform.eulerAngles = ea;
    }
}
