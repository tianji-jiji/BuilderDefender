using System.Collections.Generic;

/// <summary>
/// 防御塔奖励配置应用器，负责把卡牌效果配置交给对应 Handler 写入加成集合。
/// </summary>
public static class DefenseTowerRewardConfigApplier
{
    // 批量应用奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardEffectConfig> effectConfigList, DefenseTowerRewardModifiers defenseTowerRewardModifiers, RewardEffectContext context = null)
    {
        if (effectConfigList == null || defenseTowerRewardModifiers == null)
        {
            return;
        }

        RewardEffectContext activeContext = context ?? new RewardEffectContext(null, defenseTowerRewardModifiers, ResourceManager.Instance, EnemyWaveManager.Instance, BuildManager.Instance);
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

        if (handler is IDefenseTowerCardEffect defenseCardEffect)
        {
            context.DefenseTowerCardEffectDispatcher?.RegisterEffect(defenseCardEffect, effectConfig);
        }
    }
}
