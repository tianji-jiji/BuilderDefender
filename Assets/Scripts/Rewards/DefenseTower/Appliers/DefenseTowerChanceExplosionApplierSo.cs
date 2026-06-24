using UnityEngine;

/// <summary>
/// 防御塔概率爆炸箭奖励应用器，负责让箭矢概率造成固定范围伤害。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Chance Explosion Applier")]
public class DefenseTowerChanceExplosionApplierSo : DefenseTowerRuntimeRewardApplierSo
{
    // 修改单支箭的概率爆炸能力。
    public override void ModifyArrow(DefenseTowerRewardTriggerInstance instance, DefenseTowerArrowContext context)
    {
        if (context == null || !ShouldTrigger(instance.Config))
        {
            return;
        }

        float explosionRadius = RewardEffectParameterReader.GetFloat(instance.Config, RewardEffectParameterIds.EXPLOSION_RADIUS, 0f, true);
        int explosionDamage = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.EXPLOSION_DAMAGE, 0, true);
        context.SetChanceExplosion(explosionRadius, explosionDamage);
    }

    // 判断本支箭是否触发爆炸。
    private bool ShouldTrigger(RewardCardEffectConfig config)
    {
        float triggerChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TRIGGER_CHANCE, GetValue(config));
        return triggerChance > 0f && Random.value < Mathf.Clamp01(triggerChance);
    }
}
