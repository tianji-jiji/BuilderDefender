using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢范围伤害效果，按配置顺序执行固定伤害爆炸和基础伤害倍率爆炸。
/// </summary>
public class ArrowAreaDamageHitEffect : ArrowHitEffect
{
    private const int EXECUTION_ORDER = 200;
    private const int MAX_EFFECT_HIT_COUNT = 32;

    private readonly Collider2D[] _effectHitResults = new Collider2D[MAX_EFFECT_HIT_COUNT];
    private readonly List<ArrowAreaDamageSpec> _areaDamageSpecList = new();

    public override int ExecutionOrder => EXECUTION_ORDER;

    // 复制本次发射携带的范围伤害规格。
    public override void Configure(ArrowLaunchData launchData)
    {
        _areaDamageSpecList.Clear();
        if (launchData.AreaDamageSpecList == null)
        {
            return;
        }

        foreach (ArrowAreaDamageSpec areaDamageSpec in launchData.AreaDamageSpecList)
        {
            _areaDamageSpecList.Add(areaDamageSpec);
        }
    }

    // 按规格顺序执行本次命中的全部范围伤害。
    public override void Apply(ArrowHitContext context)
    {
        foreach (ArrowAreaDamageSpec areaDamageSpec in _areaDamageSpecList)
        {
            ApplyAreaDamage(context, areaDamageSpec);
        }
    }

    // 清空对象池实例上的范围伤害配置。
    public override void ResetState()
    {
        _areaDamageSpecList.Clear();
    }

    // 执行一项范围伤害规格。
    private void ApplyAreaDamage(ArrowHitContext context, ArrowAreaDamageSpec areaDamageSpec)
    {
        if (areaDamageSpec.Radius <= 0f)
        {
            return;
        }

        int hitCount = Physics2D.OverlapCircle(
            context.HitPosition,
            areaDamageSpec.Radius,
            ContactFilter2D.noFilter,
            _effectHitResults);
        for (int i = 0; i < hitCount; i++)
        {
            ApplyAreaDamageToCollider(context, areaDamageSpec, _effectHitResults[i]);
        }
    }

    // 对范围查询中的单个碰撞体应用伤害。
    private static void ApplyAreaDamageToCollider(
        ArrowHitContext context,
        ArrowAreaDamageSpec areaDamageSpec,
        Collider2D hitResult)
    {
        if (!hitResult
            || !Enemy.TryGetFromCollider(hitResult, out Enemy enemy)
            || !ArrowHitDamageApplier.IsEnemyValid(enemy)
            || (!areaDamageSpec.IncludeDirectHitEnemy && enemy == context.DirectHitEnemy))
        {
            return;
        }

        if (areaDamageSpec.DamageMode == ArrowAreaDamageMode.FixedRaw)
        {
            ArrowHitDamageApplier.ApplyRawDamage(
                enemy,
                areaDamageSpec.FixedDamage,
                areaDamageSpec.FloatingTextStyle,
                context.SourceDefenseTowerCombatSystem);
            return;
        }

        int damage = Mathf.Max(1, Mathf.RoundToInt(context.Damage * areaDamageSpec.DamageMultiplier));
        ArrowHitDamageApplier.ApplyDamage(
            enemy,
            damage,
            context.ArmorIgnorePercent,
            context.SourceDefenseTowerCombatSystem);
    }
}
