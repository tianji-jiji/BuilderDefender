/// <summary>
/// 防御塔奖励 Handler 基类，负责连接奖励配置和防御塔运行时卡牌效果。
/// </summary>
public abstract class DefenseRewardHandlerSo : RewardEffectHandlerSo, IDefenseCardEffect
{
    public virtual bool ShouldRegisterRuntimeEffect => false;

    // 应用奖励配置，默认不写入任何状态。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
    }

    // 读取当前效果的主数值。
    protected float GetValue(RewardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 尝试取得防御塔奖励状态。
    protected bool TryGetDefenseRewardState(RewardEffectContext context, out DefenseRewardState defenseRewardState)
    {
        defenseRewardState = context?.DefenseRewardState;
        return defenseRewardState != null;
    }

    // 修改防御塔当前战斗属性。
    public virtual void ModifyStats(DefenseCardEffectInstance instance, DefenseStatsContext context)
    {
    }

    // 处理防御塔攻击前逻辑。
    public virtual void OnBeforeAttack(DefenseCardEffectInstance instance, DefenseAttackContext context)
    {
    }

    // 处理防御塔攻击后逻辑。
    public virtual void OnAfterAttack(DefenseCardEffectInstance instance, DefenseAttackContext context)
    {
    }

    // 修改单支箭发射前的上下文。
    public virtual void ModifyArrow(DefenseCardEffectInstance instance, DefenseArrowContext context)
    {
    }

    // 处理箭矢命中敌人后的逻辑。
    public virtual void OnEnemyHit(DefenseCardEffectInstance instance, DefenseEnemyHitContext context)
    {
    }

    // 处理防御塔击杀敌人后的逻辑。
    public virtual void OnEnemyKilled(DefenseCardEffectInstance instance, DefenseEnemyKillContext context)
    {
    }

    // 处理波次结束时的逻辑。
    public virtual void OnWaveCompleted(DefenseCardEffectInstance instance, DefenseWaveContext context)
    {
    }
}
