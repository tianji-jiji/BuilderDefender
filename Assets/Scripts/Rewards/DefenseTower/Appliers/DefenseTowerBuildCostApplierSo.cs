using UnityEngine;

/// <summary>
/// 防御塔建造成本奖励应用器，负责调整防御塔建造资源消耗倍率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Build Cost Applier")]
public class DefenseTowerBuildCostApplierSo : DefenseTowerRewardApplierSo
{
    // 应用建造成本变化。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out DefenseTowerActiveRewards state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
