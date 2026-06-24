using UnityEngine;

/// <summary>
/// 箭矢能力效果执行器，负责处理命中后附带的状态和固定伤害爆炸。
/// </summary>
public static class ArrowAbilityEffectExecutor
{
    // 应用箭矢命中附带的全部能力效果。
    public static void ApplyAbilities(ArrowHitContext context)
    {
        if (context == null)
        {
            return;
        }

        ApplyStatusEffects(context);
        ApplyChanceExplosion(context);
    }

    // 向直接命中的敌人施加状态效果。
    private static void ApplyStatusEffects(ArrowHitContext context)
    {
        if (context.StatusEffectSpecList == null || context.StatusEffectSpecList.Count <= 0)
        {
            return;
        }

        if (!ArrowHitDamageApplier.IsEnemyValid(context.DirectHitEnemy)
            || !context.DirectHitEnemy.StatusEffectController)
        {
            return;
        }

        foreach (EnemyStatusEffectSpec statusEffectSpec in context.StatusEffectSpecList)
        {
            context.DirectHitEnemy.StatusEffectController.ApplyStatus(statusEffectSpec);
        }
    }

    // 应用概率爆炸固定伤害。
    private static void ApplyChanceExplosion(ArrowHitContext context)
    {
        if (!context.HasChanceExplosion || context.EffectHitResults == null)
        {
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(context.HitPosition, context.ChanceExplosionRadius, context.EffectHitResults);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitResult = context.EffectHitResults[i];
            if (!hitResult
                || !hitResult.TryGetComponent(out Enemy enemy)
                || !ArrowHitDamageApplier.IsEnemyValid(enemy))
            {
                continue;
            }

            ArrowHitDamageApplier.ApplyRawDamage(
                enemy,
                context.ChanceExplosionDamage,
                DamageFloatingTextStyle.Explosion,
                context.SourceDefenseTowerCombatSystem);
        }
    }
}
