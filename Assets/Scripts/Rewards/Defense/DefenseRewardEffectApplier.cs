using System.Collections.Generic;

/// <summary>
/// 防御塔奖励效果应用器，负责把卡牌效果配置交给对应 Handler 写入奖励状态。
/// </summary>
public static class DefenseRewardEffectApplier
{
    // 批量应用奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardEffectConfig> effectConfigList, DefenseRewardState defenseRewardState, RewardEffectContext context = null)
    {
        if (effectConfigList == null || defenseRewardState == null)
        {
            return;
        }

        RewardEffectContext activeContext = context ?? new RewardEffectContext(null, defenseRewardState, ResourceManager.Instance, EnemyWaveManager.Instance, BuildManager.Instance);
        foreach (RewardEffectConfig effectConfig in effectConfigList)
        {
            ApplyEffect(effectConfig, activeContext);
        }
    }

    // 将单个效果交给定义资产上的 Handler 执行。
    private static void ApplyEffect(RewardEffectConfig effectConfig, RewardEffectContext context)
    {
        if (effectConfig == null || !effectConfig.EffectDefinition || !effectConfig.EffectDefinition.Handler)
        {
            return;
        }

        RewardEffectHandlerSo handler = effectConfig.EffectDefinition.Handler;
        handler.Apply(context, effectConfig);

        if (handler is IDefenseCardEffect defenseCardEffect)
        {
            context.DefenseCardEffectRuntime?.RegisterEffect(defenseCardEffect, effectConfig);
        }
    }
}
