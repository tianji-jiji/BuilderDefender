using System.Collections.Generic;

/// <summary>
/// 防御塔奖励触发分发器，按运行时能力保存并调用对应奖励效果。
/// </summary>
public class TowerRewardTriggerDispatcher
{
    private readonly List<TowerRewardRuntimeState> _runtimeStateList = new();
    private readonly List<TriggerRegistration<ITowerAttackCompletedTrigger>> _attackCompletedTriggerList = new();
    private readonly List<TriggerRegistration<ITowerArrowModifier>> _arrowModifierList = new();
    private readonly List<TriggerRegistration<ITowerEnemyKilledTrigger>> _enemyKilledTriggerList = new();
    private readonly List<TriggerRegistration<ITowerWaveCompletedTrigger>> _waveCompletedTriggerList = new();

    // 按奖励实际支持的运行时能力注册触发器。
    public void RegisterEffect(ITowerRuntimeReward runtimeReward, RewardCardEffectConfig config)
    {
        if (runtimeReward == null)
        {
            return;
        }

        TowerRewardRuntimeState runtimeState = new(config);
        bool hasRegistered = false;

        hasRegistered |= TryRegister(runtimeReward, runtimeState, _attackCompletedTriggerList);
        hasRegistered |= TryRegister(runtimeReward, runtimeState, _arrowModifierList);
        hasRegistered |= TryRegister(runtimeReward, runtimeState, _enemyKilledTriggerList);
        hasRegistered |= TryRegister(runtimeReward, runtimeState, _waveCompletedTriggerList);

        if (hasRegistered)
        {
            _runtimeStateList.Add(runtimeState);
        }
    }

    // 处理一次成功完成的防御塔主动攻击。
    public void OnAttackCompleted(TowerAttackCompletedContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (TriggerRegistration<ITowerAttackCompletedTrigger> registration in _attackCompletedTriggerList)
        {
            registration.Trigger.OnAttackCompleted(registration.RuntimeState, context);
        }
    }

    // 修改单支待发射箭矢的奖励能力配置。
    public void ModifyArrow(TowerArrowContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (TriggerRegistration<ITowerArrowModifier> registration in _arrowModifierList)
        {
            registration.Trigger.ModifyArrow(registration.RuntimeState, context);
        }
    }

    // 处理一次由防御塔造成的敌人击杀。
    public void OnEnemyKilled(TowerEnemyKilledContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (TriggerRegistration<ITowerEnemyKilledTrigger> registration in _enemyKilledTriggerList)
        {
            registration.Trigger.OnEnemyKilled(registration.RuntimeState, context);
        }
    }

    // 处理一次波次完成结算。
    public void OnWaveCompleted()
    {
        foreach (TriggerRegistration<ITowerWaveCompletedTrigger> registration in _waveCompletedTriggerList)
        {
            registration.Trigger.OnWaveCompleted(registration.RuntimeState);
        }
    }

    // 清理指定防御塔在全部运行时奖励中的计数状态。
    public void ClearSource(TowerCombatSystem sourceTowerCombatSystem)
    {
        if (!sourceTowerCombatSystem)
        {
            return;
        }

        foreach (TowerRewardRuntimeState runtimeState in _runtimeStateList)
        {
            runtimeState.ClearCounters(sourceTowerCombatSystem);
        }
    }

    // 将奖励按指定能力加入对应触发列表。
    private static bool TryRegister<TTrigger>(
        ITowerRuntimeReward runtimeReward,
        TowerRewardRuntimeState runtimeState,
        List<TriggerRegistration<TTrigger>> triggerRegistrationList)
        where TTrigger : class, ITowerRuntimeReward
    {
        if (runtimeReward is not TTrigger trigger)
        {
            return false;
        }

        triggerRegistrationList.Add(new TriggerRegistration<TTrigger>(trigger, runtimeState));
        return true;
    }

    /// <summary>
    /// 单项触发能力注册记录，关联具体处理器和共享运行时状态。
    /// </summary>
    private class TriggerRegistration<T> where T : class, ITowerRuntimeReward
    {
        public T Trigger { get; }
        public TowerRewardRuntimeState RuntimeState { get; }

        public TriggerRegistration(T trigger, TowerRewardRuntimeState runtimeState)
        {
            Trigger = trigger;
            RuntimeState = runtimeState;
        }
    }
}
