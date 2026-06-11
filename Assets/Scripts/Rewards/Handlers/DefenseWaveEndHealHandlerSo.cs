using UnityEngine;

/// <summary>
/// 闃插尽濉旀尝鏈洖琛€ Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Wave End Heal Handler")]
public class DefenseWaveEndHealHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤娉㈡湯鍥炶姣斾緥銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float healPercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, GetValue(config));
            state.AddWaveEndHealPercent(healPercent);
        }
    }
}
