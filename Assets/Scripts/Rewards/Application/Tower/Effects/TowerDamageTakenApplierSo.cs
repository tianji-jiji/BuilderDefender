using UnityEngine;

/// <summary>
/// 防御塔受伤倍率奖励应用器，负责调整防御塔承受伤害的倍率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Damage Taken Applier")]
public class TowerDamageTakenApplierSo : TowerRewardApplierSo
{
    // 应用受到伤害变化。
    public override void Apply(RewardApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetTowerRewardState(applyContext, out TowerRewardState state))
        {
            float damageTakenBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER, GetValue(config));
            state.AddDamageTakenBonus(damageTakenBonus);
        }
    }
}
