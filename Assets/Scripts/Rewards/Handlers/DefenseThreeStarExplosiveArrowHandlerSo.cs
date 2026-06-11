using UnityEngine;

/// <summary>
/// 三星防御塔爆裂箭 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Three Star Explosive Arrow Handler")]
public class DefenseThreeStarExplosiveArrowHandlerSo : DefenseRewardHandlerSo
{
    // 应用三星爆裂箭配置。
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
