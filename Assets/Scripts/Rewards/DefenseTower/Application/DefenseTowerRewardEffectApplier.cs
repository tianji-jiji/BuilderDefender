using System.Collections.Generic;

/// <summary>
/// 把卡牌配置应用到奖励系统的入口，拿到一张卡上的效果配置，然后交给对应的 Handler 去执行
/// </summary>
public static class DefenseTowerRewardEffectApplier
{
    // 批量应用奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardCardEffectConfig> effectConfigList, DefenseTowerRewardState defenseTowerRewardState, RewardEffectApplyContext applyContext = null)
    {
        if (effectConfigList == null || defenseTowerRewardState == null)
        {
            return;
        }

        RewardEffectApplyContext activeApplyContext = applyContext ?? new RewardEffectApplyContext(null, defenseTowerRewardState, ResourceManager.Instance, EnemyWaveManager.Instance, BuildingPlacementManager.Instance);
        foreach (RewardCardEffectConfig effectConfig in effectConfigList)
        {
            ApplyEffect(effectConfig, activeApplyContext);
        }
    }

    // 将单个效果交给定义资产上的 Handler 执行。
    private static void ApplyEffect(RewardCardEffectConfig cardEffectConfig, RewardEffectApplyContext applyContext)
    {
        if (cardEffectConfig == null || !cardEffectConfig.EffectDefinition || !cardEffectConfig.EffectDefinition.Applier)
        {
            return;
        }

        RewardEffectApplierSo applier = cardEffectConfig.EffectDefinition.Applier;
        applier.Apply(applyContext, cardEffectConfig);

        if (applier is IDefenseTowerRuntimeEffect defenseCardEffect)
        {
            applyContext.DefenseTowerRuntimeEffectDispatcher?.RegisterEffect(defenseCardEffect, cardEffectConfig);
        }
    }
}
