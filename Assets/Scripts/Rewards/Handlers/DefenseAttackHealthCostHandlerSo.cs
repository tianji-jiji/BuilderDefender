using UnityEngine;

/// <summary>
/// 防御塔攻击扣血规则 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Health Cost Handler")]
public class DefenseAttackHealthCostHandlerSo : DefenseRewardHandlerSo
{
    // 应用攻击扣血规则。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 1);
        int healthCost = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.ATTACK_HEALTH_COST, Mathf.RoundToInt(GetValue(config)));
        state.AddAttackHealthCostRule(triggerAttackCount, healthCost);
    }
}
