using Cinemachine;
using UnityEngine;

/// <summary>
/// 处理相机拖拽移动和滚轮缩放。
/// </summary>
public class CameraHandler : MonoBehaviour
{
    private const float MIN_DRAG_DAMPING = 0f;
    private const float DRAG_ZONE_SIZE = 0f;

    [SerializeField] private float dragSensitivity = 0.35f;
    [SerializeField] private float dragFollowDamping = 0.18f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomAmount;
    [SerializeField] private float minOrthographicSize;
    [SerializeField] private float maxOrthographicSize;

    private CinemachineFramingTransposer _framingTransposer;
    private float _targetOrthographicSize;
    private float _orthographicSize;
    private Vector3 _lastMousePosition;

    // 初始化相机缩放状态并配置拖拽时的直接跟随。
    private void Start()
    {
        if (!virtualCamera)
        {
            enabled = false;
            return;
        }

        CacheCameraComponents();
        ConfigureDirectDragFollow();
        _orthographicSize = virtualCamera.m_Lens.OrthographicSize;
        _targetOrthographicSize = _orthographicSize;
    }

    // 每帧处理玩家的相机拖拽和缩放输入。
    private void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    // 缓存 Cinemachine 组件，避免运行时重复查找。
    private void CacheCameraComponents()
    {
        _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    // 将虚拟相机配置为拖拽时立即跟随控制点，避免阻尼造成卡顿感。
    private void ConfigureDirectDragFollow()
    {
        if (!_framingTransposer)
        {
            return;
        }

        float damping = Mathf.Max(MIN_DRAG_DAMPING, dragFollowDamping);
        _framingTransposer.m_XDamping = damping;
        _framingTransposer.m_YDamping = damping;
        _framingTransposer.m_ZDamping = damping;
        _framingTransposer.m_DeadZoneWidth = DRAG_ZONE_SIZE;
        _framingTransposer.m_DeadZoneHeight = DRAG_ZONE_SIZE;
        _framingTransposer.m_SoftZoneWidth = DRAG_ZONE_SIZE;
        _framingTransposer.m_SoftZoneHeight = DRAG_ZONE_SIZE;
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
            return;

        Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
        float unitsPerPixel = _orthographicSize * 2f / Screen.height;
        Vector3 moveDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0f) * (unitsPerPixel * dragSensitivity);

        transform.position += moveDelta;
        _lastMousePosition = Input.mousePosition;
    }

    // 使用鼠标滚轮缩放视角。
    private void HandleZoom()
    {
        _targetOrthographicSize += -Input.GetAxis("Mouse ScrollWheel") * zoomAmount;
        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);

        _orthographicSize = Mathf.Lerp(_orthographicSize, _targetOrthographicSize, Time.deltaTime * zoomSpeed);
        virtualCamera.m_Lens.OrthographicSize = _orthographicSize;
    }
}
