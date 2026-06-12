using UnityEngine;

/// <summary>
/// 防御塔战斗属性上下文，允许运行时卡牌调整当前计算结果。
/// </summary>
public class DefenseStatsContext
{
    public DefenseSystem SourceDefenseSystem { get; }
    public int AttackDamage { get; set; }
    public float ArrowGenerateRate { get; set; }
    public float DetectRadius { get; set; }

    // 创建防御塔战斗属性上下文。
    public DefenseStatsContext(DefenseSystem sourceDefenseSystem, int attackDamage, float arrowGenerateRate, float detectRadius)
    {
        SourceDefenseSystem = sourceDefenseSystem;
        AttackDamage = Mathf.Max(1, attackDamage);
        ArrowGenerateRate = Mathf.Max(0.01f, arrowGenerateRate);
        DetectRadius = Mathf.Max(0.01f, detectRadius);
    }
}

/// <summary>
/// 防御塔攻击上下文，允许运行时卡牌处理攻击前后逻辑并请求额外攻击。
/// </summary>
public class DefenseAttackContext
{
    public DefenseSystem SourceDefenseSystem { get; }
    public HealthSystem SourceHealthSystem { get; }
    public int ExtraAttackCount { get; private set; }

    // 创建防御塔攻击上下文。
    public DefenseAttackContext(DefenseSystem sourceDefenseSystem, HealthSystem sourceHealthSystem)
    {
        SourceDefenseSystem = sourceDefenseSystem;
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
public class DefenseArrowContext
{
    public DefenseSystem SourceDefenseSystem { get; }
    public Enemy TargetEnemy { get; }
    public int Damage { get; set; }
    public float ArmorIgnorePercent { get; set; }
    public bool IsExplosiveArrow { get; set; }
    public float ExplosionRadius { get; set; }
    public float ExplosionDamageMultiplier { get; set; }
    public Material VisualMaterial { get; set; }
    public bool EnableTrail { get; set; }

    // 创建防御塔单支箭上下文。
    public DefenseArrowContext(
        DefenseSystem sourceDefenseSystem,
        Enemy targetEnemy,
        int damage,
        float armorIgnorePercent,
        bool isExplosiveArrow,
        float explosionRadius,
        float explosionDamageMultiplier,
        Material visualMaterial,
        bool enableTrail)
    {
        SourceDefenseSystem = sourceDefenseSystem;
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
public class DefenseEnemyHitContext
{
    public DefenseSystem SourceDefenseSystem { get; }
    public Enemy HitEnemy { get; }
    public int ActualDamage { get; }

    // 创建防御塔命中敌人上下文。
    public DefenseEnemyHitContext(DefenseSystem sourceDefenseSystem, Enemy hitEnemy, int actualDamage)
    {
        SourceDefenseSystem = sourceDefenseSystem;
        HitEnemy = hitEnemy;
        ActualDamage = Mathf.Max(0, actualDamage);
    }
}

/// <summary>
/// 防御塔击杀敌人上下文，描述一次由防御塔造成的击杀。
/// </summary>
public class DefenseEnemyKillContext
{
    public DefenseSystem SourceDefenseSystem { get; }
    public Enemy KilledEnemy { get; }

    // 创建防御塔击杀敌人上下文。
    public DefenseEnemyKillContext(DefenseSystem sourceDefenseSystem, Enemy killedEnemy)
    {
        SourceDefenseSystem = sourceDefenseSystem;
        KilledEnemy = killedEnemy;
    }
}

/// <summary>
/// 防御塔波次上下文，描述当前波次结算信息。
/// </summary>
public class DefenseWaveContext
{
    public int WaveIndex { get; }

    // 创建防御塔波次上下文。
    public DefenseWaveContext(int waveIndex)
    {
        WaveIndex = waveIndex;
    }
}
