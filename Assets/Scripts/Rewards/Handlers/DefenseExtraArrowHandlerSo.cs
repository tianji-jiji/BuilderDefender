using UnityEngine;

/// <summary>
/// 闃插尽濉旈澶栨敾鍑昏鍒?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Extra Arrow Handler")]
public class DefenseExtraArrowHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤棰濆鏀诲嚮瑙勫垯銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        state.AddExtraAttackRule(triggerAttackCount, extraAttackCount);
    }
}
