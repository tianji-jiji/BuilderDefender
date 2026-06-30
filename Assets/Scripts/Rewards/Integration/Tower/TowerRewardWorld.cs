using System.Collections.Generic;

/// <summary>
/// Tower 奖励与实际建筑场景之间的适配器。
/// </summary>
public sealed class TowerRewardWorld : ITowerRewardWorld
{
    private readonly BuildingRuntimeRegistry _registry;
    private readonly IRewardRandom _random;

    public float HomeHealthNormalized => _registry?.HomeHealthNormalized ?? 1f;

    // 创建使用指定建筑注册表和随机数来源的场景适配器。
    public TowerRewardWorld(BuildingRuntimeRegistry registry, IRewardRandom random)
    {
        _registry = registry;
        _random = random;
    }

    // 返回当前三星 Tower 数量。
    public int GetThreeStarTowerCount()
    {
        return _registry?.GetThreeStarTowerCount() ?? 0;
    }

    // 判断来源 Tower 附近是否存在其他 Tower。
    public bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius)
    {
        return _registry != null && _registry.HasNearbyTower(sourceTowerCombatSystem, radius);
    }

    // 随机选择一座可升级 Tower 并提升到满级。
    public bool UpgradeRandomTowerToMaxStar()
    {
        if (_registry == null || _random == null)
        {
            return false;
        }

        TowerUpgradeController selectedController = null;
        int eligibleCount = 0;
        foreach (TowerUpgradeController controller in _registry.UpgradeControllerList)
        {
            if (!controller || controller.IsMaxStar)
            {
                continue;
            }

            eligibleCount++;
            if (_random.Range(0, eligibleCount) == 0)
            {
                selectedController = controller;
            }
        }

        return selectedController && selectedController.UpgradeToMaxStarWithoutCost();
    }

    // 将指定 Tower 免费提升一级。
    public bool UpgradeTowerOneLevel(TowerCombatSystem sourceTowerCombatSystem)
    {
        TowerUpgradeController controller = _registry?.GetUpgradeController(sourceTowerCombatSystem);
        return controller && controller.UpgradeOneLevelWithoutCost();
    }

    // 按最大生命比例治疗全部防御建筑。
    public void HealAllTowers(float healPercent)
    {
        if (_registry == null || healPercent <= 0f)
        {
            return;
        }

        IReadOnlyList<Building> buildingList = _registry.DefenseBuildingList;
        foreach (Building building in buildingList)
        {
            if (building)
            {
                building.HealByMaxHealthPercent(healPercent);
            }
        }
    }
}
