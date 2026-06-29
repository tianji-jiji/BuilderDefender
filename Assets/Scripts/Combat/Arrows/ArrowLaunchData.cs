using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢单次发射的只读数据快照，负责一次性传递目标、伤害和附加能力配置。
/// </summary>
public readonly struct ArrowLaunchData
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy TargetEnemy { get; }
    public int Damage { get; }
    public float ArmorIgnorePercent { get; }
    public IReadOnlyList<EnemyStatusEffectSpec> StatusEffectSpecList { get; }
    public IReadOnlyList<ArrowAreaDamageSpec> AreaDamageSpecList { get; }
    public int PierceCount { get; }
    public ArrowFlightBehaviorType FlightBehaviorType { get; }

    // 创建单次箭矢发射数据快照。
    public ArrowLaunchData(
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem,
        Enemy targetEnemy,
        int damage,
        float armorIgnorePercent,
        IReadOnlyList<EnemyStatusEffectSpec> statusEffectSpecList,
        IReadOnlyList<ArrowAreaDamageSpec> areaDamageSpecList,
        int pierceCount,
        ArrowFlightBehaviorType flightBehaviorType)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        TargetEnemy = targetEnemy;
        Damage = Mathf.Max(1, damage);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        StatusEffectSpecList = statusEffectSpecList;
        AreaDamageSpecList = areaDamageSpecList;
        PierceCount = Mathf.Max(0, pierceCount);
        FlightBehaviorType = flightBehaviorType;
    }
}
