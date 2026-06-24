using UnityEngine;

/// <summary>
/// 敌人持续状态配置，描述一次中毒或燃烧的持续时间和跳伤规则。
/// </summary>
public readonly struct EnemyStatusEffectSpec
{
    public EnemyStatusEffectType EffectType { get; }
    public float Duration { get; }
    public float TickInterval { get; }
    public float TickDamagePercent { get; }
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public DamageFloatingTextStyle FloatingTextStyle { get; }

    public EnemyStatusEffectSpec(
        EnemyStatusEffectType effectType,
        float duration,
        float tickInterval,
        float tickDamagePercent,
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem,
        DamageFloatingTextStyle floatingTextStyle)
    {
        EffectType = effectType;
        Duration = Mathf.Max(0f, duration);
        TickInterval = Mathf.Max(0.01f, tickInterval);
        TickDamagePercent = Mathf.Max(0f, tickDamagePercent);
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        FloatingTextStyle = floatingTextStyle;
    }
}
