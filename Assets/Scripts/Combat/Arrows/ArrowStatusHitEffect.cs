using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢状态命中效果，负责向直接目标施加本支箭携带的中毒和燃烧状态。
/// </summary>
public class ArrowStatusHitEffect : ArrowHitEffect
{
    private const int EXECUTION_ORDER = 100;

    private readonly List<EnemyStatusEffectSpec> _statusEffectSpecList = new();

    public override int ExecutionOrder => EXECUTION_ORDER;

    // 复制本次发射携带的全部状态效果。
    public override void Configure(ArrowLaunchData launchData)
    {
        _statusEffectSpecList.Clear();
        if (launchData.StatusEffectSpecList == null)
        {
            return;
        }

        foreach (EnemyStatusEffectSpec statusEffectSpec in launchData.StatusEffectSpecList)
        {
            _statusEffectSpecList.Add(statusEffectSpec);
        }
    }

    // 向仍然有效的直接命中目标施加全部状态。
    public override void Apply(ArrowHitContext context)
    {
        if (_statusEffectSpecList.Count <= 0
            || !ArrowHitDamageApplier.IsEnemyValid(context.DirectHitEnemy)
            || !context.DirectHitEnemy.StatusEffectController)
        {
            return;
        }

        foreach (EnemyStatusEffectSpec statusEffectSpec in _statusEffectSpecList)
        {
            context.DirectHitEnemy.StatusEffectController.ApplyStatus(statusEffectSpec);
        }
    }

    // 清空对象池实例上的状态效果配置。
    public override void ResetState()
    {
        _statusEffectSpecList.Clear();
    }
}
