using UnityEngine;

/// <summary>
/// 防御塔建造成本变化 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Build Cost Handler")]
public class DefenseTowerBuildCostApplierSo : DefenseTowerRewardApplierSo
{
    // 应用建造成本变化。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
