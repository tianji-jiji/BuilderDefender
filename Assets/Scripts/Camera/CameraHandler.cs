using System;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 处理相机拖拽移动和滚轮缩放。
/// </summary>
public class CameraHandler : MonoBehaviour
{

    [SerializeField] private float dragSensitivity = 0.35f;
    [SerializeField] private float dragFollowDamping = 0.18f;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomAmount;
    [SerializeField] private float minOrthographicSize;
    [SerializeField] private float maxOrthographicSize;

    private float _targetOrthographicSize;
    private float _orthographicSize;
    private Vector3 _lastMousePosition;

    private void Awake()
    {
        if (!cinemachineCamera && !TryGetComponent(out cinemachineCamera))
        {
            enabled = false;
            return;
        }

        _orthographicSize = cinemachineCamera.Lens.OrthographicSize;
        _targetOrthographicSize = _orthographicSize;
    }


    // 每帧处理玩家的相机拖拽和缩放输入。
    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }
  

    // 按住鼠标右键拖动视角。
    private void HandleMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _lastMousePosition = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(1))
        {
            return;
        }

        Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
        float unitsPerPixel = _orthographicSize * 2f / Screen.height;
        Vector3 moveDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0f) * (unitsPerPixel * dragSensitivity);

        transform.position += moveDelta;
        _lastMousePosition = Input.mousePosition;
    }

    // 使用鼠标滚轮缩放视角。
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        _targetOrthographicSize -= scroll * zoomAmount;
        _targetOrthographicSize = Mathf.Clamp(
            _targetOrthographicSize,
            minOrthographicSize,
            maxOrthographicSize
        );

        _orthographicSize = Mathf.Lerp(
            _orthographicSize,
            _targetOrthographicSize,
            Time.deltaTime * zoomSpeed
        );

        LensSettings lens = cinemachineCamera.Lens;
        lens.OrthographicSize = _orthographicSize;
        cinemachineCamera.Lens = lens;
    }
}
