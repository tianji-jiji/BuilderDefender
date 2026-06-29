using UnityEngine;

/// <summary>
/// 箭矢单项范围伤害规格，描述半径、伤害算法和直接目标处理规则。
/// </summary>
public readonly struct ArrowAreaDamageSpec
{
    public ArrowAreaDamageMode DamageMode { get; }
    public float Radius { get; }
    public int FixedDamage { get; }
    public float DamageMultiplier { get; }
    public bool IncludeDirectHitEnemy { get; }
    public DamageFloatingTextStyle FloatingTextStyle { get; }

    // 创建范围伤害规格。
    private ArrowAreaDamageSpec(
        ArrowAreaDamageMode damageMode,
        float radius,
        int fixedDamage,
        float damageMultiplier,
        bool includeDirectHitEnemy,
        DamageFloatingTextStyle floatingTextStyle)
    {
        DamageMode = damageMode;
        Radius = Mathf.Max(0f, radius);
        FixedDamage = Mathf.Max(0, fixedDamage);
        DamageMultiplier = Mathf.Max(0f, damageMultiplier);
        IncludeDirectHitEnemy = includeDirectHitEnemy;
        FloatingTextStyle = floatingTextStyle;
    }

    // 创建包含直接目标的固定无视护甲范围伤害。
    public static ArrowAreaDamageSpec CreateFixedRaw(float radius, int damage)
    {
        return new ArrowAreaDamageSpec(
            ArrowAreaDamageMode.FixedRaw,
            radius,
            damage,
            0f,
            true,
            DamageFloatingTextStyle.Explosion);
    }

    // 创建排除直接目标的基础伤害倍率范围伤害。
    public static ArrowAreaDamageSpec CreateBaseDamageMultiplier(float radius, float damageMultiplier)
    {
        return new ArrowAreaDamageSpec(
            ArrowAreaDamageMode.BaseDamageMultiplier,
            radius,
            0,
            damageMultiplier,
            false,
            DamageFloatingTextStyle.Normal);
    }
}
