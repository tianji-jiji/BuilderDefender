using UnityEngine;

/// <summary>
/// 防御塔攻击范围加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Detect Radius Handler")]
public class DefenseTowerDetectRadiusApplierSo : DefenseTowerRewardApplierSo
{
    // 应用攻击范围加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            state.AddDetectRadiusBonus(GetValue(config));
        }
    }
}
