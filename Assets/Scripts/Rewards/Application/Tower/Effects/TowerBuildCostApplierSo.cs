using UnityEngine;

/// <summary>
/// 防御塔建造成本奖励应用器，负责调整防御塔建造资源消耗倍率。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Build Cost Applier")]
public class TowerBuildCostApplierSo : TowerRewardApplierSo
{
    // 应用建造成本变化。
    public override void Apply(RewardApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetTowerRewardState(applyContext, out TowerRewardState state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
