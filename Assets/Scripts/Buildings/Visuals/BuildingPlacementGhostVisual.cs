using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 选中建筑的幽灵效果
/// </summary>
public class BuildingPlacementGhostVisual : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private List<SpriteRenderer> buildingSprite;
    private BuildingSo _currentBuildingSo;

    // 缓存世界相机，避免运行时反复查找主相机。
    private void Awake()
    {
        if (!worldCamera)
        {
            worldCamera = Camera.main;
        }
    }

    private void Start()
    {
        BuildingPlacementManager.Instance.OnSelectedBuildingChanged += OnSelectedBuildingChanged;
        HideAllGhost();
    }

    private void OnSelectedBuildingChanged(BuildingSo buildSo)
    {
        _currentBuildingSo = buildSo;
    }

    private void Update()
    {
        // 没选建筑
        if (!_currentBuildingSo)
        {
            HideAllGhost();
            TooltipManager.Instance.HideEfficiencyTooltip();
            return;
        }

        // 鼠标在UI上
        if (EventSystem.current.IsPointerOverGameObject())
        {
            HideAllGhost();
            TooltipManager.Instance.HideEfficiencyTooltip();
            return;
        }

        // 更新位置
        transform.position = GetMousePosition();

        bool isAuthorizedConstructionZone = BuildingPlacementManager.Instance.IsAuthorizedZone(
            _currentBuildingSo,
            transform.position
        );

        bool canAfford = BuildingPlacementManager.Instance.CanAfford(_currentBuildingSo);

        if (_currentBuildingSo.buildingType == BuildingSo.BuildingType.Harvester)
        {
            float efficiency =
                BuildingPlacementManager.Instance.CalculateEfficiency(
                    _currentBuildingSo,
                    transform.position
                );

            TooltipManager.Instance.ShowEfficiencyTooltip(
                efficiency,
                _currentBuildingSo.resourceSo.sprite
            );
        }
        else
        {
            TooltipManager.Instance.HideEfficiencyTooltip();
        }

        // 显示对应幽灵
        ShowGhost(_currentBuildingSo, isAuthorizedConstructionZone, canAfford);
    }

    // 获取当前鼠标位置对应的世界坐标。
    private Vector3 GetMousePosition()
    {
        if (!worldCamera)
        {
            return Vector3.zero;
        }

        Vector3 position = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0f;
        return position;
    }

    private void ShowGhost(BuildingSo buildSo, bool isAuthorizedConstructionZone, bool canAfford)
    {
        foreach (var building in buildingSprite)
        {
            building.gameObject.SetActive(building.sprite.name == buildSo.assetName);
            building.color = isAuthorizedConstructionZone && canAfford
                ? new Color(0, 1, 0, 0.5f)
                : new Color(1, 0, 0, 0.5f);
        }
    }

    private void HideAllGhost()
    {
        foreach (var building in buildingSprite)
        {
            building.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        BuildingPlacementManager.Instance.OnSelectedBuildingChanged -= OnSelectedBuildingChanged;
    }
}
