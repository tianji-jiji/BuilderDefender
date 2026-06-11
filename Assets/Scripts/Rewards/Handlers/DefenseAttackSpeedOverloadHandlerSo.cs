using UnityEngine;

/// <summary>
/// 防御塔超载攻击速度 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Speed Overload Handler")]
public class DefenseAttackSpeedOverloadHandlerSo : DefenseRewardHandlerSo
{
    // 应用超载攻速变化。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
            state.AddAttackSpeedOverloadBonus(attackSpeedBonus);
        }
    }
}
