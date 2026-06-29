using UnityEngine;

/// <summary>
/// 防御塔攻击扣血奖励应用器，负责记录并执行攻击若干次后损失生命的规则。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Attack Health Cost Applier")]
public class TowerAttackHealthCostApplierSo : TowerRewardApplierSo, ITowerAttackCompletedTrigger
{
    private const string ATTACK_COUNTER_ID = "AttackHealthCost";

    // 应用攻击扣血规则。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 1);
        int healthCost = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.ATTACK_HEALTH_COST, Mathf.RoundToInt(GetValue(config)));
        state.AddAttackHealthCostRule(triggerAttackCount, healthCost);
    }

    // 按攻击次数扣除防御塔生命。
    public void OnAttackCompleted(TowerRewardRuntimeState runtimeState, TowerAttackCompletedContext context)
    {
        if (context.SourceHealthSystem == null)
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 1);
        int healthCost = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.ATTACK_HEALTH_COST, Mathf.RoundToInt(GetValue(runtimeState.Config)));
        if (triggerAttackCount <= 0 || healthCost <= 0)
        {
            return;
        }

        TowerCombatSystem sourceTowerCombatSystem = context.SourceTowerCombatSystem;
        int attackCount = runtimeState.IncrementCounter(sourceTowerCombatSystem, ATTACK_COUNTER_ID);
        if (attackCount < triggerAttackCount)
        {
            return;
        }

        runtimeState.ResetCounter(sourceTowerCombatSystem, ATTACK_COUNTER_ID);
        context.SourceHealthSystem.LoseHealth(healthCost);
    }
}
