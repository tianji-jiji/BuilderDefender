using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 防御塔单支箭上下文，保存基础发射参数并收集运行时奖励附加能力。
/// </summary>
public class DefenseTowerArrowContext
{
    private static readonly IReadOnlyList<EnemyStatusEffectSpec> EmptyStatusEffectSpecList =
        Array.Empty<EnemyStatusEffectSpec>();

    private List<EnemyStatusEffectSpec> _statusEffectSpecList;

    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy TargetEnemy { get; }
    public int Damage { get; }
    public float ArmorIgnorePercent { get; }
    public bool IsExplosiveArrow { get; }
    public float ExplosionRadius { get; }
    public float ExplosionDamageMultiplier { get; }
    public Material VisualMaterial { get; }
    public bool EnableTrail { get; }
    public IReadOnlyList<EnemyStatusEffectSpec> StatusEffectSpecList =>
        _statusEffectSpecList ?? EmptyStatusEffectSpecList;
    public float ChanceExplosionRadius { get; private set; }
    public int ChanceExplosionDamage { get; private set; }
    public int PierceCount { get; private set; }

    public DefenseTowerArrowContext(
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem,
        Enemy targetEnemy,
        int damage,
        float armorIgnorePercent,
        bool isExplosiveArrow,
        float explosionRadius,
        float explosionDamageMultiplier,
        Material visualMaterial,
        bool enableTrail)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        TargetEnemy = targetEnemy;
        Damage = Mathf.Max(1, damage);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        IsExplosiveArrow = isExplosiveArrow;
        ExplosionRadius = Mathf.Max(0f, explosionRadius);
        ExplosionDamageMultiplier = Mathf.Max(0f, explosionDamageMultiplier);
        VisualMaterial = visualMaterial;
        EnableTrail = enableTrail;
    }

    // 添加本支箭命中时要施加的敌人状态。
    public void AddStatusEffect(EnemyStatusEffectSpec statusEffectSpec)
    {
        _statusEffectSpecList ??= new();
        _statusEffectSpecList.Add(statusEffectSpec);
    }

    // 设置本支箭命中时触发的概率爆炸。
    public void SetChanceExplosion(float radius, int damage)
    {
        ChanceExplosionRadius = Mathf.Max(0f, radius);
        ChanceExplosionDamage = Mathf.Max(0, damage);
    }

    // 设置本支箭可额外穿透的敌人数量。
    public void SetPierceCount(int pierceCount)
    {
        PierceCount = Mathf.Max(PierceCount, pierceCount);
    }
}
