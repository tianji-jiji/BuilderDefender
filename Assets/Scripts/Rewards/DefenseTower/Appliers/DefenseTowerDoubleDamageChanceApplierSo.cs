using UnityEngine;

/// <summary>
/// 防御塔双倍伤害奖励应用器，负责记录箭矢触发双倍伤害的概率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Double Damage Chance Applier")]
public class DefenseTowerDoubleDamageChanceApplierSo : DefenseTowerRewardApplierSo
{
    // 应用双倍伤害概率。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out DefenseTowerActiveRewards state))
        {
            float doubleDamageChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE, GetValue(config));
            state.AddDoubleDamageChance(doubleDamageChance);
        }
    }
}
