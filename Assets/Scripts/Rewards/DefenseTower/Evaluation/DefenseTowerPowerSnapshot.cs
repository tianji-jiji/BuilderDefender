using UnityEngine;

/// <summary>
/// 防御体系当前战力快照，用于让敌人成长软响应玩家局内成长。
/// </summary>
public readonly struct DefenseTowerPowerSnapshot
{
    private const float DEFAULT_POWER_MULTIPLIER = 1f;

    public float AttackDamageMultiplier { get; }
    public float AttackSpeedMultiplier { get; }
    public float ArmorIgnorePercent { get; }
    public float ExtraAttackMultiplier { get; }
    public float CriticalMultiplier { get; }
    public float ExplosiveMultiplier { get; }

    public float PowerMultiplier { get; }

    // 保存玩家当前主要输出维度并计算综合战力倍率。
    public DefenseTowerPowerSnapshot(
        float attackDamageMultiplier,
        float attackSpeedMultiplier,
        float armorIgnorePercent,
        float extraAttackMultiplier,
        float criticalMultiplier,
        float explosiveMultiplier)
    {
        AttackDamageMultiplier = Mathf.Max(DEFAULT_POWER_MULTIPLIER, attackDamageMultiplier);
        AttackSpeedMultiplier = Mathf.Max(DEFAULT_POWER_MULTIPLIER, attackSpeedMultiplier);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        ExtraAttackMultiplier = Mathf.Max(DEFAULT_POWER_MULTIPLIER, extraAttackMultiplier);
        CriticalMultiplier = Mathf.Max(DEFAULT_POWER_MULTIPLIER, criticalMultiplier);
        ExplosiveMultiplier = Mathf.Max(DEFAULT_POWER_MULTIPLIER, explosiveMultiplier);

        float armorIgnorePower = DEFAULT_POWER_MULTIPLIER + ArmorIgnorePercent * 0.5f;
        PowerMultiplier = Mathf.Max(
            DEFAULT_POWER_MULTIPLIER,
            AttackDamageMultiplier
            * AttackSpeedMultiplier
            * armorIgnorePower
            * ExtraAttackMultiplier
            * CriticalMultiplier
            * ExplosiveMultiplier);
    }

    // 返回没有任何奖励加成时的默认战力快照。
    public static DefenseTowerPowerSnapshot Default()
    {
        return new DefenseTowerPowerSnapshot(
            DEFAULT_POWER_MULTIPLIER,
            DEFAULT_POWER_MULTIPLIER,
            0f,
            DEFAULT_POWER_MULTIPLIER,
            DEFAULT_POWER_MULTIPLIER,
            DEFAULT_POWER_MULTIPLIER);
    }
}
