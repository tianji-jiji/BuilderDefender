/// <summary>
/// 箭矢命中处理器，负责执行直接命中伤害并触发附加效果。
/// </summary>
public static class ArrowHitProcessor
{
    // 处理箭矢命中后的直接伤害和可选特殊效果。
    public static void ApplyHit(ArrowHitContext context, ArrowEffectSo arrowEffect)
    {
        if (context == null || !ArrowDamageApplier.IsEnemyValid(context.DirectHitEnemy))
        {
            return;
        }

        ArrowDamageApplier.ApplyDamage(
            context.DirectHitEnemy,
            context.Damage,
            context.ArmorIgnorePercent,
            context.SourceDefenseSystem);

        if (arrowEffect)
        {
            arrowEffect.Apply(context);
        }
    }
}
