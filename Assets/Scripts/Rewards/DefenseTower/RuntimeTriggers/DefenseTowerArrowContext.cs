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
    private static readonly IReadOnlyList<ArrowAreaDamageSpec> EmptyAreaDamageSpecList =
        Array.Empty<ArrowAreaDamageSpec>();

    private List<EnemyStatusEffectSpec> _statusEffectSpecList;
    private readonly bool _isExplosiveArrow;
    private readonly float _explosionRadius;
    private readonly float _explosionDamageMultiplier;
    private float _chanceExplosionRadius;
    private int _chanceExplosionDamage;
    private ArrowFlightBehaviorType _flightBehaviorType = ArrowFlightBehaviorType.Homing;

    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy TargetEnemy { get; }
    public int Damage { get; }
    public float ArmorIgnorePercent { get; }
    public int PierceCount { get; private set; }

    public DefenseTowerArrowContext(
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem,
        Enemy targetEnemy,
        int damage,
        float armorIgnorePercent,
        bool isExplosiveArrow,
        float explosionRadius,
        float explosionDamageMultiplier)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        TargetEnemy = targetEnemy;
        Damage = Mathf.Max(1, damage);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        _isExplosiveArrow = isExplosiveArrow;
        _explosionRadius = Mathf.Max(0f, explosionRadius);
        _explosionDamageMultiplier = Mathf.Max(0f, explosionDamageMultiplier);
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
        _chanceExplosionRadius = Mathf.Max(0f, radius);
        _chanceExplosionDamage = Mathf.Max(0, damage);
    }

    // 设置本支箭可额外穿透的敌人数量。
    public void SetPierceCount(int pierceCount)
    {
        PierceCount = Mathf.Max(PierceCount, pierceCount);
    }

    // 设置本支箭使用的飞行行为类型。
    public void SetFlightBehavior(ArrowFlightBehaviorType flightBehaviorType)
    {
        _flightBehaviorType = flightBehaviorType;
    }

    // 生成可一次性写入箭矢的发射数据快照。
    public ArrowLaunchData BuildLaunchData()
    {
        return new ArrowLaunchData(
            SourceDefenseTowerCombatSystem,
            TargetEnemy,
            Damage,
            ArmorIgnorePercent,
            _statusEffectSpecList ?? EmptyStatusEffectSpecList,
            BuildAreaDamageSpecList(),
            PierceCount,
            _flightBehaviorType);
    }

    // 按既有结算顺序构建本支箭的范围伤害规格。
    private IReadOnlyList<ArrowAreaDamageSpec> BuildAreaDamageSpecList()
    {
        List<ArrowAreaDamageSpec> areaDamageSpecList = null;
        if (_chanceExplosionRadius > 0f && _chanceExplosionDamage > 0)
        {
            areaDamageSpecList = new List<ArrowAreaDamageSpec>
            {
                ArrowAreaDamageSpec.CreateFixedRaw(_chanceExplosionRadius, _chanceExplosionDamage)
            };
        }

        if (_isExplosiveArrow && _explosionRadius > 0f && _explosionDamageMultiplier > 0f)
        {
            areaDamageSpecList ??= new();
            areaDamageSpecList.Add(ArrowAreaDamageSpec.CreateBaseDamageMultiplier(
                _explosionRadius,
                _explosionDamageMultiplier));
        }

        return areaDamageSpecList ?? EmptyAreaDamageSpecList;
    }
}
