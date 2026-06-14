/// <summary>
/// 防御塔奖励 Handler 基类，负责连接奖励配置和防御塔运行时卡牌效果。
/// </summary>
public abstract class DefenseTowerRewardHandlerSo : RewardEffectHandlerSo, IDefenseTowerCardEffect
{
    public virtual bool ShouldRegisterRuntimeEffect => false;

    // 应用奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
    }

    // 读取当前效果的主数值。
    protected float GetValue(RewardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 尝试取得防御塔奖励加成集合。
    protected bool TryGetDefenseTowerRewardModifiers(RewardEffectContext context, out DefenseTowerRewardModifiers defenseTowerRewardModifiers)
    {
        defenseTowerRewardModifiers = context?.DefenseTowerRewardModifiers;
        return defenseTowerRewardModifiers != null;
    }

    // 修改防御塔当前战斗属性。
    public virtual void ModifyStats(DefenseTowerRuntimeEffectInstance instance, DefenseTowerStatsContext context)
    {
    }

    // 处理防御塔攻击前逻辑。
    public virtual void OnBeforeAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context)
    {
    }

    // 处理防御塔攻击后逻辑。
    public virtual void OnAfterAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context)
    {
    }

    // 修改单支箭发射前的上下文。
    public virtual void ModifyArrow(DefenseTowerRuntimeEffectInstance instance, DefenseTowerArrowContext context)
    {
    }

    // 处理箭矢命中敌人后的逻辑。
    public virtual void OnEnemyHit(DefenseTowerRuntimeEffectInstance instance, DefenseTowerEnemyHitContext context)
    {
    }

    // 处理防御塔击杀敌人后的逻辑。
    public virtual void OnEnemyKilled(DefenseTowerRuntimeEffectInstance instance, DefenseTowerEnemyKillContext context)
    {
    }

    // 处理波次结束时的逻辑。
    public virtual void OnWaveCompleted(DefenseTowerRuntimeEffectInstance instance, DefenseTowerWaveContext context)
    {
    }
}
