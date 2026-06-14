using UnityEngine;

/// <summary>
/// 基地低血量防御塔攻击加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Final Defense Attack Damage Handler")]
public class DefenseTowerFinalDefenseAttackDamageHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用最终防线攻击加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            float threshold = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.HOME_HEALTH_THRESHOLD, 0f, true);
            state.AddFinalDefense(GetValue(config), threshold);
        }
    }
}
