using UnityEngine;

/// <summary>
/// 防御塔超载攻速奖励应用器，负责记录额外攻速加成或代价型攻速变化。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Attack Speed Overload Applier")]
public class TowerAttackSpeedOverloadApplierSo : TowerRewardApplierSo
{
    // 应用超载攻速变化。
    public override void Apply(RewardApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetTowerRewardState(applyContext, out TowerRewardState state))
        {
            float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
            state.AddAttackSpeedOverloadBonus(attackSpeedBonus);
        }
    }
}
