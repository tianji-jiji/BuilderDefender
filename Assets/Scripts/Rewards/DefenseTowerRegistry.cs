using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 防御塔运行时注册表，负责登记场上防御塔并提供奖励系统查询。
/// </summary>
public static class DefenseTowerRegistry
{
    private static readonly List<DefenseSystem> DefenseSystemList = new();
    private static readonly List<Building> DefenseBuildingList = new();
    private static readonly List<BuildingUpgradeButton> UpgradeButtonList = new();
    private static HealthSystem HomeHealthSystem;

    public static float HomeHealthNormalized => HomeHealthSystem ? HomeHealthSystem.CurrentHealthNormalized : 1f;

    // 注册防御塔战斗系统。
    public static void RegisterDefenseSystem(DefenseSystem defenseSystem)
    {
        AddUnique(DefenseSystemList, defenseSystem);
    }

    // 取消注册防御塔战斗系统。
    public static void UnregisterDefenseSystem(DefenseSystem defenseSystem)
    {
        DefenseSystemList.Remove(defenseSystem);
    }

    // 注册防御塔建筑。
    public static void RegisterDefenseBuilding(Building building)
    {
        AddUnique(DefenseBuildingList, building);
    }

    // 取消注册防御塔建筑。
    public static void UnregisterDefenseBuilding(Building building)
    {
        DefenseBuildingList.Remove(building);
    }

    // 注册升星按钮。
    public static void RegisterUpgradeButton(BuildingUpgradeButton upgradeButton)
    {
        AddUnique(UpgradeButtonList, upgradeButton);
    }

    // 取消注册升星按钮。
    public static void UnregisterUpgradeButton(BuildingUpgradeButton upgradeButton)
    {
        UpgradeButtonList.Remove(upgradeButton);
    }

    // 注册基地生命系统。
    public static void RegisterHomeHealthSystem(HealthSystem healthSystem)
    {
        if (healthSystem)
        {
            HomeHealthSystem = healthSystem;
        }
    }

    // 取消注册基地生命系统。
    public static void UnregisterHomeHealthSystem(HealthSystem healthSystem)
    {
        if (HomeHealthSystem == healthSystem)
        {
            HomeHealthSystem = null;
        }
    }

    // 统计当前三星防御塔数量。
    public static int GetThreeStarDefenseCount()
    {
        int count = 0;
        foreach (DefenseSystem defenseSystem in DefenseSystemList)
        {
            if (defenseSystem && defenseSystem.CurrentStarLevel >= 3)
            {
                count++;
            }
        }

        return count;
    }

    // 判断指定防御塔附近是否存在另一座防御塔。
    public static bool HasNearbyDefenseTower(DefenseSystem sourceDefenseSystem, float radius)
    {
        if (!sourceDefenseSystem || radius <= 0f)
        {
            return false;
        }

        float radiusSqr = radius * radius;
        Vector3 sourcePosition = sourceDefenseSystem.transform.position;

        foreach (DefenseSystem defenseSystem in DefenseSystemList)
        {
            if (!defenseSystem || defenseSystem == sourceDefenseSystem)
            {
                continue;
            }

            if ((defenseSystem.transform.position - sourcePosition).sqrMagnitude <= radiusSqr)
            {
                return true;
            }
        }

        return false;
    }

    // 获取一座可升星的随机防御塔。
    public static BuildingUpgradeButton GetRandomUpgradeableTower()
    {
        List<BuildingUpgradeButton> upgradeableButtonList = new List<BuildingUpgradeButton>();
        foreach (BuildingUpgradeButton upgradeButton in UpgradeButtonList)
        {
            if (upgradeButton && !upgradeButton.IsMaxStar)
            {
                upgradeableButtonList.Add(upgradeButton);
            }
        }

        if (upgradeableButtonList.Count <= 0)
        {
            return null;
        }

        int index = Random.Range(0, upgradeableButtonList.Count);
        return upgradeableButtonList[index];
    }

    // 获取指定防御塔对应的升星按钮。
    public static BuildingUpgradeButton GetUpgradeButton(DefenseSystem defenseSystem)
    {
        if (!defenseSystem)
        {
            return null;
        }

        foreach (BuildingUpgradeButton upgradeButton in UpgradeButtonList)
        {
            if (upgradeButton && upgradeButton.DefenseSystem == defenseSystem)
            {
                return upgradeButton;
            }
        }

        return null;
    }

    // 按最大生命百分比治疗所有防御塔。
    public static void HealAllDefenseTowers(float healPercent)
    {
        if (healPercent <= 0f)
        {
            return;
        }

        foreach (Building building in DefenseBuildingList)
        {
            if (building)
            {
                building.HealByMaxHealthPercent(healPercent);
            }
        }
    }

    // 向列表添加唯一元素。
    private static void AddUnique<T>(List<T> targetList, T item) where T : Object
    {
        if (!item || targetList.Contains(item))
        {
            return;
        }

        targetList.Add(item);
    }
}
