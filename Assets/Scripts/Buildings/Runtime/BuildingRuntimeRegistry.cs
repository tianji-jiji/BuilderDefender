using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 建筑运行时注册表，保存当前场景中的建筑、Tower、升级组件和基地生命。
/// </summary>
public sealed class BuildingRuntimeRegistry
{
    private const int THREE_STAR_LEVEL = 3;

    private readonly List<Building> _defenseBuildingList = new();
    private readonly List<TowerCombatSystem> _towerList = new();
    private readonly List<TowerUpgradeController> _upgradeControllerList = new();
    private HealthSystem _homeHealthSystem;

    public IReadOnlyList<Building> DefenseBuildingList => _defenseBuildingList;
    public IReadOnlyList<TowerUpgradeController> UpgradeControllerList => _upgradeControllerList;
    public float HomeHealthNormalized => _homeHealthSystem
        ? _homeHealthSystem.CurrentHealthNormalized
        : 1f;

    // 注册防御建筑。
    public void RegisterBuilding(Building building)
    {
        AddUnique(_defenseBuildingList, building);
    }

    // 注销防御建筑。
    public void UnregisterBuilding(Building building)
    {
        _defenseBuildingList.Remove(building);
    }

    // 注册 Tower 战斗组件。
    public void RegisterTower(TowerCombatSystem towerCombatSystem)
    {
        AddUnique(_towerList, towerCombatSystem);
    }

    // 注销 Tower 战斗组件。
    public void UnregisterTower(TowerCombatSystem towerCombatSystem)
    {
        _towerList.Remove(towerCombatSystem);
    }

    // 注册 Tower 升级组件。
    public void RegisterUpgradeController(TowerUpgradeController upgradeController)
    {
        AddUnique(_upgradeControllerList, upgradeController);
    }

    // 注销 Tower 升级组件。
    public void UnregisterUpgradeController(TowerUpgradeController upgradeController)
    {
        _upgradeControllerList.Remove(upgradeController);
    }

    // 注册基地生命组件。
    public void RegisterHomeHealth(HealthSystem healthSystem)
    {
        if (healthSystem)
        {
            _homeHealthSystem = healthSystem;
        }
    }

    // 注销基地生命组件。
    public void UnregisterHomeHealth(HealthSystem healthSystem)
    {
        if (_homeHealthSystem == healthSystem)
        {
            _homeHealthSystem = null;
        }
    }

    // 返回当前三星 Tower 数量。
    public int GetThreeStarTowerCount()
    {
        int count = 0;
        foreach (TowerCombatSystem tower in _towerList)
        {
            if (tower && tower.CurrentStarLevel >= THREE_STAR_LEVEL)
            {
                count++;
            }
        }

        return count;
    }

    // 判断来源 Tower 附近是否存在其他 Tower。
    public bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius)
    {
        if (!sourceTowerCombatSystem || radius <= 0f)
        {
            return false;
        }

        float radiusSqr = radius * radius;
        Vector3 sourcePosition = sourceTowerCombatSystem.transform.position;
        foreach (TowerCombatSystem tower in _towerList)
        {
            if (!tower || tower == sourceTowerCombatSystem)
            {
                continue;
            }

            if ((tower.transform.position - sourcePosition).sqrMagnitude <= radiusSqr)
            {
                return true;
            }
        }

        return false;
    }

    // 返回指定 Tower 对应的升级组件。
    public TowerUpgradeController GetUpgradeController(TowerCombatSystem towerCombatSystem)
    {
        if (!towerCombatSystem)
        {
            return null;
        }

        foreach (TowerUpgradeController controller in _upgradeControllerList)
        {
            if (controller && controller.TowerCombatSystem == towerCombatSystem)
            {
                return controller;
            }
        }

        return null;
    }

    // 向列表中添加唯一且有效的 Unity 对象。
    private static void AddUnique<T>(List<T> targetList, T item) where T : Object
    {
        if (item && !targetList.Contains(item))
        {
            targetList.Add(item);
        }
    }
}
