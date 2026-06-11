/// <summary>
/// 防御塔奖励 Handler 基类，提供读取参数和奖励状态的通用方法。
/// </summary>
public abstract class DefenseRewardHandlerSo : RewardEffectHandlerSo
{
    // 读取当前效果的主数值。
    protected float GetValue(RewardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 尝试取得防御塔奖励状态。
    protected bool TryGetDefenseRewardState(RewardEffectContext context, out DefenseRewardState defenseRewardState)
    {
        defenseRewardState = context?.DefenseRewardState;
        return defenseRewardState != null;
    }
}
