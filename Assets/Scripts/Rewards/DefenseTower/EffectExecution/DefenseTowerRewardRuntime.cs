/// <summary>
/// 防御塔奖励运行时，集中持有防御塔生效奖励和触发器。
/// </summary>
public class DefenseTowerRewardRuntime
{
    public DefenseTowerActiveRewards ActiveRewards { get; }
    public DefenseTowerRewardTriggerDispatcher TriggerDispatcher { get; }

    // 创建默认防御塔奖励运行时。
    public DefenseTowerRewardRuntime() : this(new DefenseTowerActiveRewards(), new DefenseTowerRewardTriggerDispatcher())
    {
    }

    // 创建使用指定账本和触发器的防御塔奖励运行时。
    public DefenseTowerRewardRuntime(DefenseTowerActiveRewards activeRewards, DefenseTowerRewardTriggerDispatcher triggerDispatcher)
    {
        ActiveRewards = activeRewards ?? new DefenseTowerActiveRewards();
        TriggerDispatcher = triggerDispatcher ?? new DefenseTowerRewardTriggerDispatcher();
    }

    // 计算防御塔建筑经过奖励修正后的建造消耗。
    public int GetAdjustedBuildCostAmount(BuildingSo buildingSo, ResourceCost resourceCost)
    {
        return DefenseTowerRewardedBuildCostCalculator.GetAdjustedAmount(buildingSo, resourceCost, ActiveRewards);
    }

    // 构建当前防御塔奖励摘要文本。
    public string BuildSummaryText()
    {
        return DefenseTowerRewardSummaryFormatter.BuildSummaryText(ActiveRewards);
    }

    // 创建当前防御体系战力快照。
    public DefenseTowerPowerSnapshot CreatePowerSnapshot()
    {
        return DefenseTowerPowerEvaluator.CreateSnapshot(ActiveRewards);
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        return ActiveRewards.GetFinalDefenseTowerAttackDamageMultiplier();
    }

    // 波次结束时转发给防御塔奖励触发器。
    public void OnWaveCompleted()
    {
        TriggerDispatcher.OnWaveCompleted();
    }
}
