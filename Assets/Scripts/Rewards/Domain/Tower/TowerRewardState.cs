using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tower 奖励状态，负责保存、校验和聚合本局 Tower 奖励配置。
/// </summary>
public sealed class TowerRewardState
{
    private const float MIN_ATTRIBUTE_MULTIPLIER = 0.01f;
    private const float MIN_COST_MULTIPLIER = 0f;
    private const float DEFAULT_MULTIPLIER = 1f;

    private readonly List<TowerExtraAttackRule> _extraAttackRuleList = new();
    private readonly List<TowerAttackHealthCostRule> _attackHealthCostRuleList = new();

    private float _attackDamageBonus;
    private float _attackSpeedBonus;
    private float _detectRadiusBonus;
    private float _maxHealthBonus;
    private float _buildCostBonus;
    private float _armorIgnorePercent;
    private float _damageTakenBonus;
    private float _waveEndHealPercent;
    private int _killCountAutoUpgrade;
    private float _attackSpeedOverloadBonus;
    private float _attackDamagePerThreeStarTower;
    private float _doubleDamageChance;
    private float _linkedAttackSpeedBonus;
    private float _linkRadius;
    private int _newTowerInitialStarBonus;
    private float _finalDefenseAttackDamageBonus;
    private float _finalDefenseHomeHealthThreshold;
    private bool _threeStarExplosiveArrowEnabled;
    private float _explosionRadius;
    private float _explosionDamageMultiplier;

    public float AttackDamageMultiplier => GetAttributeMultiplier(_attackDamageBonus);
    public float AttackIntervalMultiplier => GetAttackIntervalMultiplier(_attackSpeedBonus);
    public float DetectRadiusMultiplier => GetAttributeMultiplier(_detectRadiusBonus);
    public float MaxHealthMultiplier => GetAttributeMultiplier(_maxHealthBonus);
    public float BuildCostMultiplier => GetCostMultiplier();
    public float ArmorIgnorePercent => Mathf.Clamp01(_armorIgnorePercent);
    public IReadOnlyList<TowerExtraAttackRule> ExtraAttackRuleList => _extraAttackRuleList;
    public float DamageTakenMultiplier => GetAttributeMultiplier(_damageTakenBonus);
    public float WaveEndHealPercent => Mathf.Max(0f, _waveEndHealPercent);
    public int KillCountAutoUpgrade => Mathf.Max(0, _killCountAutoUpgrade);
    public float OverloadAttackIntervalMultiplier => GetAttackIntervalMultiplier(_attackSpeedOverloadBonus);
    public float AttackDamagePerThreeStarTower => Mathf.Max(0f, _attackDamagePerThreeStarTower);
    public float DoubleDamageChance => Mathf.Clamp01(_doubleDamageChance);
    public float LinkedAttackIntervalMultiplier => GetAttackIntervalMultiplier(_linkedAttackSpeedBonus);
    public float LinkRadius => Mathf.Max(0f, _linkRadius);
    public int NewTowerInitialStarBonus => Mathf.Max(0, _newTowerInitialStarBonus);
    public float FinalDefenseAttackDamageBonus => Mathf.Max(0f, _finalDefenseAttackDamageBonus);
    public float FinalDefenseHomeHealthThreshold => Mathf.Clamp01(_finalDefenseHomeHealthThreshold);
    public bool ThreeStarExplosiveArrowEnabled => _threeStarExplosiveArrowEnabled;
    public float ExplosionRadius => Mathf.Max(0f, _explosionRadius);
    public float ExplosionDamageMultiplier => Mathf.Max(0f, _explosionDamageMultiplier);

    // 累加攻击力奖励。
    public void AddAttackDamageBonus(float bonus)
    {
        _attackDamageBonus += bonus;
    }

    // 累加攻击速度奖励。
    public void AddAttackSpeedBonus(float bonus)
    {
        _attackSpeedBonus += bonus;
    }

    // 累加探测范围奖励。
    public void AddDetectRadiusBonus(float bonus)
    {
        _detectRadiusBonus += bonus;
    }

