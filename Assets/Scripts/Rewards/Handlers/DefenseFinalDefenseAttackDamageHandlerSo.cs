using UnityEngine;

/// <summary>
/// 鍩哄湴浣庤閲忛槻寰″鏀诲嚮鍔犳垚 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Final Defense Attack Damage Handler")]
public class DefenseFinalDefenseAttackDamageHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏈€缁堥槻绾挎敾鍑诲姞鎴愩€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float threshold = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.HOME_HEALTH_THRESHOLD, 0f, true);
            state.AddFinalDefense(GetValue(config), threshold);
        }
    }
}
