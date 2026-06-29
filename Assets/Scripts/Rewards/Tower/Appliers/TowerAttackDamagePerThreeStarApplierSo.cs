using UnityEngine;

/// <summary>
/// 三星塔共鸣奖励应用器，负责让每座三星防御塔提供额外攻击力。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Attack Damage Per Three Star Applier")]
public class TowerAttackDamagePerThreeStarApplierSo : TowerRewardApplierSo
{
    // 应用每座三星塔攻击加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            float damageBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_BONUS_PER_THREE_STAR_TOWER, GetValue(config));
            state.AddAttackDamagePerThreeStarTower(damageBonus);
        }
    }
}
