using UnityEngine;

/// <summary>
/// 闃插尽濉旀敾鍑绘墸琛€瑙勫垯 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Health Cost Handler")]
public class DefenseAttackHealthCostHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏀诲嚮鎵ｈ瑙勫垯銆?
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
