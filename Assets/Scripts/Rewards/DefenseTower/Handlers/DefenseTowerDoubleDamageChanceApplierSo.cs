using UnityEngine;

/// <summary>
/// 防御塔双倍伤害概率 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Double Damage Chance Handler")]
public class DefenseTowerDoubleDamageChanceApplierSo : DefenseTowerRewardApplierSo
{
    // 应用双倍伤害概率。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            float doubleDamageChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE, GetValue(config));
            state.AddDoubleDamageChance(doubleDamageChance);
        }
    }
}
