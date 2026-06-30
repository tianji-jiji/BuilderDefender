using UnityEngine;

/// <summary>
/// Tower 战力计算器，负责把奖励状态和外部输入转换为战力倍率。
/// </summary>
public static class TowerPowerCalculator
{
    private const float DEFAULT_POWER_MULTIPLIER = 1f;
    private const float EXPLOSIVE_POWER_FACTOR = 0.1f;
    private const float MAX_EXPLOSIVE_POWER_BONUS = 1.5f;

    // 根据当前 Tower 奖励状态创建战力快照。
    public static TowerPowerSnapshot CreateSnapshot(TowerRewardState rewardState)
    {
        if (rewardState == null)
        {
            return TowerPowerSnapshot.Default();
        }

        return new TowerPowerSnapshot(
            rewardState.AttackDamageMultiplier,
            GetAttackSpeedPowerMultiplier(rewardState),
            rewardState.ArmorIgnorePercent,
            GetExtraAttackPowerMultiplier(rewardState),
            GetCriticalPowerMultiplier(rewardState),
            GetExplosivePowerMultiplier(rewardState));
    }

    // 计算额外攻击规则提供的平均输出倍率。
    public static float GetExtraAttackPowerMultiplier(TowerRewardState rewardState)
    {
        if (rewardState == null)
        {
            return DEFAULT_POWER_MULTIPLIER;
        }

        float extraAttackPower = 0f;
        foreach (TowerExtraAttackRule rule in rewardState.ExtraAttackRuleList)
        {
            extraAttackPower += (float)rule.extraAttackCount / rule.triggerAttackCount;
        }

        return DEFAULT_POWER_MULTIPLIER + Mathf.Max(0f, extraAttackPower);
    }

    // 计算双倍伤害概率提供的平均输出倍率。
    public static float GetCriticalPowerMultiplier(TowerRewardState rewardState)
    {
        return DEFAULT_POWER_MULTIPLIER + (rewardState?.DoubleDamageChance ?? 0f);
    }

    // 计算爆裂箭提供的粗略范围输出倍率。
    public static float GetExplosivePowerMultiplier(TowerRewardState rewardState)
    {
        if (rewardState == null
            || !rewardState.ThreeStarExplosiveArrowEnabled
            || rewardState.ExplosionRadius <= 0f
            || rewardState.ExplosionDamageMultiplier <= 0f)
        {
            return DEFAULT_POWER_MULTIPLIER;
        }

        float explosivePowerBonus = rewardState.ExplosionRadius
                                    * rewardState.ExplosionDamageMultiplier
                                    * EXPLOSIVE_POWER_FACTOR;
        return DEFAULT_POWER_MULTIPLIER
               + Mathf.Clamp(explosivePowerBonus, 0f, MAX_EXPLOSIVE_POWER_BONUS);
    }

    // 根据基地生命比例计算最终防线倍率。
    public static float GetFinalDefenseMultiplier(
        TowerRewardState rewardState,
        float homeHealthNormalized)
    {
        if (rewardState == null
            || rewardState.FinalDefenseAttackDamageBonus <= 0f
            || rewardState.FinalDefenseHomeHealthThreshold <= 0f
            || homeHealthNormalized > rewardState.FinalDefenseHomeHealthThreshold)
        {
            return DEFAULT_POWER_MULTIPLIER;
        }

        return DEFAULT_POWER_MULTIPLIER + rewardState.FinalDefenseAttackDamageBonus;
    }

    // 将攻击间隔倍率转换为战力倍率。
    private static float GetAttackSpeedPowerMultiplier(TowerRewardState rewardState)
    {
        float attackIntervalMultiplier = rewardState.AttackIntervalMultiplier
                                         * rewardState.OverloadAttackIntervalMultiplier
                                         * rewardState.LinkedAttackIntervalMultiplier;
        if (attackIntervalMultiplier <= 0f)
        {
            return DEFAULT_POWER_MULTIPLIER;
        }

        return Mathf.Max(
            DEFAULT_POWER_MULTIPLIER,
            DEFAULT_POWER_MULTIPLIER / attackIntervalMultiplier);
    }
}
