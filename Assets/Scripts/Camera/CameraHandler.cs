using Cinemachine;
using UnityEngine;

/// <summary>
/// 处理相机拖拽移动和滚轮缩放。
/// </summary>
public class CameraHandler : MonoBehaviour
{
    [SerializeField] private float dragSensitivity = 0.35f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomAmount;
    [SerializeField] private float minOrthographicSize;
    [SerializeField] private float maxOrthographicSize;

    private float _targetOrthographicSize;
    private float _orthographicSize;
    private Vector3 _lastMousePosition;

    private void Start()
    {
        _orthographicSize = virtualCamera.m_Lens.OrthographicSize;
        _targetOrthographicSize = _orthographicSize;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    /// <summary>
    /// 按住鼠标右键拖动视角。
    /// </summary>
    private void HandleMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _lastMousePosition = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(1))
            return;

        Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
        float unitsPerPixel = _orthographicSize * 2f / Screen.height;
        Vector3 moveDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0f) * (unitsPerPixel * dragSensitivity);

        transform.position += moveDelta;
        _lastMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// 使用鼠标滚轮缩放视角。
    /// </summary>
    private void HandleZoom()
    {
        _targetOrthographicSize += -Input.GetAxis("Mouse ScrollWheel") * zoomAmount;
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);

        _orthographicSize = Mathf.Lerp(_orthographicSize, _targetOrthographicSize, Time.deltaTime * zoomSpeed);
        virtualCamera.m_Lens.OrthographicSize = _orthographicSize;
    }
}
