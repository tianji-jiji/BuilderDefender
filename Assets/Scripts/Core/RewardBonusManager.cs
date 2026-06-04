using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 防御塔额外攻击规则，保存触发攻击次数和额外攻击数量。
/// </summary>
public readonly struct DefenseExtraAttackRule
{
    public readonly int TriggerAttackCount;
    public readonly int ExtraAttackCount;

    // 创建额外攻击规则。
    public DefenseExtraAttackRule(int triggerAttackCount, int extraAttackCount)
    {
        TriggerAttackCount = Mathf.Max(1, triggerAttackCount);
        ExtraAttackCount = Mathf.Max(1, extraAttackCount);
    }
}

/// <summary>
/// 全局奖励加成管理器，负责保存玩家本局已获得的奖励数值。
/// </summary>
public class RewardBonusManager : MonoBehaviour
{
    private const float MIN_ATTRIBUTE_MULTIPLIER = 0.01f;
    private const float MIN_COST_MULTIPLIER = 0f;
    private const float DEFAULT_MULTIPLIER = 1f;

    public static RewardBonusManager Instance;

    // 奖励数值发生变化时通知所有需要刷新属性的对象。
    public static event Action OnRewardBonusChanged;

    private readonly List<DefenseExtraAttackRule> _defenseExtraAttackRuleList = new List<DefenseExtraAttackRule>();

    private EnemyWaveManager _waveManager;
    private float _defenseAttackDamageBonus;
    private float _defenseAttackSpeedBonus;
    private float _defenseDetectRadiusBonus;
    private float _defenseMaxHealthBonus;
    private float _defenseBuildCostBonus;
    private float _defenseArmorIgnorePercent;
    private float _defenseDamageTakenBonus;
    private float _defenseWaveEndHealPercent;
    private int _defenseKillCountAutoUpgrade;
    private float _defenseAttackSpeedOverloadBonus;
    private int _defenseAttackHealthCost;
    private float _defenseAttackDamagePerThreeStarTower;
    private float _defenseDoubleDamageChance;
    private float _defenseLinkedAttackSpeedBonus;
    private float _defenseLinkRadius;
    private int _defenseNewTowerInitialStarBonus;
    private float _defenseFinalDefenseAttackDamageBonus;
    private float _defenseFinalDefenseHomeHealthThreshold;
    private bool _defenseThreeStarExplosiveArrowEnabled;
    private float _defenseExplosionRadius;
    private float _defenseExplosionDamageMultiplier;

    public float DefenseAttackDamageMultiplier => GetAttributeMultiplier(_defenseAttackDamageBonus);
    public float DefenseAttackIntervalMultiplier => GetAttackIntervalMultiplier(_defenseAttackSpeedBonus);
    public float DefenseDetectRadiusMultiplier => GetAttributeMultiplier(_defenseDetectRadiusBonus);
    public float DefenseMaxHealthMultiplier => GetAttributeMultiplier(_defenseMaxHealthBonus);
    public float DefenseBuildCostMultiplier => GetCostMultiplier();
    public float DefenseArmorIgnorePercent => Mathf.Clamp01(_defenseArmorIgnorePercent);
    public IReadOnlyList<DefenseExtraAttackRule> DefenseExtraAttackRuleList => _defenseExtraAttackRuleList;
    public float DefenseDamageTakenMultiplier => GetAttributeMultiplier(_defenseDamageTakenBonus);
    public float DefenseWaveEndHealPercent => Mathf.Max(0f, _defenseWaveEndHealPercent);
    public int DefenseKillCountAutoUpgrade => Mathf.Max(0, _defenseKillCountAutoUpgrade);
    public float DefenseOverloadAttackIntervalMultiplier => GetAttackIntervalMultiplier(_defenseAttackSpeedOverloadBonus);
    public int DefenseAttackHealthCost => Mathf.Max(0, _defenseAttackHealthCost);
    public float DefenseAttackDamagePerThreeStarTower => Mathf.Max(0f, _defenseAttackDamagePerThreeStarTower);
    public float DefenseDoubleDamageChance => Mathf.Clamp01(_defenseDoubleDamageChance);
    public float DefenseLinkedAttackIntervalMultiplier => GetAttackIntervalMultiplier(_defenseLinkedAttackSpeedBonus);
    public float DefenseLinkRadius => Mathf.Max(0f, _defenseLinkRadius);
    public int DefenseNewTowerInitialStarBonus => Mathf.Max(0, _defenseNewTowerInitialStarBonus);
    public bool DefenseThreeStarExplosiveArrowEnabled => _defenseThreeStarExplosiveArrowEnabled;
    public float DefenseExplosionRadius => Mathf.Max(0f, _defenseExplosionRadius);
    public float DefenseExplosionDamageMultiplier => Mathf.Max(0f, _defenseExplosionDamageMultiplier);

