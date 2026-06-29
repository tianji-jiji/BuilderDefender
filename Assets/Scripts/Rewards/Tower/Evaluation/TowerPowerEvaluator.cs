using UnityEngine;

/// <summary>
/// 防御塔战力评估器，负责根据当前生效奖励生成防御体系战力快照。
/// </summary>
public static class TowerPowerEvaluator
{
    private const float DEFAULT_POWER_MULTIPLIER = 1f;

    // 根据当前防御塔奖励状态创建战力快照。
    public static TowerPowerSnapshot CreateSnapshot(TowerActiveRewards activeRewards)
    {
        if (activeRewards == null)
        {
            return TowerPowerSnapshot.Default();
        }

        return new TowerPowerSnapshot(
            activeRewards.AttackDamageMultiplier,
            GetAttackSpeedPowerMultiplier(activeRewards),
            activeRewards.ArmorIgnorePercent,
            activeRewards.GetExtraAttackPowerMultiplier(),
            activeRewards.GetCriticalPowerMultiplier(),
            activeRewards.GetExplosivePowerMultiplier());
    }

    // 将攻击间隔倍率转换为战力倍率。
    private static float GetAttackSpeedPowerMultiplier(TowerActiveRewards activeRewards)
    {
        float attackIntervalMultiplier = activeRewards.AttackIntervalMultiplier
                                         * activeRewards.OverloadAttackIntervalMultiplier
                                         * activeRewards.LinkedAttackIntervalMultiplier;
        if (attackIntervalMultiplier <= 0f)
        {
            return DEFAULT_POWER_MULTIPLIER;
        }

        return Mathf.Max(DEFAULT_POWER_MULTIPLIER, DEFAULT_POWER_MULTIPLIER / attackIntervalMultiplier);
    }
}
