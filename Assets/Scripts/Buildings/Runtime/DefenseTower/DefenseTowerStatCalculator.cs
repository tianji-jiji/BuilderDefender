using UnityEngine;

/// <summary>
/// 防御塔战斗属性计算器，集中处理升星、奖励和攻击特效数值。
/// </summary>
public class DefenseTowerStatCalculator
{
    private const float MIN_ARROW_GENERATE_RATE = 0.05f;
    private const int DEFAULT_STAR_LEVEL = 1;
    private const int STAR_LEVEL_THREE = 3;

    private readonly DefenseTowerCombatSystem _sourceDefenseTowerCombatSystem;
    private readonly int _baseAttackDamage;
    private readonly float _baseArrowGenerateRate;
    private readonly float _baseDetectRadius;

    private float _upgradeAttackDamageMultiplier = 1f;
    private float _upgradeAttackIntervalMultiplier = 1f;
    private float _upgradeDetectRadiusMultiplier = 1f;

    public int CurrentStarLevel { get; private set; } = DEFAULT_STAR_LEVEL;
    public bool SuperArrowUnlocked { get; private set; }
    private DefenseTowerActiveRewards ActiveRewards => RewardRuntimeCoordinator.Instance
        ? RewardRuntimeCoordinator.Instance.DefenseTowerRewards.ActiveRewards
        : null;

    public DefenseTowerStatCalculator(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, int baseAttackDamage, float baseArrowGenerateRate, float baseDetectRadius)
    {
        _sourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
        _baseAttackDamage = Mathf.Max(1, baseAttackDamage);
        _baseArrowGenerateRate = Mathf.Max(MIN_ARROW_GENERATE_RATE, baseArrowGenerateRate);
        _baseDetectRadius = Mathf.Max(0.01f, baseDetectRadius);
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

    // 重新计算当前防御塔属性并允许卡牌运行时修改结果。
    public DefenseTowerCombatStats RefreshStats(DefenseTowerRewardTriggerDispatcher activeDispatcher)
    {
        DefenseTowerStatsContext statsContext = new(
            _sourceDefenseTowerCombatSystem,
            GetCurrentAttackDamage(false),
            GetCurrentArrowGenerateRate(),
            GetCurrentDetectRadius());
        activeDispatcher?.ModifyStats(statsContext);

        return new DefenseTowerCombatStats(
            statsContext.AttackDamage,
            Mathf.Max(MIN_ARROW_GENERATE_RATE, statsContext.ArrowGenerateRate),
            statsContext.DetectRadius);
    }

    // 计算当前攻击伤害。
    public int GetCurrentAttackDamage(bool includeRandomDoubleDamage = true)
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        float rewardAttackDamageMultiplier = activeRewards != null ? activeRewards.AttackDamageMultiplier : 1f;
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
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        float rewardAttackIntervalMultiplier = activeRewards != null ? activeRewards.AttackIntervalMultiplier : 1f;
        float overloadAttackIntervalMultiplier = activeRewards != null ? activeRewards.OverloadAttackIntervalMultiplier : 1f;
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
        return ActiveRewards != null ? ActiveRewards.ArmorIgnorePercent : 0f;
    }

    // 判断本次箭矢是否启用三星爆裂箭。
    public bool ShouldUseExplosiveArrow()
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        return CurrentStarLevel >= STAR_LEVEL_THREE
               && activeRewards != null
               && activeRewards.ThreeStarExplosiveArrowEnabled;
    }

    // 获取爆裂箭爆炸半径。
    public float GetExplosionRadius()
    {
        return ActiveRewards != null ? ActiveRewards.ExplosionRadius : 0f;
    }

    // 获取爆裂箭范围伤害倍率。
    public float GetExplosionDamageMultiplier()
    {
        return ActiveRewards != null ? ActiveRewards.ExplosionDamageMultiplier : 0f;
    }

    // 计算当前索敌半径。
    private float GetCurrentDetectRadius()
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        float rewardDetectRadiusMultiplier = activeRewards != null ? activeRewards.DetectRadiusMultiplier : 1f;

        return Mathf.Max(0.01f, _baseDetectRadius * _upgradeDetectRadiusMultiplier * rewardDetectRadiusMultiplier);
    }

    // 计算星级共鸣提供的攻击力倍率。
    private float GetThreeStarResonanceMultiplier()
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        if (activeRewards == null)
        {
            return 1f;
        }

        int threeStarCount = DefenseTowerRegistry.GetThreeStarDefenseTowerCount();
        return 1f + threeStarCount * activeRewards.AttackDamagePerThreeStarTower;
    }

    // 计算最终防线提供的攻击力倍率。
    private float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        return RewardRuntimeCoordinator.Instance
            ? RewardRuntimeCoordinator.Instance.DefenseTowerRewards.GetFinalDefenseTowerAttackDamageMultiplier()
            : 1f;
    }

    // 判断防线联动攻速是否生效。
    private bool ShouldApplyLinkedAttackSpeed()
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        return _sourceDefenseTowerCombatSystem
               && activeRewards != null
               && activeRewards.LinkRadius > 0f
               && DefenseTowerRegistry.HasNearbyDefenseTower(_sourceDefenseTowerCombatSystem, activeRewards.LinkRadius);
    }

    // 判断本次攻击是否触发双倍伤害。
    private bool ShouldApplyDoubleDamage()
    {
        DefenseTowerActiveRewards activeRewards = ActiveRewards;
        return activeRewards != null
               && activeRewards.DoubleDamageChance > 0f
               && Random.value < activeRewards.DoubleDamageChance;
    }
}
