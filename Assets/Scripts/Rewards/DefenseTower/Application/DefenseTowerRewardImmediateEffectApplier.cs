/// <summary>
/// 防御塔即时奖励执行器，负责处理选择卡牌后立刻发生的防御塔效果。
/// </summary>
public static class DefenseTowerRewardImmediateEffectApplier
{
    // 随机选择一座可升级防御塔并升到满星。
    public static void UpgradeRandomTowerToMaxStar()
    {
        BuildingUpgradeButton upgradeButton = DefenseTowerTracker.GetRandomUpgradeableTower();
        if (!upgradeButton)
        {
            return;
        }

        upgradeButton.UpgradeToMaxStarWithoutCost();
    }
}