    // 累加最大生命值奖励。
    public void AddMaxHealthBonus(float bonus)
    {
        _maxHealthBonus += bonus;
    }

    // 累加建造成本奖励。
    public void AddBuildCostBonus(float bonus)
    {
        _buildCostBonus += bonus;
    }

    // 累加护甲穿透奖励。
    public void AddArmorIgnorePercent(float percent)
    {
        _armorIgnorePercent += percent;
    }

    // 添加额外攻击规则。
    public void AddExtraAttackRule(int triggerAttackCount, int extraAttackCount)
    {
        if (triggerAttackCount <= 0 || extraAttackCount <= 0)
        {
            return;
        }

        _extraAttackRuleList.Add(new TowerExtraAttackRule(triggerAttackCount, extraAttackCount));
    }

    // 累加受到伤害变化奖励。
    public void AddDamageTakenBonus(float bonus)
    {
        _damageTakenBonus += bonus;
    }

    // 累加波末回血奖励。
    public void AddWaveEndHealPercent(float percent)
    {
        _waveEndHealPercent += percent;
    }

    // 设置击杀自动升星阈值，多个阈值取更容易触发的较小值。
    public void AddKillCountAutoUpgrade(int killCountToUpgrade)
    {
        if (killCountToUpgrade <= 0)
        {
            return;
        }

        _killCountAutoUpgrade = _killCountAutoUpgrade <= 0
            ? killCountToUpgrade
            : Mathf.Min(_killCountAutoUpgrade, killCountToUpgrade);
    }

    // 累加超载攻击速度奖励。
    public void AddAttackSpeedOverloadBonus(float bonus)
    {
        _attackSpeedOverloadBonus += bonus;
    }

    // 添加攻击损失生命规则。
    public void AddAttackHealthCostRule(int triggerAttackCount, int healthCost)
    {
        if (triggerAttackCount <= 0 || healthCost <= 0)
        {
            return;
        }

        _attackHealthCostRuleList.Add(new TowerAttackHealthCostRule(triggerAttackCount, healthCost));
    }

    // 累加每座三星塔提供的攻击力奖励。
    public void AddAttackDamagePerThreeStarTower(float bonus)
    {
        _attackDamagePerThreeStarTower += bonus;
    }

    // 累加双倍伤害概率。
    public void AddDoubleDamageChance(float chance)
    {
        _doubleDamageChance += chance;
    }

    // 累加防线联动攻击速度奖励并记录最大联动半径。
    public void AddLinkedAttackSpeed(float attackSpeedBonus, float linkRadius)
    {
        _linkedAttackSpeedBonus += attackSpeedBonus;
        _linkRadius = Mathf.Max(_linkRadius, linkRadius);
    }

    // 累加新建塔初始星级奖励。
    public void AddNewTowerInitialStarBonus(int starBonus)
    {
        _newTowerInitialStarBonus += Mathf.Max(0, starBonus);
    }

    // 累加最终防线奖励并记录更容易触发的生命阈值。
    public void AddFinalDefense(float attackDamageBonus, float homeHealthThreshold)
    {
        _finalDefenseAttackDamageBonus += attackDamageBonus;
        _finalDefenseHomeHealthThreshold = _finalDefenseHomeHealthThreshold <= 0f
            ? homeHealthThreshold
            : Mathf.Min(_finalDefenseHomeHealthThreshold, homeHealthThreshold);
    }

    // 开启三星爆裂箭并记录更强的爆炸配置。
    public void AddThreeStarExplosiveArrow(float explosionRadius, float explosionDamageMultiplier)
    {
        _threeStarExplosiveArrowEnabled = true;
        _explosionRadius = Mathf.Max(_explosionRadius, explosionRadius);
        _explosionDamageMultiplier = Mathf.Max(_explosionDamageMultiplier, explosionDamageMultiplier);
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
        return Mathf.Max(MIN_COST_MULTIPLIER, DEFAULT_MULTIPLIER + _buildCostBonus);
    }
}
