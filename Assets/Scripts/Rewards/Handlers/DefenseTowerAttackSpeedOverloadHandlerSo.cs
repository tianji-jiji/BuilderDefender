using UnityEngine;

/// <summary>
/// 防御塔超载攻击速度 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Speed Overload Handler")]
public class DefenseTowerAttackSpeedOverloadHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用超载攻速变化。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
            state.AddAttackSpeedOverloadBonus(attackSpeedBonus);
        }
    }
}
