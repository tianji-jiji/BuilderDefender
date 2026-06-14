using UnityEngine;

/// <summary>
/// 防御塔波末回血 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Wave End Heal Handler")]
public class DefenseTowerWaveEndHealApplierSo : DefenseTowerRewardApplierSo
{
    public override bool ShouldRegisterRuntimeEffect => true;

    // 应用波末回血比例。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            float healPercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(config));
            state.AddWaveEndHealPercent(healPercent);
        }
    }

    // 在波次结束时治疗所有防御塔。
    public override void OnWaveCompleted(DefenseTowerRuntimeEffectInstance instance, DefenseTowerWaveContext context)
    {
        float healPercent = RewardEffectParameterReader.GetFloat(instance.Config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(instance.Config));
        DefenseTowerWaveEndRewardEffectApplier.ApplyWaveEndHeal(healPercent);
    }
}