    // 初始化全局奖励管理器单例。
    private void Awake()
    {
        Instance = this;
    }

    // 绑定波次结束事件。
    private void Start()
    {
        BindWaveManager();
    }

    // 解绑波次结束事件。
    private void OnDisable()
    {
        UnbindWaveManager();
    }

    // 应用一张奖励卡中配置的全部效果。
    public void ApplyReward(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return;
        }

        ApplyEffects(rewardCard.EffectConfigList);
        OnRewardBonusChanged?.Invoke();
    }

    // 计算建筑消耗经过全局奖励后的实际数量。
    public static int GetAdjustedBuildCostAmount(BuildingSo buildingSo, ResourceCost resourceCost)
    {
        if (resourceCost == null)
        {
            return 0;
        }

        float costMultiplier = DEFAULT_MULTIPLIER;
        if (buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Defense && Instance)
        {
            costMultiplier = Instance.DefenseBuildCostMultiplier;
        }

        return Mathf.Max(0, Mathf.RoundToInt(resourceCost.amount * costMultiplier));
    }

    // 判断最终防线奖励当前是否处于激活状态。
    public bool IsFinalDefenseActive()
    {
        if (_defenseFinalDefenseAttackDamageBonus <= 0f || _defenseFinalDefenseHomeHealthThreshold <= 0f)
        {
            return false;
        }

        return DefenseTowerRegistry.HomeHealthNormalized <= _defenseFinalDefenseHomeHealthThreshold;
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseAttackDamageMultiplier()
    {
        return IsFinalDefenseActive() ? GetAttributeMultiplier(_defenseFinalDefenseAttackDamageBonus) : DEFAULT_MULTIPLIER;
    }

    // 绑定当前场景中的波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !EnemyWaveManager.Instance)
        {
            return;
        }

        _waveManager = EnemyWaveManager.Instance;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
    }

    // 解绑当前场景中的波次管理器。
    private void UnbindWaveManager()
    {
        if (!_waveManager)
        {
            return;
        }

        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
        _waveManager = null;
    }

    // 波次结束时应用波次结算型奖励。
    private void HandleWaveCompleted(int waveIndex)
    {
        DefenseRewardWaveEndApplier.ApplyWaveEndHeal(DefenseWaveEndHealPercent);
    }

    // 批量应用奖励效果配置。
    private void ApplyEffects(IReadOnlyList<RewardEffectConfig> effectConfigList)
    {
        if (effectConfigList == null)
        {
            return;
        }

        foreach (RewardEffectConfig effectConfig in effectConfigList)
        {
            ApplyEffect(effectConfig);
        }
    }

    // 根据效果类型累加对应的全局奖励值。
    private void ApplyEffect(RewardEffectConfig effectConfig)
    {
        if (effectConfig == null)
        {
            return;
        }

        switch (effectConfig.EffectType)
        {
            case RewardEffectType.DefenseAttackDamageMultiplier:
                _defenseAttackDamageBonus += GetValue(effectConfig);
                break;

            case RewardEffectType.DefenseAttackSpeedMultiplier:
                _defenseAttackSpeedBonus += GetValue(effectConfig);
                break;

            case RewardEffectType.DefenseDetectRadiusMultiplier:
                _defenseDetectRadiusBonus += GetValue(effectConfig);
                break;

            case RewardEffectType.DefenseMaxHealthMultiplier:
                _defenseMaxHealthBonus += GetValue(effectConfig);
                break;

            case RewardEffectType.DefenseBuildCostMultiplier:
                _defenseBuildCostBonus += GetValue(effectConfig);
                break;

            case RewardEffectType.DefenseArmorIgnorePercent:
                _defenseArmorIgnorePercent += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ArmorIgnorePercent, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseExtraArrowEveryAttackCount:
                AddExtraAttackRule(effectConfig);
                break;

            case RewardEffectType.DefenseDamageTakenMultiplier:
                _defenseDamageTakenBonus += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DamageTakenMultiplier, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseWaveEndHealPercent:
                _defenseWaveEndHealPercent += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.WaveEndHealPercent, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseKillCountAutoUpgrade:
                ApplyKillCountAutoUpgrade(effectConfig);
                break;

            case RewardEffectType.DefenseAttackSpeedOverloadMultiplier:
                _defenseAttackSpeedOverloadBonus += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.AttackSpeedMultiplier, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseAttackHealthCost:
                _defenseAttackHealthCost += RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.AttackHealthCost, Mathf.RoundToInt(GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseAttackDamagePerThreeStarTower:
                _defenseAttackDamagePerThreeStarTower += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DamageBonusPerThreeStarTower, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseDoubleDamageChance:
                _defenseDoubleDamageChance += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DoubleDamageChance, GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseLinkedAttackSpeedMultiplier:
                ApplyLinkedAttackSpeed(effectConfig);
                break;

            case RewardEffectType.DefenseRandomTowerMaxStar:
                DefenseRewardImmediateEffectApplier.UpgradeRandomTowerToMaxStar();
                break;

            case RewardEffectType.DefenseNewTowerInitialStarBonus:
                _defenseNewTowerInitialStarBonus += RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.InitialStarBonus, Mathf.RoundToInt(GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseFinalDefenseAttackDamageMultiplier:
                ApplyFinalDefense(effectConfig);
                break;

            case RewardEffectType.DefenseThreeStarExplosiveArrow:
                ApplyThreeStarExplosiveArrow(effectConfig);
                break;
        }
    }

    // 添加一条额外攻击规则。
    private void AddExtraAttackRule(RewardEffectConfig effectConfig)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.TriggerAttackCount, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.ExtraAttackCount, 1);

        if (triggerAttackCount <= 0 || extraAttackCount <= 0)
        {
            return;
        }

        _defenseExtraAttackRuleList.Add(new DefenseExtraAttackRule(triggerAttackCount, extraAttackCount));
    }

    // 应用击杀自动升星阈值。
    private void ApplyKillCountAutoUpgrade(RewardEffectConfig effectConfig)
    {
        int killCountToUpgrade = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.KillCountToUpgrade, 0, true);
        if (killCountToUpgrade <= 0)
        {
            return;
        }

        _defenseKillCountAutoUpgrade = _defenseKillCountAutoUpgrade <= 0
            ? killCountToUpgrade
            : Mathf.Min(_defenseKillCountAutoUpgrade, killCountToUpgrade);
    }

    // 应用防线联动攻速奖励。
    private void ApplyLinkedAttackSpeed(RewardEffectConfig effectConfig)
    {
        _defenseLinkedAttackSpeedBonus += RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.AttackSpeedMultiplier, GetValue(effectConfig));
        float linkRadius = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.LinkRadius, 0f, true);
        _defenseLinkRadius = Mathf.Max(_defenseLinkRadius, linkRadius);
    }

    // 应用最终防线奖励配置。
    private void ApplyFinalDefense(RewardEffectConfig effectConfig)
    {
        _defenseFinalDefenseAttackDamageBonus += GetValue(effectConfig);
        float threshold = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.HomeHealthThreshold, 0f, true);
        _defenseFinalDefenseHomeHealthThreshold = _defenseFinalDefenseHomeHealthThreshold <= 0f
            ? threshold
            : Mathf.Min(_defenseFinalDefenseHomeHealthThreshold, threshold);
    }

    // 应用三星爆裂箭奖励配置。
    private void ApplyThreeStarExplosiveArrow(RewardEffectConfig effectConfig)
    {
        _defenseThreeStarExplosiveArrowEnabled = true;
        _defenseExplosionRadius = Mathf.Max(_defenseExplosionRadius, RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ExplosionRadius, 0f, true));
        _defenseExplosionDamageMultiplier = Mathf.Max(_defenseExplosionDamageMultiplier, RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ExplosionDamageMultiplier, 0f, true));
    }

    // 读取旧版主数值参数。
    private float GetValue(RewardEffectConfig effectConfig)
    {
        return RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.Value, effectConfig.LegacyValue);
    }

    // 根据奖励加成计算属性倍率。
    private float GetAttributeMultiplier(float bonus)
    {
        return Mathf.Max(MIN_ATTRIBUTE_MULTIPLIER, DEFAULT_MULTIPLIER + bonus);
    }

    // 根据攻速加成计算攻击间隔倍率。
    private float GetAttackIntervalMultiplier(float attackSpeedBonus)
    {
        float attackSpeedMultiplier = Mathf.Max(MIN_ATTRIBUTE_MULTIPLIER, DEFAULT_MULTIPLIER + attackSpeedBonus);
        return DEFAULT_MULTIPLIER / attackSpeedMultiplier;
    }

    // 根据建造成本加成计算成本倍率。
    private float GetCostMultiplier()
    {
        return Mathf.Max(MIN_COST_MULTIPLIER, DEFAULT_MULTIPLIER + _defenseBuildCostBonus);
    }
}

