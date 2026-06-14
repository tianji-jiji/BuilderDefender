using UnityEngine;

/// <summary>
/// 防御塔联动攻速 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Linked Attack Speed Handler")]
public class DefenseTowerLinkedAttackSpeedApplierSo : DefenseTowerRewardApplierSo
{
    // 应用联动攻速规则。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            return;
        }

        float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
        float linkRadius = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.LINK_RADIUS, 0f, true);
        state.AddLinkedAttackSpeed(attackSpeedBonus, linkRadius);
    }
}
