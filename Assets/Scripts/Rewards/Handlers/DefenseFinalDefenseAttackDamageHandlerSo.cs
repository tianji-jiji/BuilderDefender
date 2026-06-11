using UnityEngine;

/// <summary>
/// 基地低血量防御塔攻击加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Final Defense Attack Damage Handler")]
public class DefenseFinalDefenseAttackDamageHandlerSo : DefenseRewardHandlerSo
{
    // 应用最终防线攻击加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float threshold = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.HOME_HEALTH_THRESHOLD, 0f, true);
            state.AddFinalDefense(GetValue(config), threshold);
        }
    }
}