/// <summary>
/// 防御塔运行时注册表，负责登记场上防御塔并提供奖励系统查询。
/// </summary>
public static class DefenseTowerRegistry
{
    private static readonly List<DefenseSystem> DefenseSystemList = new List<DefenseSystem>();
    private static readonly List<Building> DefenseBuildingList = new List<Building>();
    private static readonly List<BuildingUpgradeButton> UpgradeButtonList = new List<BuildingUpgradeButton>();
    private static HealthSystem HomeHealthSystem;

    public static float HomeHealthNormalized => HomeHealthSystem ? HomeHealthSystem.CurrentHealthNormalized : 1f;

    // 注册防御塔战斗系统。
    public static void RegisterDefenseSystem(DefenseSystem defenseSystem)
    {
        AddUnique(DefenseSystemList, defenseSystem);
    }

    // 取消注册防御塔战斗系统。
    public static void UnregisterDefenseSystem(DefenseSystem defenseSystem)
    {
        DefenseSystemList.Remove(defenseSystem);
    }

    // 注册防御塔建筑。
    public static void RegisterDefenseBuilding(Building building)
    {
        AddUnique(DefenseBuildingList, building);
    }

    // 取消注册防御塔建筑。
    public static void UnregisterDefenseBuilding(Building building)
    {
        DefenseBuildingList.Remove(building);
    }

