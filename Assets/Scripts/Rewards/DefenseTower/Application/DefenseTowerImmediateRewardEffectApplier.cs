/// <summary>
/// 防御塔即时奖励执行器，处理“立刻发生”的效果，比如随机一座塔升满星。
/// </summary>
public static class DefenseTowerImmediateRewardEffectApplier
{
    // 随机选择一座可升级防御塔并升到满星。
    public static void UpgradeRandomTowerToMaxStar()
    {
        BuildingUpgradeButton upgradeButton = DefenseTowerRegistry.GetRandomUpgradeableTower();
        if (!upgradeButton)
        {
            return;
        }

        upgradeButton.UpgradeToMaxStarWithoutCost();
    }
}
