using System.Collections.Generic;

/// <summary>
/// 防御塔奖励效果应用入口，兼容旧调用并转交给通用奖励效果路由器。
/// </summary>
public static class DefenseTowerRewardEffectApplier
{
    // 批量应用奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardCardEffectConfig> effectConfigList, DefenseTowerActiveRewards defenseTowerActiveRewards, RewardEffectApplyContext applyContext = null)
    {
        if (effectConfigList == null || applyContext == null)
        {
            return;
        }

        RewardEffectApplicationRouter.ApplyEffects(effectConfigList, applyContext);
    }
}
