using UnityEngine;

/// <summary>
/// 防御塔奖励建造成本计算器，负责计算奖励影响后的建造资源消耗。
/// </summary>
public static class TowerBuildCostCalculator
{
    // 计算建筑消耗经过防御塔奖励修正后的实际数量。
    public static int GetAdjustedAmount(
        BuildingSo buildingSo,
        ResourceCost resourceCost,
        TowerRewardState rewardState)
    {
        if (resourceCost == null)
        {
            return 0;
        }

        float costMultiplier = 1f;
        if (buildingSo
            && buildingSo.buildingType == BuildingSo.BuildingType.Defense
            && rewardState != null)
        {
            costMultiplier = rewardState.BuildCostMultiplier;
        }

        return Mathf.Max(0, Mathf.RoundToInt(resourceCost.amount * costMultiplier));
    }
}
