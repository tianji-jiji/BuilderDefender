/// <summary>
/// 防御塔运行时奖励应用器基类，负责连接奖励配置、防御塔奖励状态和运行时触发效果。
/// </summary>
public abstract class DefenseTowerRuntimeRewardApplierSo : DefenseTowerRewardApplierSo, IDefenseTowerRewardTrigger
{
    public virtual bool ShouldRegisterRuntimeEffect => true;

    // 修改防御塔当前战斗属性。
    public virtual void ModifyStats(DefenseTowerRewardTriggerInstance instance, DefenseTowerStatsContext context)
    {
    }

    // 处理防御塔攻击前逻辑。
    public virtual void OnBeforeAttack(DefenseTowerRewardTriggerInstance instance, DefenseTowerAttackContext context)
    {
    }

    // 处理防御塔攻击后逻辑。
    public virtual void OnAfterAttack(DefenseTowerRewardTriggerInstance instance, DefenseTowerAttackContext context)
    {
    }

    // 修改单支箭发射前的上下文。
    public virtual void ModifyArrow(DefenseTowerRewardTriggerInstance instance, DefenseTowerArrowContext context)
    {
    }

    // 处理箭矢命中敌人后的逻辑。
    public virtual void OnEnemyHit(DefenseTowerRewardTriggerInstance instance, DefenseTowerEnemyHitContext context)
    {
    }

    // 处理防御塔击杀敌人后的逻辑。
    public virtual void OnEnemyKilled(DefenseTowerRewardTriggerInstance instance, DefenseTowerEnemyKillContext context)
    {
    }

    // 处理波次结束时的逻辑。
    public virtual void OnWaveCompleted(DefenseTowerRewardTriggerInstance instance, DefenseTowerWaveContext context)
    {
    }
}
