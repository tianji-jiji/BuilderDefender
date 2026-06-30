using UnityEngine;

/// <summary>
/// 防线联动攻速奖励应用器，负责让附近有其他防御塔时获得攻速加成。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Linked Attack Speed Applier")]
public class TowerLinkedAttackSpeedApplierSo : TowerRewardApplierSo
{
    // 应用联动攻速规则。
    public override void Apply(RewardApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetTowerRewardState(applyContext, out TowerRewardState state))
        {
            return;
        }

        float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
        float linkRadius = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.LINK_RADIUS, 0f, true);
        state.AddLinkedAttackSpeed(attackSpeedBonus, linkRadius);
    }
}
