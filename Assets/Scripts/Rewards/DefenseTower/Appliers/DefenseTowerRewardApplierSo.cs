/// <summary>
/// 防御塔奖励应用器基类，负责连接奖励配置和防御塔奖励状态。
/// </summary>
public abstract class DefenseTowerRewardApplierSo : RewardEffectApplierSo
{
    // 应用奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
    }

    // 读取当前效果的主数值。
    protected float GetValue(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 尝试取得防御塔奖励状态。
    protected bool TryGetDefenseTowerRewardState(RewardEffectApplyContext applyContext, out DefenseTowerActiveRewards defenseTowerActiveRewards)
    {
        defenseTowerActiveRewards = applyContext?.DefenseTowerActiveRewards;
        return defenseTowerActiveRewards != null;
    }
}
