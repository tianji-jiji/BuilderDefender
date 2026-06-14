using UnityEngine;

/// <summary>
/// 防御塔攻击范围加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Detect Radius Handler")]
public class DefenseTowerDetectRadiusHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用攻击范围加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            state.AddDetectRadiusBonus(GetValue(config));
        }
    }
}
