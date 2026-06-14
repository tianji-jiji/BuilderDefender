using UnityEngine;

/// <summary>
/// 防御塔攻击速度加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Speed Handler")]
public class DefenseTowerAttackSpeedHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用攻击速度加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
