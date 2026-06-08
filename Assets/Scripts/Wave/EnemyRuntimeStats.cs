using UnityEngine;

/// <summary>
/// 敌人出生时锁定的运行时属性
/// </summary>
public readonly struct EnemyRuntimeStats
{
    public int MaxHealth { get; }
    public int Armor { get; }
    public int AttackDamage { get; }
    public float MoveSpeed { get; }
    public float DetectRadius { get; }

    // 保存敌人本次出生使用的运行时属性。
    public EnemyRuntimeStats(int maxHealth, int armor, int attackDamage, float moveSpeed, float detectRadius)
    {
        MaxHealth = Mathf.Max(1, maxHealth);
        Armor = Mathf.Max(0, armor);
        AttackDamage = Mathf.Max(1, attackDamage);
        MoveSpeed = Mathf.Max(0.01f, moveSpeed);
        DetectRadius = Mathf.Max(0.01f, detectRadius);
    }

    // 从敌人静态配置创建未成长的运行时属性。
    public static EnemyRuntimeStats FromEnemySo(EnemySo enemySo)
    {
        if (!enemySo)
        {
            return new EnemyRuntimeStats(1, 0, 1, 1f, 1f);
        }

        return new EnemyRuntimeStats(
            enemySo.maxHealth,
            enemySo.armor,
            enemySo.atk,
            enemySo.moveSpeed,
            enemySo.detectRadius);
    }
}
