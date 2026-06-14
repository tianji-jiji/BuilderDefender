using UnityEngine;

/// <summary>
/// 防御塔最大生命加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Max Health Handler")]
public class DefenseTowerMaxHealthApplierSo : DefenseTowerRewardApplierSo
{
    // 应用最大生命加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
