using UnityEngine;

/// <summary>
/// 防御塔战斗属性计算器，集中处理升星、奖励和攻击特效数值。
/// </summary>
public class DefenseStatCalculator
{
    private const float MIN_ARROW_GENERATE_RATE = 0.05f;
    private const int DEFAULT_STAR_LEVEL = 1;
    private const int STAR_LEVEL_THREE = 3;

    private readonly DefenseSystem _sourceDefenseSystem;
    private readonly int _baseAttackDamage;
    private readonly float _baseArrowGenerateRate;
    private readonly float _baseDetectRadius;

    private float _upgradeAttackDamageMultiplier = 1f;
    private float _upgradeAttackIntervalMultiplier = 1f;
    private float _upgradeDetectRadiusMultiplier = 1f;

    public int CurrentStarLevel { get; private set; } = DEFAULT_STAR_LEVEL;
    public bool SuperArrowUnlocked { get; private set; }

    // 创建防御塔战斗属性计算器。
    public DefenseStatCalculator(DefenseSystem sourceDefenseSystem, int baseAttackDamage, float baseArrowGenerateRate, float baseDetectRadius)
    {
        _sourceDefenseSystem = sourceDefenseSystem;
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
    public DefenseCombatStats RefreshStats(DefenseCardEffectRuntime activeRuntime)
    {
        DefenseStatsContext statsContext = new(
            _sourceDefenseSystem,
            GetCurrentAttackDamage(false),
            GetCurrentArrowGenerateRate(),
            GetCurrentDetectRadius());
        activeRuntime?.ModifyStats(statsContext);

        return new DefenseCombatStats(
            statsContext.AttackDamage,
            Mathf.Max(MIN_ARROW_GENERATE_RATE, statsContext.ArrowGenerateRate),
            statsContext.DetectRadius);
    }

    // 计算当前攻击伤害。
    public int GetCurrentAttackDamage(bool includeRandomDoubleDamage = true)
    {
        float rewardAttackDamageMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseAttackDamageMultiplier
            : 1f;
        float damage = _baseAttackDamage
                       * _upgradeAttackDamageMultiplier
                       * rewardAttackDamageMultiplier
                       * GetThreeStarResonanceMultiplier()
                       * GetFinalDefenseAttackDamageMultiplier();

        if (includeRandomDoubleDamage && ShouldApplyDoubleDamage())
        {
            damage *= 2f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    // 计算当前攻击间隔。
    private float GetCurrentArrowGenerateRate()
    {
        float rewardAttackIntervalMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseAttackIntervalMultiplier
            : 1f;
        float overloadAttackIntervalMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseOverloadAttackIntervalMultiplier
            : 1f;
        float linkedAttackIntervalMultiplier = ShouldApplyLinkedAttackSpeed() && RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseLinkedAttackIntervalMultiplier
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
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseArmorIgnorePercent : 0f;
    }

    // 判断本次箭矢是否启用三星爆裂箭。
    public bool ShouldUseExplosiveArrow()
    {
        return CurrentStarLevel >= STAR_LEVEL_THREE
               && RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseThreeStarExplosiveArrowEnabled;
    }

    // 获取爆裂箭爆炸半径。
    public float GetExplosionRadius()
    {
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseExplosionRadius : 0f;
    }

    // 获取爆裂箭范围伤害倍率。
    public float GetExplosionDamageMultiplier()
    {
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseExplosionDamageMultiplier : 0f;
    }

    // 计算当前索敌半径。
    private float GetCurrentDetectRadius()
    {
        float rewardDetectRadiusMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseDetectRadiusMultiplier
            : 1f;

        return Mathf.Max(0.01f, _baseDetectRadius * _upgradeDetectRadiusMultiplier * rewardDetectRadiusMultiplier);
    }

    // 计算星级共鸣提供的攻击力倍率。
    private float GetThreeStarResonanceMultiplier()
    {
        if (!RewardBonusManager.Instance)
        {
            return 1f;
        }

        int threeStarCount = DefenseTowerRegistry.GetThreeStarDefenseCount();
        return 1f + threeStarCount * RewardBonusManager.Instance.DefenseAttackDamagePerThreeStarTower;
    }

    // 计算最终防线提供的攻击力倍率。
    private float GetFinalDefenseAttackDamageMultiplier()
    {
        return RewardBonusManager.Instance
            ? RewardBonusManager.Instance.GetFinalDefenseAttackDamageMultiplier()
            : 1f;
    }

    // 判断防线联动攻速是否生效。
    private bool ShouldApplyLinkedAttackSpeed()
    {
        return _sourceDefenseSystem
               && RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseLinkRadius > 0f
               && DefenseTowerRegistry.HasNearbyDefenseTower(_sourceDefenseSystem, RewardBonusManager.Instance.DefenseLinkRadius);
    }

    // 判断本次攻击是否触发双倍伤害。
    private bool ShouldApplyDoubleDamage()
    {
        return RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseDoubleDamageChance > 0f
               && Random.value < RewardBonusManager.Instance.DefenseDoubleDamageChance;
    }
}
