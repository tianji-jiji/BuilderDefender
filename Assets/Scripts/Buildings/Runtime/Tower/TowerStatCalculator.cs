using UnityEngine;

/// <summary>
/// 防御塔战斗属性计算器，集中处理升星、奖励和攻击特效数值。
/// </summary>
public class TowerStatCalculator
{
    private const float MIN_ARROW_GENERATE_RATE = 0.05f;
    private const int DEFAULT_STAR_LEVEL = 1;
    private const int STAR_LEVEL_THREE = 3;

    private readonly TowerCombatSystem _sourceTowerCombatSystem;
    private readonly int _baseAttackDamage;
    private readonly float _baseArrowGenerateRate;
    private readonly float _baseDetectRadius;
    private readonly TowerRewardRuntime _rewardRuntime;

    private float _upgradeAttackDamageMultiplier = 1f;
    private float _upgradeAttackIntervalMultiplier = 1f;
    private float _upgradeDetectRadiusMultiplier = 1f;

    public int CurrentStarLevel { get; private set; } = DEFAULT_STAR_LEVEL;
    public bool SuperArrowUnlocked { get; private set; }
    private TowerRewardState RewardState => _rewardRuntime?.State;

    // 创建使用指定基础属性和奖励运行时的 Tower 属性计算器。
    public TowerStatCalculator(
        TowerCombatSystem sourceTowerCombatSystem,
        int baseAttackDamage,
        float baseArrowGenerateRate,
        float baseDetectRadius,
        TowerRewardRuntime rewardRuntime)
    {
        _sourceTowerCombatSystem = sourceTowerCombatSystem;
        _baseAttackDamage = Mathf.Max(1, baseAttackDamage);
        _baseArrowGenerateRate = Mathf.Max(MIN_ARROW_GENERATE_RATE, baseArrowGenerateRate);
        _baseDetectRadius = Mathf.Max(0.01f, baseDetectRadius);
        _rewardRuntime = rewardRuntime;
    }

    // 应用当前升星等级提供的战斗属性。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (upgradeLevel == null)
        {
            return;
        }

        _upgradeAttackDamageMultiplier = upgradeLevel.AttackDamageMultiplier;
        _upgradeAttackIntervalMultiplier = upgradeLevel.AttackIntervalMultiplier;
        _upgradeDetectRadiusMultiplier = upgradeLevel.DetectRadiusMultiplier;
        SuperArrowUnlocked = upgradeLevel.UnlockSuperArrow;
        CurrentStarLevel = upgradeLevel.StarLevel;
    }

    // 重新计算当前防御塔属性。
    public TowerCombatStats RefreshStats()
    {
        return new TowerCombatStats(
            GetCurrentAttackDamage(false),
            GetCurrentArrowGenerateRate(),
            GetCurrentDetectRadius());
    }

    // 计算当前攻击伤害。
    public int GetCurrentAttackDamage(bool includeRandomDoubleDamage = true)
    {
        TowerRewardState activeRewards = RewardState;
        float rewardAttackDamageMultiplier = activeRewards?.AttackDamageMultiplier ?? 1f;
        float damage = _baseAttackDamage
                       * _upgradeAttackDamageMultiplier
                       * rewardAttackDamageMultiplier
                       * GetThreeStarResonanceMultiplier()
                       * GetFinalDefenseTowerAttackDamageMultiplier();

        if (includeRandomDoubleDamage && ShouldApplyDoubleDamage())
        {
            damage *= 2f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    // 计算当前攻击间隔。
    private float GetCurrentArrowGenerateRate()
    {
        TowerRewardState activeRewards = RewardState;
        float rewardAttackIntervalMultiplier = activeRewards?.AttackIntervalMultiplier ?? 1f;
        float overloadAttackIntervalMultiplier = activeRewards?.OverloadAttackIntervalMultiplier ?? 1f;
        float linkedAttackIntervalMultiplier = ShouldApplyLinkedAttackSpeed() && activeRewards != null
            ? activeRewards.LinkedAttackIntervalMultiplier
            : 1f;

        return Mathf.Max(
            MIN_ARROW_GENERATE_RATE,
            _baseArrowGenerateRate
            * _upgradeAttackIntervalMultiplier
            * rewardAttackIntervalMultiplier
            * overloadAttackIntervalMultiplier
            * linkedAttackIntervalMultiplier);
    }

    // 获取本次箭矢护甲穿透比例。
    public float GetArmorIgnorePercent()
    {
        return RewardState?.ArmorIgnorePercent ?? 0f;
    }

    // 判断本次箭矢是否启用三星爆裂箭。
    public bool ShouldUseExplosiveArrow()
    {
        TowerRewardState activeRewards = RewardState;
        return CurrentStarLevel >= STAR_LEVEL_THREE
               && activeRewards != null
               && activeRewards.ThreeStarExplosiveArrowEnabled;
    }

    // 获取爆裂箭爆炸半径。
    public float GetExplosionRadius()
    {
        return RewardState?.ExplosionRadius ?? 0f;
    }

    // 获取爆裂箭范围伤害倍率。
    public float GetExplosionDamageMultiplier()
    {
        return RewardState?.ExplosionDamageMultiplier ?? 0f;
    }

    // 计算当前索敌半径。
    private float GetCurrentDetectRadius()
    {
        TowerRewardState activeRewards = RewardState;
        float rewardDetectRadiusMultiplier = activeRewards?.DetectRadiusMultiplier ?? 1f;

        return Mathf.Max(0.01f, _baseDetectRadius * _upgradeDetectRadiusMultiplier * rewardDetectRadiusMultiplier);
    }

    // 计算星级共鸣提供的攻击力倍率。
    private float GetThreeStarResonanceMultiplier()
    {
        TowerRewardState activeRewards = RewardState;
        if (activeRewards == null)
        {
            return 1f;
        }

        int threeStarCount = _rewardRuntime?.World?.GetThreeStarTowerCount() ?? 0;
        return 1f + threeStarCount * activeRewards.AttackDamagePerThreeStarTower;
    }

    // 计算最终防线提供的攻击力倍率。
    private float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        return _rewardRuntime?.GetFinalDefenseTowerAttackDamageMultiplier() ?? 1f;
    }

    // 判断防线联动攻速是否生效。
    private bool ShouldApplyLinkedAttackSpeed()
    {
        TowerRewardState activeRewards = RewardState;
        return _sourceTowerCombatSystem
               && activeRewards is { LinkRadius: > 0f }
               && _rewardRuntime?.World?.HasNearbyTower(
                   _sourceTowerCombatSystem,
                   activeRewards.LinkRadius) == true;
    }

    // 判断本次攻击是否触发双倍伤害。
    private bool ShouldApplyDoubleDamage()
    {
        TowerRewardState activeRewards = RewardState;
        return activeRewards is { DoubleDamageChance: > 0f }
               && Random.value < activeRewards.DoubleDamageChance;
    }
}
