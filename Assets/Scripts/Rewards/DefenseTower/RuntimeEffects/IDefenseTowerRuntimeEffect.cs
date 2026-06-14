/// <summary>
/// 防御塔卡牌运行时效果接口，定义卡牌可介入的战斗生命周期节点。
/// </summary>
public interface IDefenseTowerRuntimeEffect
{
    bool ShouldRegisterRuntimeEffect { get; }

    // 修改防御塔当前战斗属性。
    void ModifyStats(DefenseTowerRuntimeEffectInstance instance, DefenseTowerStatsContext context);

    // 处理防御塔攻击前逻辑。
    void OnBeforeAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context);

    // 处理防御塔攻击后逻辑。
    void OnAfterAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context);

    // 修改单支箭发射前的上下文。
    void ModifyArrow(DefenseTowerRuntimeEffectInstance instance, DefenseTowerArrowContext context);

    // 处理箭矢命中敌人后的逻辑。
    void OnEnemyHit(DefenseTowerRuntimeEffectInstance instance, DefenseTowerEnemyHitContext context);

    // 处理防御塔击杀敌人后的逻辑。
    void OnEnemyKilled(DefenseTowerRuntimeEffectInstance instance, DefenseTowerEnemyKillContext context);

    // 处理波次结束时的逻辑。
    void OnWaveCompleted(DefenseTowerRuntimeEffectInstance instance, DefenseTowerWaveContext context);
}
