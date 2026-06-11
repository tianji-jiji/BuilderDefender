using UnityEngine;

/// <summary>
/// 防御塔攻击范围加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Detect Radius Handler")]
public class DefenseDetectRadiusHandlerSo : DefenseRewardHandlerSo
{
    // 应用攻击范围加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddDetectRadiusBonus(GetValue(config));
        }
    }
}
