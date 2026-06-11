using UnityEngine;

/// <summary>
/// 闃插尽濉旇仈鍔ㄦ敾閫?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Linked Attack Speed Handler")]
public class DefenseLinkedAttackSpeedHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鑱斿姩鏀婚€熻鍒欍€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            return;
        }

        float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
        float linkRadius = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.LINK_RADIUS, 0f, true);
        state.AddLinkedAttackSpeed(attackSpeedBonus, linkRadius);
    }
}
