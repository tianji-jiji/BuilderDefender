using UnityEngine;

/// <summary>
/// 防御塔最大生命奖励应用器，负责提高防御塔生命上限倍率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Max Health Handler")]
public class DefenseTowerMaxHealthApplierSo : DefenseTowerRewardApplierSo
{
    // 应用最大生命加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
