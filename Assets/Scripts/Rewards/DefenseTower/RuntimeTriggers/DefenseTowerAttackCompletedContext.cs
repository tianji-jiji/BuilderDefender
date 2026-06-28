using UnityEngine;

/// <summary>
/// 防御塔攻击完成上下文，保存本次主动攻击来源并收集额外攻击请求。
/// </summary>
public class DefenseTowerAttackCompletedContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }
    public HealthSystem SourceHealthSystem { get; }
    public int ExtraAttackCount { get; private set; }

    public DefenseTowerAttackCompletedContext(
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem,
        HealthSystem sourceHealthSystem)
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
