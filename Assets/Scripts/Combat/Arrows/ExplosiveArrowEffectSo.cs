using UnityEngine;

/// <summary>
/// 爆裂箭命中特殊效果，负责对命中点范围内的其他敌人造成范围伤害。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/ArrowEffects/Explosive Arrow Effect")]
public class ExplosiveArrowEffectSo : ArrowEffectSo
{
    // 应用爆裂箭范围伤害效果。
    public override void Apply(ArrowHitContext context)
    {
        if (context == null
            || context.ExplosionRadius <= 0f
            || context.ExplosionDamageMultiplier <= 0f
            || context.EffectHitResults == null)
        {
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(context.HitPosition, context.ExplosionRadius, context.EffectHitResults);
        int explosionDamage = Mathf.Max(1, Mathf.RoundToInt(context.Damage * context.ExplosionDamageMultiplier));

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitResult = context.EffectHitResults[i];
            if (!hitResult
                || !hitResult.TryGetComponent(out Enemy enemy)
                || enemy == context.DirectHitEnemy
                || !ArrowDamageApplier.IsEnemyValid(enemy))
            {
                continue;
            }

            ArrowDamageApplier.ApplyDamage(
                enemy,
                explosionDamage,
                context.ArmorIgnorePercent,
                context.SourceDefenseTowerSystem);
        }
    }
}
