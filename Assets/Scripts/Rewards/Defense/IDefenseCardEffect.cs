/// <summary>
/// 防御塔卡牌运行时效果接口，定义卡牌可介入的战斗生命周期节点。
/// </summary>
public interface IDefenseCardEffect
{
    bool ShouldRegisterRuntimeEffect { get; }

    // 修改防御塔当前战斗属性。
    void ModifyStats(DefenseCardEffectInstance instance, DefenseStatsContext context);

    // 处理防御塔攻击前逻辑。
    void OnBeforeAttack(DefenseCardEffectInstance instance, DefenseAttackContext context);

    // 处理防御塔攻击后逻辑。
    void OnAfterAttack(DefenseCardEffectInstance instance, DefenseAttackContext context);

    // 修改单支箭发射前的上下文。
    void ModifyArrow(DefenseCardEffectInstance instance, DefenseArrowContext context);

    // 处理箭矢命中敌人后的逻辑。
    void OnEnemyHit(DefenseCardEffectInstance instance, DefenseEnemyHitContext context);

    // 处理防御塔击杀敌人后的逻辑。
    void OnEnemyKilled(DefenseCardEffectInstance instance, DefenseEnemyKillContext context);

    // 处理波次结束时的逻辑。
    void OnWaveCompleted(DefenseCardEffectInstance instance, DefenseWaveContext context);
}
