using UnityEngine;

/// <summary>
/// 防御塔战斗属性上下文，当某个战斗事件发生时，把相关信息打包成一个对象传过去
/// </summary>
public class DefenseTowerStatsContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public int AttackDamage { get; set; }
    public float ArrowGenerateRate { get; set; }
    public float DetectRadius { get; set; }

    public DefenseTowerStatsContext(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, int attackDamage, float arrowGenerateRate, float detectRadius)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        AttackDamage = Mathf.Max(1, attackDamage);
        ArrowGenerateRate = Mathf.Max(0.01f, arrowGenerateRate);
        DetectRadius = Mathf.Max(0.01f, detectRadius);
    }
}

/// <summary>
/// 防御塔攻击上下文，允许运行时卡牌处理攻击前后逻辑并请求额外攻击。
/// </summary>
public class DefenseTowerAttackContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public HealthSystem SourceHealthSystem { get; }
    public int ExtraAttackCount { get; private set; }

    public DefenseTowerAttackContext(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, HealthSystem sourceHealthSystem)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        SourceHealthSystem = sourceHealthSystem;
    }

    // 请求额外发射指定数量的普通箭。
    public void RequestExtraAttack(int extraAttackCount)
    {
        ExtraAttackCount += Mathf.Max(0, extraAttackCount);
    }
}

/// <summary>
/// 防御塔单支箭上下文，允许运行时卡牌修改本次发射参数。
/// </summary>
public class DefenseTowerArrowContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy TargetEnemy { get; }
    public int Damage { get; set; }
    public float ArmorIgnorePercent { get; set; }
    public bool IsExplosiveArrow { get; set; }
    public float ExplosionRadius { get; set; }
    public float ExplosionDamageMultiplier { get; set; }
    public Material VisualMaterial { get; set; }
    public bool EnableTrail { get; set; }

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
}

/// <summary>
/// 防御塔命中敌人上下文，描述一次箭矢命中的结算结果。
/// </summary>
public class DefenseTowerEnemyHitContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy HitEnemy { get; }
    public int ActualDamage { get; }

    public DefenseTowerEnemyHitContext(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, Enemy hitEnemy, int actualDamage)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        HitEnemy = hitEnemy;
        ActualDamage = Mathf.Max(0, actualDamage);
    }
}

/// <summary>
/// 防御塔击杀敌人上下文，描述一次由防御塔造成的击杀。
/// </summary>
public class DefenseTowerEnemyKillContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public Enemy KilledEnemy { get; }

    public DefenseTowerEnemyKillContext(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, Enemy killedEnemy)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        KilledEnemy = killedEnemy;
    }
}

/// <summary>
/// 防御塔波次上下文，描述当前波次结算信息。
/// </summary>
public class DefenseTowerWaveContext
{
    public int WaveIndex { get; }

    public DefenseTowerWaveContext(int waveIndex)
    {
        WaveIndex = waveIndex;
    }
}
