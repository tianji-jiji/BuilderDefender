using System.Collections.Generic;

/// <summary>
/// 奖励效果应用服务，负责调用效果应用器并注册对应运行时触发器。
/// </summary>
public static class RewardEffectApplicationService
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

    // 检查这个奖励效果的 applier 属于哪一类“运行时触发效果”，然后把它登记到对应运行时的 TriggerDispatcher 里
    // 有些奖励不是选中卡牌后马上一次性完成，而是要在后续游戏过程中触发
    private static void RegisterRuntimeTrigger(RewardEffectApplierSo applier, RewardCardEffectConfig effectConfig, RewardEffectApplyContext applyContext)
    {
        if (applier is IDefenseTowerRuntimeReward defenseTowerRuntimeReward)
        {
            applyContext.DefenseTowerRewardRuntime?.TriggerDispatcher.RegisterEffect(defenseTowerRuntimeReward, effectConfig);
        }

        if (applier is IResourceRewardTrigger resourceRewardTrigger)
        {
            applyContext.ResourceRewardRuntime?.TriggerDispatcher.RegisterEffect(resourceRewardTrigger, effectConfig);
        }

        if (applier is IHomeRewardTrigger homeRewardTrigger)
        {
            applyContext.HomeRewardRuntime?.TriggerDispatcher.RegisterEffect(homeRewardTrigger, effectConfig);
        }
    }
}
