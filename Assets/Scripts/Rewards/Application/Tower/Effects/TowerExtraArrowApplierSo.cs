using UnityEngine;

/// <summary>
/// 防御塔额外箭奖励应用器，负责记录并执行攻击若干次后额外射箭的规则。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Extra Arrow Applier")]
public class TowerExtraArrowApplierSo : TowerRewardApplierSo, ITowerAttackCompletedTrigger
{
    private const string ATTACK_COUNTER_ID = "ExtraArrowAttack";

    // 应用额外攻击规则。
    public override void Apply(RewardApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetTowerRewardState(applyContext, out TowerRewardState state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        state.AddExtraAttackRule(triggerAttackCount, extraAttackCount);
    }

    // 按攻击次数触发额外普通箭。
    public void OnAttackCompleted(TowerEffectState runtimeState, TowerAttackCompletedContext context)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0);
        int extraAttackCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        if (triggerAttackCount <= 0 || extraAttackCount <= 0)
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
        context.RequestExtraAttack(extraAttackCount);
    }
}
