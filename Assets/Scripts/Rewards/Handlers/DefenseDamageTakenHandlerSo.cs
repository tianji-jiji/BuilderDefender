using UnityEngine;

/// <summary>
/// 防御塔受到伤害变化 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Damage Taken Handler")]
public class DefenseDamageTakenHandlerSo : DefenseRewardHandlerSo
{
    // 应用受到伤害变化。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float damageTakenBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER, GetValue(config));
            state.AddDamageTakenBonus(damageTakenBonus);
        }
    }
}
