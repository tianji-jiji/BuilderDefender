using System.Collections.Generic;

/// <summary>
/// 防御塔卡牌运行时效果集合，负责按获得顺序分发战斗生命周期钩子。
/// </summary>
public class DefenseCardEffectRuntime
{
    private readonly List<DefenseCardEffectInstance> _effectInstanceList = new();

    public int Count => _effectInstanceList.Count;

    // 注册一条可运行时生效的防御塔卡牌效果。
    public void RegisterEffect(IDefenseCardEffect effect, RewardEffectConfig config)
    {
        if (effect == null || !effect.ShouldRegisterRuntimeEffect)
        {
            return;
        }

        _effectInstanceList.Add(new DefenseCardEffectInstance(effect, config));
    }

    // 依次修改防御塔当前战斗属性。
    public void ModifyStats(DefenseStatsContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.ModifyStats(instance, context);
        }
    }

    // 依次处理攻击前逻辑。
    public void OnBeforeAttack(DefenseAttackContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.OnBeforeAttack(instance, context);
        }
    }

    // 依次处理攻击后逻辑。
    public void OnAfterAttack(DefenseAttackContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.OnAfterAttack(instance, context);
        }
    }

    // 依次修改单支箭发射上下文。
    public void ModifyArrow(DefenseArrowContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.ModifyArrow(instance, context);
        }
    }

    // 依次处理敌人命中逻辑。
    public void OnEnemyHit(DefenseEnemyHitContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.OnEnemyHit(instance, context);
        }
    }

    // 依次处理敌人击杀逻辑。
    public void OnEnemyKilled(DefenseEnemyKillContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.OnEnemyKilled(instance, context);
        }
    }

    // 依次处理波次结束逻辑。
    public void OnWaveCompleted(DefenseWaveContext context)
    {
        if (context == null)
        {
            return;
        }

        foreach (DefenseCardEffectInstance instance in _effectInstanceList)
        {
            instance.Effect.OnWaveCompleted(instance, context);
        }
    }
}
