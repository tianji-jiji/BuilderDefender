using UnityEngine;

/// <summary>
/// 防御塔箭矢发射组件，负责生成箭矢并写入本次发射上下文。
/// </summary>
public class DefenseArrowLauncher : MonoBehaviour
{
    private const int STAR_LEVEL_TWO = 2;
    private const int STAR_LEVEL_THREE = 3;

    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform[] superArrowSpawnPoints;
    [SerializeField] private Material starOneArrowMaterial;
    [SerializeField] private Material starTwoArrowMaterial;
    [SerializeField] private Material starThreeArrowMaterial;

    // 发射当前星级对应的一组主动攻击箭矢。
    public bool FirePrimaryShotGroup(
        DefenseSystem sourceDefenseSystem,
        DefenseTargetSelector targetSelector,
        DefenseStatCalculator statCalculator,
        DefenseCardEffectRuntime activeRuntime)
    {
        if (statCalculator == null)
        {
            return false;
        }

        bool hasFired = FireFromSpawnPoint(sourceDefenseSystem, targetSelector, statCalculator, activeRuntime, arrowSpawnPoint, DefenseTargetLane.Any);

        if (statCalculator.SuperArrowUnlocked)
        {
            hasFired |= FireSuperArrowSpawnPoint(sourceDefenseSystem, targetSelector, statCalculator, activeRuntime, 0, DefenseTargetLane.Upper);
            hasFired |= FireSuperArrowSpawnPoint(sourceDefenseSystem, targetSelector, statCalculator, activeRuntime, 1, DefenseTargetLane.Lower);
        }

        return hasFired;
    }

    // 发射指定数量的额外普通箭。
    public void FireExtraAttackArrows(
        int extraAttackCount,
        DefenseSystem sourceDefenseSystem,
        DefenseTargetSelector targetSelector,
        DefenseStatCalculator statCalculator,
        DefenseCardEffectRuntime activeRuntime)
    {
        for (int i = 0; i < extraAttackCount; i++)
        {
            FireFromSpawnPoint(sourceDefenseSystem, targetSelector, statCalculator, activeRuntime, arrowSpawnPoint, DefenseTargetLane.Any);
        }
    }

    // 使用三星解锁的额外发射点进行分区射击。
    private bool FireSuperArrowSpawnPoint(
        DefenseSystem sourceDefenseSystem,
        DefenseTargetSelector targetSelector,
        DefenseStatCalculator statCalculator,
        DefenseCardEffectRuntime activeRuntime,
        int index,
        DefenseTargetLane targetLane)
    {
        if (superArrowSpawnPoints == null || index < 0 || index >= superArrowSpawnPoints.Length)
        {
            return false;
        }

        return FireFromSpawnPoint(sourceDefenseSystem, targetSelector, statCalculator, activeRuntime, superArrowSpawnPoints[index], targetLane);
    }

    // 使用指定发射点向目标区域内的最近敌人射箭。
    private bool FireFromSpawnPoint(
        DefenseSystem sourceDefenseSystem,
        DefenseTargetSelector targetSelector,
        DefenseStatCalculator statCalculator,
        DefenseCardEffectRuntime activeRuntime,
        Transform spawnPoint,
        DefenseTargetLane targetLane)
    {
        if (!arrowPrefab || !spawnPoint || !targetSelector || statCalculator == null)
        {
            return false;
        }

        Enemy target = targetSelector.FindPreferredTarget(targetLane);
        if (!DefenseTargetSelector.IsTargetValid(target))
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

        DefenseArrowContext arrowContext = CreateArrowContext(sourceDefenseSystem, target, statCalculator);
        activeRuntime?.ModifyArrow(arrowContext);
        ApplyArrowContext(arrow, arrowContext);
        return true;
    }

    // 创建本次发射的箭矢上下文。
    private DefenseArrowContext CreateArrowContext(DefenseSystem sourceDefenseSystem, Enemy target, DefenseStatCalculator statCalculator)
    {
        return new DefenseArrowContext(
            sourceDefenseSystem,
            target,
            statCalculator.GetCurrentAttackDamage(),
            statCalculator.GetArmorIgnorePercent(),
            statCalculator.ShouldUseExplosiveArrow(),
            statCalculator.GetExplosionRadius(),
            statCalculator.GetExplosionDamageMultiplier(),
            GetArrowMaterialForCurrentStarLevel(statCalculator.CurrentStarLevel),
            ShouldEnableArrowTrail(statCalculator.CurrentStarLevel));
    }

    // 将箭矢上下文写入箭矢实例。
    private void ApplyArrowContext(Arrow arrow, DefenseArrowContext arrowContext)
    {
        arrow.SetVisualEffect(arrowContext.VisualMaterial, arrowContext.EnableTrail);
        arrow.SetDamage(arrowContext.Damage);
        arrow.SetAttackContext(
            arrowContext.SourceDefenseSystem,
            arrowContext.ArmorIgnorePercent,
            arrowContext.IsExplosiveArrow,
            arrowContext.ExplosionRadius,
            arrowContext.ExplosionDamageMultiplier);
        arrow.SetTarget(arrowContext.TargetEnemy);
    }

    // 根据当前星级获取箭头应该使用的视觉材质。
    private Material GetArrowMaterialForCurrentStarLevel(int currentStarLevel)
    {
        if (currentStarLevel >= STAR_LEVEL_THREE && starThreeArrowMaterial)
        {
            return starThreeArrowMaterial;
        }

        if (currentStarLevel >= STAR_LEVEL_TWO && starTwoArrowMaterial)
        {
            return starTwoArrowMaterial;
        }

        return starOneArrowMaterial;
    }

    // 判断当前星级是否启用箭头拖尾效果。
    private bool ShouldEnableArrowTrail(int currentStarLevel)
    {
        return currentStarLevel >= STAR_LEVEL_TWO;
    }
}
