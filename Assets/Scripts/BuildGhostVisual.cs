using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 选中建筑的幽灵效果
/// </summary>
public class BuildGhostVisual : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> buildingSprite;
    private BuildingSo _currentBuildingSo;

    private void Start()
    {
        BuildManager.Instance.OnSelectedBuildingChanged += OnSelectedBuildingChanged;
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
        transform.position = Utils.GetMousePosition();

        bool canBuild = BuildManager.Instance.CanBuild(
            _currentBuildingSo,
            transform.position
        );

        if (_currentBuildingSo.buildingType == BuildingSo.BuildingType.Harvester)
        {
            float efficiency =
                BuildManager.Instance.CalculateEfficiency(
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
        ShowGhost(_currentBuildingSo, canBuild);
    }

    private void ShowGhost(BuildingSo buildSo, bool canBuild)
    {
        foreach (var building in buildingSprite)
        {
            building.gameObject.SetActive(building.sprite.name == buildSo.assetName);
            building.color = canBuild ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
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
        BuildManager.Instance.OnSelectedBuildingChanged -= OnSelectedBuildingChanged;
    }
}