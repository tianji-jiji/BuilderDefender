using UnityEngine;

/// <summary>
/// 防御塔额外箭奖励应用器，负责记录并执行攻击若干次后额外射箭的规则。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Extra Arrow Applier")]
public class DefenseTowerExtraArrowApplierSo : DefenseTowerRewardApplierSo, IDefenseTowerAttackCompletedRewardTrigger
{
    private const string ATTACK_COUNTER_ID = "ExtraArrowAttack";

    // 应用额外攻击规则。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerActiveRewards(applyContext, out DefenseTowerActiveRewards state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        state.AddExtraAttackRule(triggerAttackCount, extraAttackCount);
    }

    // 按攻击次数触发额外普通箭。
    public void OnAttackCompleted(DefenseTowerRewardRuntimeState runtimeState, DefenseTowerAttackCompletedContext context)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0);
        int extraAttackCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        if (triggerAttackCount <= 0 || extraAttackCount <= 0)
        {
            return;
        }

        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem = context.SourceDefenseTowerCombatSystem;
        int attackCount = runtimeState.IncrementCounter(sourceDefenseTowerCombatSystem, ATTACK_COUNTER_ID);
        if (attackCount < triggerAttackCount)
        {
            return;
        }

        runtimeState.ResetCounter(sourceDefenseTowerCombatSystem, ATTACK_COUNTER_ID);
        context.RequestExtraAttack(extraAttackCount);
    }
}
