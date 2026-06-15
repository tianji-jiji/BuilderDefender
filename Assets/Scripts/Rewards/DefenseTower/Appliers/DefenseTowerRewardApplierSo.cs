/// <summary>
/// 防御塔奖励应用器基类，负责连接奖励配置、防御塔奖励状态和运行时效果。
/// </summary>
public abstract class DefenseTowerRewardApplierSo : RewardEffectApplierSo, IDefenseTowerRewardTrigger
{
    public virtual bool ShouldRegisterRuntimeEffect => false;

    // 应用奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
    }

    // 读取当前效果的主数值。
    protected float GetValue(RewardCardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 尝试取得防御塔奖励状态。
    protected bool TryGetDefenseTowerRewardState(RewardEffectApplyContext applyContext, out DefenseTowerActiveRewards defenseTowerActiveRewards)
    {
        defenseTowerActiveRewards = applyContext?.DefenseTowerActiveRewards;
        return defenseTowerActiveRewards != null;
    }

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
