using UnityEngine;

/// <summary>
/// 防御塔燃烧箭奖励应用器，负责让箭矢概率施加燃烧状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Burn Arrow Applier")]
public class DefenseTowerBurnArrowApplierSo : DefenseTowerRuntimeRewardApplierSo
{
    // 修改单支箭的燃烧能力。
    public override void ModifyArrow(DefenseTowerRewardTriggerInstance instance, DefenseTowerArrowContext context)
    {
        if (context == null || !ShouldTrigger(instance.Config))
        {
            return;
        }

        EnemyStatusEffectSpec statusEffectSpec = new(
            EnemyStatusEffectType.Burn,
            GetStatusDuration(instance.Config),
            GetTickInterval(instance.Config),
            GetTickDamagePercent(instance.Config),
            context.SourceDefenseTowerCombatSystem,
            DamageFloatingTextStyle.Burn);

        context.AddStatusEffect(statusEffectSpec);
    }

    // 判断本支箭是否触发燃烧。
    private bool ShouldTrigger(RewardCardEffectConfig config)
    {
        float triggerChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TRIGGER_CHANCE, GetValue(config));
        return triggerChance > 0f && Random.value < Mathf.Clamp01(triggerChance);
    }

    // 获取燃烧持续时间。
    private float GetStatusDuration(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.STATUS_DURATION, 0f, true);
    }

    // 获取燃烧跳伤间隔。
    private float GetTickInterval(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TICK_INTERVAL, 0f, true);
    }

    // 获取燃烧单次跳伤百分比。
    private float GetTickDamagePercent(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TICK_DAMAGE, 0f, true);
    }
}
