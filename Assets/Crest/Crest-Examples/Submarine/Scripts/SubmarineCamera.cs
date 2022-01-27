// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

public class SubmarineCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraRoot = null;
    [SerializeField] private Transform _cameraTarget = null;
    [SerializeField] private Transform _cameraTargetPosition = null;

    private Vector3 _deltaMousePosition = Vector3.zero;
    private Vector3 _lastMousePosition = Vector3.zero;
    private float _deltaMouseScroll;

    public static SubmarineCamera Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (_cameraTarget == null) return;

        _deltaMousePosition = Input.mousePosition - _lastMousePosition;
        _lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0) == true)
        {
            _cameraRoot.Rotate(0f, _deltaMousePosition.x * Time.deltaTime * 3.5f, 0f);
            _cameraTargetPosition.position = new Vector3(_cameraTargetPosition.position.x, _cameraTargetPosition.position.y - _deltaMousePosition.y * Time.deltaTime, _cameraTargetPosition.position.z);
        }

        _deltaMouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (_deltaMouseScroll != 0)
        {
            _cameraTargetPosition.position = _cameraTargetPosition.position + _cameraTargetPosition.forward * _deltaMouseScroll * 1000 * Time.deltaTime;
        }

        transform.position = Vector3.Lerp(transform.position, _cameraTargetPosition.position, Time.deltaTime);
        transform.LookAt(_cameraTarget, Vector3.up);
    }
}
