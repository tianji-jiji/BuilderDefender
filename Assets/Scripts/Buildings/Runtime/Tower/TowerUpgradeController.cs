using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tower 升级控制器，负责星级状态、升级消耗和升级属性应用。
/// </summary>
[DisallowMultipleComponent]
public class TowerUpgradeController : MonoBehaviour
{
    private const int MIN_STAR_LEVEL = 1;

    [SerializeField] private BuildingUpgradeConfigSo upgradeConfig;
    [SerializeField] private Building building;
    [SerializeField] private TowerCombatSystem towerCombatSystem;
    [SerializeField] private int currentStarLevel = MIN_STAR_LEVEL;

    private BuildingRuntimeRegistry _buildingRegistry;

    // Tower 星级发生变化后触发。
    public event Action<int> OnStarLevelChanged;

    public int CurrentStarLevel => currentStarLevel;
    public int MaxStarLevel => upgradeConfig ? upgradeConfig.MaxStarLevel : currentStarLevel;
    public bool IsMaxStar => currentStarLevel >= MaxStarLevel;
    public TowerCombatSystem TowerCombatSystem => towerCombatSystem;
    public IReadOnlyList<ResourceCost> NextUpgradeCostList =>
        TryGetNextUpgradeLevel(out BuildingUpgradeLevel level)
            ? level.UpgradeCost
            : Array.Empty<ResourceCost>();

    private void Awake()
    {
        if (!building)
        {
            building = GetComponentInParent<Building>();
        }

        if (!towerCombatSystem)
        {
            towerCombatSystem = GetComponentInParent<TowerCombatSystem>();
        }
    }

    private void OnEnable()
    {
        _buildingRegistry?.RegisterUpgradeController(this);
    }

    private void OnDisable()
    {
        _buildingRegistry?.UnregisterUpgradeController(this);
    }

    private void Start()
    {
        CacheBuildingRegistry();
        _buildingRegistry?.RegisterUpgradeController(this);
        ApplyInitialStarBonusFromReward();
    }

    // 尝试消耗资源提升一级。
    public bool TryUpgradeWithCost(ResourceManager resourceManager)
    {
        if (!resourceManager || !TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel))
        {
            return false;
        }

        if (!resourceManager.CanAfford(upgradeLevel.UpgradeCost))
        {
            return false;
        }

        resourceManager.Spend(upgradeLevel.UpgradeCost);
        return ApplyStarLevel(currentStarLevel + 1);
    }

    // 无消耗提升一级。
    public bool UpgradeOneLevelWithoutCost()
    {
        return !IsMaxStar && ApplyStarLevel(currentStarLevel + 1);
    }

    // 无消耗提升到最高星级。
    public bool UpgradeToMaxStarWithoutCost()
    {
        return !IsMaxStar && ApplyStarLevel(MaxStarLevel);
    }

    // 应用奖励提供的初始星级加成。
    public bool ApplyInitialStarBonus(int initialStarBonus)
    {
        if (initialStarBonus <= 0 || IsMaxStar)
        {
            return false;
        }

        int targetStarLevel = Mathf.Clamp(
            currentStarLevel + initialStarBonus,
            MIN_STAR_LEVEL,
            MaxStarLevel);
        return ApplyStarLevel(targetStarLevel);
    }

    // 应用奖励运行时提供的新建 Tower 初始星级加成。
    private void ApplyInitialStarBonusFromReward()
    {
        if (!RewardRuntimeCoordinator.Instance)
        {
            return;
        }

        ApplyInitialStarBonus(
            RewardRuntimeCoordinator.Instance.TowerRewards.State.NewTowerInitialStarBonus);
    }

    // 缓存场景建筑注册表。
    private void CacheBuildingRegistry()
    {
        _buildingRegistry = BuildingPlacementManager.Instance
            ? BuildingPlacementManager.Instance.BuildingRegistry
            : null;
    }

    // 应用指定星级配置。
    private bool ApplyStarLevel(int targetStarLevel)
    {
        if (!upgradeConfig)
        {
            return false;
        }

        targetStarLevel = Mathf.Clamp(targetStarLevel, MIN_STAR_LEVEL, MaxStarLevel);
        if (targetStarLevel <= currentStarLevel)
        {
            return false;
        }

        BuildingUpgradeLevel upgradeLevel = upgradeConfig.GetUpgradeLevel(targetStarLevel);
        if (upgradeLevel == null)
        {
            return false;
        }

        currentStarLevel = targetStarLevel;
        if (building)
        {
            building.ApplyUpgradeLevel(upgradeLevel);
        }

        if (towerCombatSystem)
        {
            towerCombatSystem.ApplyUpgradeLevel(upgradeLevel);
        }

        OnStarLevelChanged?.Invoke(currentStarLevel);
        return true;
    }

    // 获取下一星级配置。
    private bool TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel)
    {
        upgradeLevel = upgradeConfig
            ? upgradeConfig.GetUpgradeLevel(currentStarLevel + 1)
            : null;
        return upgradeLevel != null;
    }
}
