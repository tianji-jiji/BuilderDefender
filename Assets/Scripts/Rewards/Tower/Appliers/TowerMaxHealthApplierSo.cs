using UnityEngine;

/// <summary>
/// 防御塔最大生命奖励应用器，负责提高防御塔生命上限倍率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Max Health Applier")]
public class TowerMaxHealthApplierSo : TowerRewardApplierSo
{
    // 应用最大生命加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
