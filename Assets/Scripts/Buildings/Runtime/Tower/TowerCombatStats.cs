using UnityEngine;

/// <summary>
/// 防御塔当前战斗属性快照。
/// </summary>
public readonly struct TowerCombatStats
{
    public int AttackDamage { get; }
    public float ArrowGenerateRate { get; }
    public float DetectRadius { get; }

    // 创建防御塔当前战斗属性快照。
    public TowerCombatStats(int attackDamage, float arrowGenerateRate, float detectRadius)
    {
        AttackDamage = Mathf.Max(1, attackDamage);
        ArrowGenerateRate = Mathf.Max(0.01f, arrowGenerateRate);
        DetectRadius = Mathf.Max(0.01f, detectRadius);
    }
}
