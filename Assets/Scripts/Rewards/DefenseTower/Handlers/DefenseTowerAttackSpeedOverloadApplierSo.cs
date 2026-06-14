using UnityEngine;

/// <summary>
/// 防御塔超载攻击速度 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Speed Overload Handler")]
public class DefenseTowerAttackSpeedOverloadApplierSo : DefenseTowerRewardApplierSo
{
    // 应用超载攻速变化。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
            state.AddAttackSpeedOverloadBonus(attackSpeedBonus);
        }
    }
}
