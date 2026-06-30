using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励效果应用服务，负责调用效果应用器并注册对应运行时触发器。
/// </summary>
public static class RewardEffectApplicationService
{
    // 批量应用一张卡牌上的全部奖励效果配置。
    public static void ApplyEffects(
        IReadOnlyList<RewardCardEffectConfig> effectConfigList,
        RewardApplyContext applyContext)
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

    // 应用单个效果并注册其运行时能力。
    private static void ApplyEffect(
        RewardCardEffectConfig effectConfig,
        RewardApplyContext applyContext)
    {
        if (effectConfig == null)
        {
            return;
        }

        RewardEffectDefinitionSo definition = effectConfig.EffectDefinition;
        if (!definition)
        {
            Debug.LogWarning("奖励效果缺少 EffectDefinition，已跳过。");
            return;
        }

        RewardEffectApplierSo applier = definition.Applier;
        if (!applier)
        {
            Debug.LogWarning($"奖励效果 {definition.name} 缺少 Applier，已跳过。", definition);
            return;
        }

        applier.Apply(applyContext, effectConfig);
        if (applier is ITowerRuntimeReward towerRuntimeReward)
        {
            applyContext.TowerRewardRuntime?.TriggerDispatcher.RegisterEffect(
                towerRuntimeReward,
                effectConfig);
        }
    }
}
