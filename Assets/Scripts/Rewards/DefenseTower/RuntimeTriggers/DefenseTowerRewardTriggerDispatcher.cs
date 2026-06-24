using System.Collections.Generic;

/// <summary>
/// 防御塔卡牌效果分发器，持有那些需要在运行时特定时机被执行的防御塔卡牌效果
/// </summary>
public class DefenseTowerRewardTriggerDispatcher
{
    private readonly List<DefenseTowerRewardTriggerInstance> _effectInstanceList = new();

    // 注册一条可运行时生效的防御塔卡牌效果。
    public void RegisterEffect(IDefenseTowerRewardTrigger effect, RewardCardEffectConfig config)
    {
        if (effect == null || !effect.ShouldRegisterRuntimeEffect)
        {
            return;
        }

        _effectInstanceList.Add(new DefenseTowerRewardTriggerInstance(effect, config));
    }

    // 依次修改防御塔当前战斗属性。
    public void ModifyStats(DefenseTowerStatsContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.ModifyStats(instance, context);
        }
    }

    // 依次处理攻击前逻辑。
    public void OnBeforeAttack(DefenseTowerAttackContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.OnBeforeAttack(instance, context);
        }
    }

    // 依次处理攻击后逻辑。
    public void OnAfterAttack(DefenseTowerAttackContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.OnAfterAttack(instance, context);
        }
    }

    // 依次修改单支箭发射上下文。
    public void ModifyArrow(DefenseTowerArrowContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.ModifyArrow(instance, context);
        }
    }

    // 依次处理敌人命中逻辑。
    public void OnEnemyHit(DefenseTowerEnemyHitContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.OnEnemyHit(instance, context);
        }
    }

    // 依次处理敌人击杀逻辑。
    public void OnEnemyKilled(DefenseTowerEnemyKillContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.OnEnemyKilled(instance, context);
        }
    }

    // 依次处理波次结束逻辑。
    public void OnWaveCompleted(DefenseTowerWaveContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseTowerRewardTriggerInstance instance in _effectInstanceList)
        {
            instance.Effect.OnWaveCompleted(instance, context);
        }
    }
}
