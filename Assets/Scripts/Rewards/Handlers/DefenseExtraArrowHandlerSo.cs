using UnityEngine;

/// <summary>
/// 防御塔额外攻击规则 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Extra Arrow Handler")]
public class DefenseExtraArrowHandlerSo : DefenseRewardHandlerSo
{
    // 应用额外攻击规则。
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