    // 注册升星按钮。
    public static void RegisterUpgradeButton(BuildingUpgradeButton upgradeButton)
    {
        AddUnique(UpgradeButtonList, upgradeButton);
    }

    // 取消注册升星按钮。
    public static void UnregisterUpgradeButton(BuildingUpgradeButton upgradeButton)
    {
        UpgradeButtonList.Remove(upgradeButton);
    }

    // 注册基地生命系统。
    public static void RegisterHomeHealthSystem(HealthSystem healthSystem)
    {
        if (healthSystem)
        {
            HomeHealthSystem = healthSystem;
        }
    }

    // 取消注册基地生命系统。
    public static void UnregisterHomeHealthSystem(HealthSystem healthSystem)
    {
        if (HomeHealthSystem == healthSystem)
        {
            HomeHealthSystem = null;
        }
    }

    // 统计当前三星防御塔数量。
    public static int GetThreeStarDefenseCount()
    {
        int count = 0;
        foreach (DefenseSystem defenseSystem in DefenseSystemList)
        {
            if (defenseSystem && defenseSystem.CurrentStarLevel >= 3)
            {
                count++;
            }
        }

        return count;
    }

    // 判断指定防御塔附近是否存在另一座防御塔。
    public static bool HasNearbyDefenseTower(DefenseSystem sourceDefenseSystem, float radius)
    {
        if (!sourceDefenseSystem || radius <= 0f)
        {
            return false;
        }

        float radiusSqr = radius * radius;
        Vector3 sourcePosition = sourceDefenseSystem.transform.position;

        foreach (DefenseSystem defenseSystem in DefenseSystemList)
        {
            if (!defenseSystem || defenseSystem == sourceDefenseSystem)
            {
                continue;
            }

            if ((defenseSystem.transform.position - sourcePosition).sqrMagnitude <= radiusSqr)
            {
                return true;
            }
        }

        return false;
    }

