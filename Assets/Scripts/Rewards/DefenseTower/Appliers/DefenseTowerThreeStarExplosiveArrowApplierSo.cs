using UnityEngine;

/// <summary>
/// 三星爆裂箭奖励应用器，负责让三星防御塔箭矢获得爆炸伤害能力。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Three Star Explosive Arrow Handler")]
public class DefenseTowerThreeStarExplosiveArrowApplierSo : DefenseTowerRewardApplierSo
{
    // 应用三星爆裂箭配置。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state))
        {
            return;
        }

        float explosionRadius = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.EXPLOSION_RADIUS, 0f, true);
        float explosionDamageMultiplier = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.EXPLOSION_DAMAGE_MULTIPLIER, 0f, true);
        state.AddThreeStarExplosiveArrow(explosionRadius, explosionDamageMultiplier);
    }
}
