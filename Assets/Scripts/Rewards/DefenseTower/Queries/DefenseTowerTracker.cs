using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 防御塔跟踪器，负责记录场上的防御塔、升星按钮和基地生命。
/// </summary>
public static class DefenseTowerTracker
{
    private static readonly List<DefenseTowerSystem> DefenseTowerSystemList = new();
    private static readonly List<Building> DefenseTowerBuildingList = new();
    private static readonly List<BuildingUpgradeButton> UpgradeButtonList = new();
    private static HealthSystem HomeHealthSystem;

    public static float HomeHealthNormalized => HomeHealthSystem ? HomeHealthSystem.CurrentHealthNormalized : 1f;

    // 注册防御塔战斗系统。
    public static void RegisterDefenseTowerSystem(DefenseTowerSystem defenseTowerSystem)
    {
        AddUnique(DefenseTowerSystemList, defenseTowerSystem);
    }

    // 取消注册防御塔战斗系统。
    public static void UnregisterDefenseTowerSystem(DefenseTowerSystem defenseTowerSystem)
    {
        DefenseTowerSystemList.Remove(defenseTowerSystem);
    }

    // 注册防御塔建筑。
    public static void RegisterDefenseBuilding(Building building)
    {
        AddUnique(DefenseTowerBuildingList, building);
    }

    // 取消注册防御塔建筑。
    public static void UnregisterDefenseBuilding(Building building)
    {
        DefenseTowerBuildingList.Remove(building);
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
    public static int GetThreeStarDefenseTowerCount()
    {
        int count = 0;
        foreach (DefenseTowerSystem defenseTowerSystem in DefenseTowerSystemList)
        {
            if (defenseTowerSystem && defenseTowerSystem.CurrentStarLevel >= 3)
            {
                count++;
            }
        }

        return count;
    }

    // 判断指定防御塔附近是否存在另一座防御塔。
    public static bool HasNearbyDefenseTower(DefenseTowerSystem sourceDefenseTowerSystem, float radius)
    {
        if (!sourceDefenseTowerSystem || radius <= 0f)
        {
            return false;
        }

        float radiusSqr = radius * radius;
        Vector3 sourcePosition = sourceDefenseTowerSystem.transform.position;

        foreach (DefenseTowerSystem defenseTowerSystem in DefenseTowerSystemList)
        {
            if (!defenseTowerSystem || defenseTowerSystem == sourceDefenseTowerSystem)
            {
                continue;
            }

            if ((defenseTowerSystem.transform.position - sourcePosition).sqrMagnitude <= radiusSqr)
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
    public static BuildingUpgradeButton GetUpgradeButton(DefenseTowerSystem defenseTowerSystem)
    {
        if (!defenseTowerSystem)
        {
            return null;
        }

        foreach (BuildingUpgradeButton upgradeButton in UpgradeButtonList)
        {
            if (upgradeButton && upgradeButton.DefenseTowerSystem == defenseTowerSystem)
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

        foreach (Building building in DefenseTowerBuildingList)
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
