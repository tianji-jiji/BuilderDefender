using UnityEngine;

/// <summary>
/// 防御塔箭矢发射组件，负责生成箭矢并写入本次发射上下文。
/// </summary>
public class DefenseTowerArrowLauncher : MonoBehaviour
{
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform[] superArrowSpawnPoints;

    // 发射一组主动攻击箭矢。
    public bool FirePrimaryShotGroup(
        DefenseTowerSystem sourceDefenseTowerSystem,
        DefenseTowerTargetSelector targetSelector,
        DefenseTowerStatCalculator statCalculator,
        DefenseTowerCardEffectDispatcher activeDispatcher)
    {
        if (statCalculator == null)
        {
            return false;
        }

        bool hasFired = FireFromSpawnPoint(sourceDefenseTowerSystem, targetSelector, statCalculator, activeDispatcher, arrowSpawnPoint, DefenseTowerTargetLane.Any);

        if (statCalculator.SuperArrowUnlocked)
        {
            hasFired |= FireSuperArrowSpawnPoint(sourceDefenseTowerSystem, targetSelector, statCalculator, activeDispatcher, 0, DefenseTowerTargetLane.Upper);
            hasFired |= FireSuperArrowSpawnPoint(sourceDefenseTowerSystem, targetSelector, statCalculator, activeDispatcher, 1, DefenseTowerTargetLane.Lower);
        }

        return hasFired;
    }

    // 发射指定数量的额外普通箭。
    public void FireExtraAttackArrows(
        int extraAttackCount,
        DefenseTowerSystem sourceDefenseTowerSystem,
        DefenseTowerTargetSelector targetSelector,
        DefenseTowerStatCalculator statCalculator,
        DefenseTowerCardEffectDispatcher activeDispatcher)
    {
        for (int i = 0; i < extraAttackCount; i++)
        {
            FireFromSpawnPoint(sourceDefenseTowerSystem, targetSelector, statCalculator, activeDispatcher, arrowSpawnPoint, DefenseTowerTargetLane.Any);
        }
    }

    // 使用三星解锁的额外发射点进行分区射击。
    private bool FireSuperArrowSpawnPoint(
        DefenseTowerSystem sourceDefenseTowerSystem,
        DefenseTowerTargetSelector targetSelector,
        DefenseTowerStatCalculator statCalculator,
        DefenseTowerCardEffectDispatcher activeDispatcher,
        int index,
        DefenseTowerTargetLane targetLane)
    {
        if (superArrowSpawnPoints == null || index < 0 || index >= superArrowSpawnPoints.Length)
        {
            return false;
        }

        return FireFromSpawnPoint(sourceDefenseTowerSystem, targetSelector, statCalculator, activeDispatcher, superArrowSpawnPoints[index], targetLane);
    }

    // 使用指定发射点向目标区域内的最近敌人射箭。
    private bool FireFromSpawnPoint(
        DefenseTowerSystem sourceDefenseTowerSystem,
        DefenseTowerTargetSelector targetSelector,
        DefenseTowerStatCalculator statCalculator,
        DefenseTowerCardEffectDispatcher activeDispatcher,
        Transform spawnPoint,
        DefenseTowerTargetLane targetLane)
    {
        if (!arrowPrefab || !spawnPoint || !targetSelector || statCalculator == null)
        {
            return false;
        }

        Enemy target = targetSelector.FindPreferredTarget(targetLane);
        if (!DefenseTowerTargetSelector.IsTargetValid(target))
        {
            return false;
        }

        Arrow arrow = PoolManager.Instance
            ? PoolManager.Instance.Spawn(arrowPrefab, spawnPoint.position, Quaternion.identity)
            : Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity);

        if (!arrow)
        {
            return false;
        }

        DefenseTowerArrowContext arrowContext = CreateArrowContext(sourceDefenseTowerSystem, target, statCalculator);
        activeDispatcher?.ModifyArrow(arrowContext);
        ApplyArrowContext(arrow, arrowContext);
        return true;
    }

    // 创建本次发射的箭矢上下文。
    private DefenseTowerArrowContext CreateArrowContext(DefenseTowerSystem sourceDefenseTowerSystem, Enemy target, DefenseTowerStatCalculator statCalculator)
    {
        return new DefenseTowerArrowContext(
            sourceDefenseTowerSystem,
            target,
            statCalculator.GetCurrentAttackDamage(),
            statCalculator.GetArmorIgnorePercent(),
            statCalculator.ShouldUseExplosiveArrow(),
            statCalculator.GetExplosionRadius(),
            statCalculator.GetExplosionDamageMultiplier(),
            null,
            false);
    }

    // 将箭矢上下文写入箭矢实例。
    private void ApplyArrowContext(Arrow arrow, DefenseTowerArrowContext arrowContext)
    {
        arrow.SetVisualEffect(arrowContext.VisualMaterial, arrowContext.EnableTrail);
        arrow.SetDamage(arrowContext.Damage);
        arrow.SetAttackContext(
            arrowContext.SourceDefenseTowerSystem,
            arrowContext.ArmorIgnorePercent,
            arrowContext.IsExplosiveArrow,
            arrowContext.ExplosionRadius,
            arrowContext.ExplosionDamageMultiplier);
        arrow.SetTarget(arrowContext.TargetEnemy);
    }

}