    // 获取一座可升星的随机防御塔。
    public static BuildingUpgradeButton GetRandomUpgradeableTower()
    {
        List<BuildingUpgradeButton> upgradeableButtonList = new List<BuildingUpgradeButton>();
        foreach (BuildingUpgradeButton upgradeButton in UpgradeButtonList)
        {
            if (upgradeButton && !upgradeButton.IsMaxStar)
            {
                upgradeableButtonList.Add(upgradeButton);
            }
        }

        if (upgradeableButtonList.Count <= 0)
        {
            return null;
        }

        int index = UnityEngine.Random.Range(0, upgradeableButtonList.Count);
        return upgradeableButtonList[index];
    }

    // 获取指定防御塔对应的升星按钮。
    public static BuildingUpgradeButton GetUpgradeButton(DefenseSystem defenseSystem)
    {
        if (!defenseSystem)
        {
            return null;
        }

        foreach (BuildingUpgradeButton upgradeButton in UpgradeButtonList)
        {
            if (upgradeButton && upgradeButton.DefenseSystem == defenseSystem)
            {
                return upgradeButton;
            }
        }

        return null;
    }

    // 按最大生命百分比治疗所有防御塔。
    public static void HealAllDefenseTowers(float healPercent)
    {
        if (healPercent <= 0f)
        {
            return;
        }

        foreach (Building building in DefenseBuildingList)
        {
            if (building)
            {
                building.HealByMaxHealthPercent(healPercent);
            }
        }
    }

    // 向列表添加唯一元素。
    private static void AddUnique<T>(List<T> targetList, T item) where T : UnityEngine.Object
    {
        if (!item || targetList.Contains(item))
        {
            return;
        }

        targetList.Add(item);
    }
}

/// <summary>
/// 防御塔即时奖励执行器，负责处理选择卡牌后立刻发生的防御塔效果。
/// </summary>
public static class DefenseRewardImmediateEffectApplier
{
    // 随机选择一座可升级防御塔并升到满星。
    public static void UpgradeRandomTowerToMaxStar()
    {
        BuildingUpgradeButton upgradeButton = DefenseTowerRegistry.GetRandomUpgradeableTower();
        if (!upgradeButton)
        {
            return;
        }

        upgradeButton.UpgradeToMaxStarWithoutCost();
    }
}

/// <summary>
/// 防御塔波次结算奖励执行器，负责处理每波结束时触发的奖励。
/// </summary>
public static class DefenseRewardWaveEndApplier
{
    // 按配置比例治疗所有防御塔。
    public static void ApplyWaveEndHeal(float healPercent)
    {
        DefenseTowerRegistry.HealAllDefenseTowers(healPercent);
    }
}
