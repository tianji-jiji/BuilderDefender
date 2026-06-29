/// <summary>
/// 防御塔奖励应用器基类，负责连接奖励配置和防御塔奖励状态。
/// </summary>
public abstract class TowerRewardApplierSo : RewardEffectApplierSo
{
    // 应用奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
    }

    // 读取当前效果的主数值。
    protected float GetValue(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, 0f);
    }

    // 尝试取得防御塔奖励状态。
    protected bool TryGetDefenseTowerActiveRewards(RewardEffectApplyContext applyContext, out TowerActiveRewards towerActiveRewards)
    {
        towerActiveRewards = applyContext?.TowerActiveRewards;
        return towerActiveRewards != null;
    }
}
