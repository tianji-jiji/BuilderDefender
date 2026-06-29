using UnityEngine;

/// <summary>
/// 防御塔攻击完成上下文，保存本次主动攻击来源并收集额外攻击请求。
/// </summary>
public class TowerAttackCompletedContext
{
    public TowerCombatSystem SourceTowerCombatSystem { get; }
    public HealthSystem SourceHealthSystem { get; }
    public int ExtraAttackCount { get; private set; }

    public TowerAttackCompletedContext(
        TowerCombatSystem sourceTowerCombatSystem,
        HealthSystem sourceHealthSystem)
    {
        SourceTowerCombatSystem = sourceTowerCombatSystem;
        SourceHealthSystem = sourceHealthSystem;
    }

    // 请求额外发射指定数量的普通箭。
    public void RequestExtraAttack(int extraAttackCount)
    {
        ExtraAttackCount += Mathf.Max(0, extraAttackCount);
    }
}
