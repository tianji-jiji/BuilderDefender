using UnityEngine;

/// <summary>
/// 防御塔最大生命加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Max Health Handler")]
public class DefenseTowerMaxHealthHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用最大生命加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
