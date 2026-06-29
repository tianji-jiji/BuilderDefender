using UnityEngine;

/// <summary>
/// 防御塔箭矢发射组件，负责生成箭矢并写入本次发射上下文。
/// </summary>
public class TowerArrowLauncher : MonoBehaviour
{
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform[] superArrowSpawnPoints;

    // 发射一组主动攻击箭矢。
    public bool FirePrimaryShotGroup(
        TowerCombatSystem sourceTowerCombatSystem,
        TowerTargetSelector targetSelector,
        TowerStatCalculator statCalculator,
        TowerRewardTriggerDispatcher activeDispatcher)
    {
        if (statCalculator == null)
        {
            return false;
        }

        bool hasFired = FireFromSpawnPoint(sourceTowerCombatSystem, targetSelector, statCalculator, activeDispatcher, arrowSpawnPoint, TowerTargetLane.Any);

        if (statCalculator.SuperArrowUnlocked)
        {
            hasFired |= FireSuperArrowSpawnPoint(sourceTowerCombatSystem, targetSelector, statCalculator, activeDispatcher, 0, TowerTargetLane.Upper);
            hasFired |= FireSuperArrowSpawnPoint(sourceTowerCombatSystem, targetSelector, statCalculator, activeDispatcher, 1, TowerTargetLane.Lower);
        }

        return hasFired;
    }

    // 发射指定数量的额外普通箭。
    public void FireExtraAttackArrows(
        int extraAttackCount,
        TowerCombatSystem sourceTowerCombatSystem,
        TowerTargetSelector targetSelector,
        TowerStatCalculator statCalculator,
        TowerRewardTriggerDispatcher activeDispatcher)
    {
        for (int i = 0; i < extraAttackCount; i++)
        {
            FireFromSpawnPoint(sourceTowerCombatSystem, targetSelector, statCalculator, activeDispatcher, arrowSpawnPoint, TowerTargetLane.Any);
        }
    }

    // 使用三星解锁的额外发射点进行分区射击。
    private bool FireSuperArrowSpawnPoint(
        TowerCombatSystem sourceTowerCombatSystem,
        TowerTargetSelector targetSelector,
        TowerStatCalculator statCalculator,
        TowerRewardTriggerDispatcher activeDispatcher,
        int index,
        TowerTargetLane targetLane)
    {
        if (superArrowSpawnPoints == null || index < 0 || index >= superArrowSpawnPoints.Length)
        {
            return false;
        }

        return FireFromSpawnPoint(sourceTowerCombatSystem, targetSelector, statCalculator, activeDispatcher, superArrowSpawnPoints[index], targetLane);
    }

    // 使用指定发射点向目标区域内的最近敌人射箭。
    private bool FireFromSpawnPoint(
        TowerCombatSystem sourceTowerCombatSystem,
        TowerTargetSelector targetSelector,
        TowerStatCalculator statCalculator,
        TowerRewardTriggerDispatcher activeDispatcher,
        Transform spawnPoint,
        TowerTargetLane targetLane)
    {
        if (!arrowPrefab || !spawnPoint || !targetSelector || statCalculator == null)
        {
            return false;
        }

        Enemy target = targetSelector.FindPreferredTarget(targetLane);
        if (!TowerTargetSelector.IsTargetValid(target))
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

        TowerArrowContext arrowContext = CreateArrowContext(sourceTowerCombatSystem, target, statCalculator);
        activeDispatcher?.ModifyArrow(arrowContext);
        ApplyArrowContext(arrow, arrowContext);
        return true;
    }

    // 创建本次发射的箭矢上下文。
    private TowerArrowContext CreateArrowContext(TowerCombatSystem sourceTowerCombatSystem, Enemy target, TowerStatCalculator statCalculator)
    {
        return new TowerArrowContext(
            sourceTowerCombatSystem,
            target,
            statCalculator.GetCurrentAttackDamage(),
            statCalculator.GetArmorIgnorePercent(),
            statCalculator.ShouldUseExplosiveArrow(),
            statCalculator.GetExplosionRadius(),
            statCalculator.GetExplosionDamageMultiplier());
    }

    // 将箭矢上下文写入箭矢实例。
    private void ApplyArrowContext(Arrow arrow, TowerArrowContext arrowContext)
    {
        arrow.Launch(arrowContext.BuildLaunchData());
    }

}
