using UnityEngine;

/// <summary>
/// 涓夋槦闃插尽濉旂垎瑁傜 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Three Star Explosive Arrow Handler")]
public class DefenseThreeStarExplosiveArrowHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤涓夋槦鐖嗚绠厤缃€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            return;
        }

        float explosionRadius = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.EXPLOSION_RADIUS, 0f, true);
        float explosionDamageMultiplier = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.EXPLOSION_DAMAGE_MULTIPLIER, 0f, true);
        state.AddThreeStarExplosiveArrow(explosionRadius, explosionDamageMultiplier);
    }
}
