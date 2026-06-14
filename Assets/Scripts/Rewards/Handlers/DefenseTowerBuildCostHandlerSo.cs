using UnityEngine;

/// <summary>
/// 防御塔建造成本变化 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Build Cost Handler")]
public class DefenseTowerBuildCostHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用建造成本变化。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
