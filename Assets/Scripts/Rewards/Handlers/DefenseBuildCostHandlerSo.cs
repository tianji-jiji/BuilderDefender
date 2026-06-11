using UnityEngine;

/// <summary>
/// 防御塔建造成本变化 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Build Cost Handler")]
public class DefenseBuildCostHandlerSo : DefenseRewardHandlerSo
{
    // 应用建造成本变化。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
