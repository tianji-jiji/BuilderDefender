using System.Collections.Generic;

/// <summary>
/// 奖励效果应用路由器，负责调用效果应用器并注册对应模块的运行时触发器。
/// </summary>
public static class RewardEffectApplicationRouter
{
    // 批量应用一张卡牌上的全部奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardCardEffectConfig> effectConfigList, RewardEffectApplyContext applyContext)
    {
        if (effectConfigList == null || applyContext == null)
        {
            return;
        }

        foreach (RewardCardEffectConfig effectConfig in effectConfigList)
        {
            ApplyEffect(effectConfig, applyContext);
        }
    }

    // 应用单个奖励效果配置并注册运行时触发器。
    private static void ApplyEffect(RewardCardEffectConfig effectConfig, RewardEffectApplyContext applyContext)
    {
        if (effectConfig == null || !effectConfig.EffectDefinition || !effectConfig.EffectDefinition.Applier)
        {
            return;
        }

        RewardEffectApplierSo applier = effectConfig.EffectDefinition.Applier;
        applier.Apply(applyContext, effectConfig);
        RegisterRuntimeTrigger(applier, effectConfig, applyContext);
    }

    // 根据应用器实现的触发接口注册到对应奖励模块。
    private static void RegisterRuntimeTrigger(RewardEffectApplierSo applier, RewardCardEffectConfig effectConfig, RewardEffectApplyContext applyContext)
    {
        if (applier is IDefenseTowerRewardTrigger defenseTowerRewardTrigger)
        {
            applyContext.DefenseTowerRewardTriggerDispatcher?.RegisterEffect(defenseTowerRewardTrigger, effectConfig);
        }

        if (applier is IResourceRewardTrigger resourceRewardTrigger)
        {
            applyContext.ResourceRewardModule?.TriggerDispatcher.RegisterEffect(resourceRewardTrigger, effectConfig);
        }

        if (applier is IHomeRewardTrigger homeRewardTrigger)
        {
            applyContext.HomeRewardModule?.TriggerDispatcher.RegisterEffect(homeRewardTrigger, effectConfig);
        }
    }
}
