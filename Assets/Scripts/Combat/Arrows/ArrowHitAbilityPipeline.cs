using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢命中能力管线，负责去重、直接伤害、组合效果和续飞结果汇总。
/// </summary>
public class ArrowHitAbilityPipeline : MonoBehaviour
{
    private readonly List<Enemy> _hitEnemyList = new();
    private readonly List<ArrowHitEffect> _hitEffectList = new();
    private readonly List<ArrowHitContinuation> _hitContinuationList = new();

    private DefenseTowerCombatSystem _sourceDefenseTowerCombatSystem;
    private int _damage;
    private float _armorIgnorePercent;
    private bool _isConfigured;

    private void Awake()
    {
        CacheAbilities();
    }

    // 使用本次发射快照配置直接伤害和全部组合能力。
    public void Configure(ArrowLaunchData launchData)
    {
        ResetState();
        _sourceDefenseTowerCombatSystem = launchData.SourceDefenseTowerCombatSystem;
        _damage = launchData.Damage;
        _armorIgnorePercent = launchData.ArmorIgnorePercent;

        foreach (ArrowHitEffect hitEffect in _hitEffectList)
        {
            hitEffect.Configure(launchData);
        }

        foreach (ArrowHitContinuation hitContinuation in _hitContinuationList)
        {
            hitContinuation.Configure(launchData);
        }

        _isConfigured = true;
    }

    // 尝试结算一次有效且未重复的敌人命中。
    public bool TryResolveHit(
        Enemy enemy,
        Vector3 hitPosition,
        out ArrowHitContext hitContext,
        out ArrowHitResolution hitResolution)
    {
        hitContext = default;
        hitResolution = default;
        if (!_isConfigured
            || !ArrowHitDamageApplier.IsEnemyValid(enemy)
            || _hitEnemyList.Contains(enemy))
        {
            return false;
        }

        hitContext = new ArrowHitContext(
            enemy,
            hitPosition,
            _damage,
            _armorIgnorePercent,
            _sourceDefenseTowerCombatSystem);
        ApplyHitEffects(hitContext);
        _hitEnemyList.Add(enemy);
        hitResolution = new ArrowHitResolution(ResolveContinuation(hitContext));
        return true;
    }

    // 清理本次发射的命中记录和能力状态。
    public void ResetState()
    {
        _sourceDefenseTowerCombatSystem = null;
        _damage = 0;
        _armorIgnorePercent = 0f;
        _isConfigured = false;
        _hitEnemyList.Clear();

        foreach (ArrowHitEffect hitEffect in _hitEffectList)
        {
            hitEffect.ResetState();
        }

        foreach (ArrowHitContinuation hitContinuation in _hitContinuationList)
        {
            hitContinuation.ResetState();
        }
    }

    // 缓存并排序同一箭矢对象上的命中能力组件。
    private void CacheAbilities()
    {
        _hitEffectList.Clear();
        _hitEffectList.AddRange(GetComponents<ArrowHitEffect>());
        _hitEffectList.Sort((left, right) => left.ExecutionOrder.CompareTo(right.ExecutionOrder));

        _hitContinuationList.Clear();
        _hitContinuationList.AddRange(GetComponents<ArrowHitContinuation>());
    }

    // 先执行直接伤害，再按稳定顺序执行组合命中效果。
    private void ApplyHitEffects(ArrowHitContext hitContext)
    {
        ArrowHitDamageApplier.ApplyDamage(
            hitContext.DirectHitEnemy,
            hitContext.Damage,
            hitContext.ArmorIgnorePercent,
            hitContext.SourceDefenseTowerCombatSystem);

        foreach (ArrowHitEffect hitEffect in _hitEffectList)
        {
            hitEffect.Apply(hitContext);
        }
    }

    // 汇总全部续飞规则的结果。
    private bool ResolveContinuation(ArrowHitContext hitContext)
    {
        bool shouldContinueFlight = false;
        foreach (ArrowHitContinuation hitContinuation in _hitContinuationList)
        {
            shouldContinueFlight |= hitContinuation.ShouldContinue(hitContext);
        }

        return shouldContinueFlight;
    }
}
