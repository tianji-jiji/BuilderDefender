using UnityEngine;

/// <summary>
/// 防御塔攻击扣血规则 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Health Cost Handler")]
public class DefenseTowerAttackHealthCostApplierSo : DefenseTowerRewardApplierSo
{
    private const string ATTACK_COUNTER_ID = "AttackHealthCost";

    public override bool ShouldRegisterRuntimeEffect => true;

    // 应用攻击扣血规则。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 1);
        int healthCost = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.ATTACK_HEALTH_COST, Mathf.RoundToInt(GetValue(config)));
        state.AddAttackHealthCostRule(triggerAttackCount, healthCost);
    }

    // 按攻击次数扣除防御塔生命。
    public override void OnAfterAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context)
    {
        if (context.SourceHealthSystem == null)
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 1);
        int healthCost = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.ATTACK_HEALTH_COST, Mathf.RoundToInt(GetValue(instance.Config)));
        if (triggerAttackCount <= 0 || healthCost <= 0)
        {
            return;
        }

        int attackCount = instance.IncrementCounter(ATTACK_COUNTER_ID);
        if (attackCount < triggerAttackCount)
        {
            return;
        }

        instance.ResetCounter(ATTACK_COUNTER_ID);
        context.SourceHealthSystem.LoseHealth(healthCost);
    }
}
