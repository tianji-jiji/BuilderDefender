using UnityEngine;

/// <summary>
/// 防御塔攻击损失生命规则，保存触发攻击次数和每次触发损失的生命值。
/// </summary>
public readonly struct TowerAttackHealthCostRule
{
    public readonly int triggerAttackCount;
    public readonly int healthCost;

    // 创建攻击损失生命规则。
    public TowerAttackHealthCostRule(int triggerAttackCount, int healthCost)
    {
        this.triggerAttackCount = Mathf.Max(1, triggerAttackCount);
        this.healthCost = Mathf.Max(1, healthCost);
    }
}
