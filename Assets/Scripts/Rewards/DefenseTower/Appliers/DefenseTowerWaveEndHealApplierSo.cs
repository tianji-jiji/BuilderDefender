using UnityEngine;

/// <summary>
/// 防御塔波末回血奖励应用器，负责在波次结束时治疗所有防御塔。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Wave End Heal Applier")]
public class DefenseTowerWaveEndHealApplierSo : DefenseTowerRewardApplierSo, IDefenseTowerWaveCompletedRewardTrigger
{
    // 应用波末回血比例。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out DefenseTowerActiveRewards state))
        {
            float healPercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(config));
            state.AddWaveEndHealPercent(healPercent);
        }
    }

    // 在波次结束时治疗所有防御塔。
    public void OnWaveCompleted(DefenseTowerRewardRuntimeState runtimeState)
    {
        float healPercent = RewardEffectParameterReader.GetFloat(runtimeState.Config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(runtimeState.Config));
        DefenseTowerWaveEndRewardEffectApplier.ApplyWaveEndHeal(healPercent);
    }
}
