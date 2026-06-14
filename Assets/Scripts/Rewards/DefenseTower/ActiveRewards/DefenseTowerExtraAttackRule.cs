using UnityEngine;

/// <summary>
/// 防御塔额外攻击规则，保存触发攻击次数和额外攻击数量。
/// </summary>
public readonly struct DefenseTowerExtraAttackRule
{
    public readonly int triggerAttackCount;
    public readonly int extraAttackCount;

    // 创建额外攻击规则。
    public DefenseTowerExtraAttackRule(int triggerAttackCount, int extraAttackCount)
    {
        this.triggerAttackCount = Mathf.Max(1, triggerAttackCount);
        this.extraAttackCount = Mathf.Max(1, extraAttackCount);
    }
}
