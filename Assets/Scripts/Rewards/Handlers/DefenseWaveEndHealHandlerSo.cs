using UnityEngine;

/// <summary>
/// 防御塔波末回血 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Wave End Heal Handler")]
public class DefenseWaveEndHealHandlerSo : DefenseRewardHandlerSo
{
    public override bool ShouldRegisterRuntimeEffect => true;

    // 应用波末回血比例。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float healPercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(config));
            state.AddWaveEndHealPercent(healPercent);
        }
    }

    // 在波次结束时治疗所有防御塔。
    public override void OnWaveCompleted(DefenseCardEffectInstance instance, DefenseWaveContext context)
    {
        float healPercent = RewardEffectParameterReader.GetFloat(instance.Config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(instance.Config));
        DefenseRewardWaveEndApplier.ApplyWaveEndHeal(healPercent);
    }
}
