using UnityEngine;

/// <summary>
/// 防御塔中毒箭奖励应用器，负责让箭矢概率施加中毒状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Poison Arrow Applier")]
public class TowerPoisonArrowApplierSo : TowerRewardApplierSo, ITowerArrowModifier
{
    // 修改单支箭的中毒能力。
    public void ModifyArrow(TowerRewardRuntimeState runtimeState, TowerArrowContext context)
    {
        if (context == null || !ShouldTrigger(runtimeState.Config))
        {
            return;
        }

        EnemyStatusEffectSpec statusEffectSpec = new(
            EnemyStatusEffectType.Poison,
            GetStatusDuration(runtimeState.Config),
            GetTickInterval(runtimeState.Config),
            GetTickDamagePercent(runtimeState.Config),
            context.SourceTowerCombatSystem,
            DamageFloatingTextStyle.Poison);

        context.AddStatusEffect(statusEffectSpec);
    }

    // 判断本支箭是否触发中毒。
    private bool ShouldTrigger(RewardCardEffectConfig config)
    {
        float triggerChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TRIGGER_CHANCE, GetValue(config));
        return triggerChance > 0f && Random.value < Mathf.Clamp01(triggerChance);
    }

    // 获取中毒持续时间。
    private float GetStatusDuration(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.STATUS_DURATION, 0f, true);
    }

    // 获取中毒跳伤间隔。
    private float GetTickInterval(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TICK_INTERVAL, 0f, true);
    }

    // 获取中毒单次跳伤百分比。
    private float GetTickDamagePercent(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TICK_DAMAGE, 0f, true);
    }
}
