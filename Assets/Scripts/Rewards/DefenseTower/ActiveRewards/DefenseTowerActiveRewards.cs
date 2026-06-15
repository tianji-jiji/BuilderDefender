using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励账本，保存本局所有防御塔奖励
/// 防御塔有哪些全局 buff，并且它的总配置是多少
/// </summary>
public class DefenseTowerActiveRewards
{
    private const float MIN_ATTRIBUTE_MULTIPLIER = 0.01f;
    private const float MIN_COST_MULTIPLIER = 0f;
    private const float DEFAULT_MULTIPLIER = 1f;

    private readonly List<DefenseTowerExtraAttackRule> _defenseTowerExtraAttackRuleList = new();
    private readonly List<DefenseTowerAttackHealthCostRule> _attackHealthCostRuleList = new();

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
    public IReadOnlyList<DefenseTowerExtraAttackRule> ExtraAttackRuleList => _defenseTowerExtraAttackRuleList;
    public float DamageTakenMultiplier => GetAttributeMultiplier(_damageTakenBonus);
    public float WaveEndHealPercent => Mathf.Max(0f, _waveEndHealPercent);
    public int KillCountAutoUpgrade => Mathf.Max(0, _killCountAutoUpgrade);
    public float OverloadAttackIntervalMultiplier => GetAttackIntervalMultiplier(_attackSpeedOverloadBonus);
    public IReadOnlyList<DefenseTowerAttackHealthCostRule> AttackHealthCostRuleList => _attackHealthCostRuleList;
    public float AttackDamagePerThreeStarTower => Mathf.Max(0f, _attackDamagePerThreeStarTower);
    public float DoubleDamageChance => Mathf.Clamp01(_doubleDamageChance);
    public float LinkedAttackIntervalMultiplier => GetAttackIntervalMultiplier(_linkedAttackSpeedBonus);
    public float LinkRadius => Mathf.Max(0f, _linkRadius);
    public int NewTowerInitialStarBonus => Mathf.Max(0, _newTowerInitialStarBonus);
    public bool ThreeStarExplosiveArrowEnabled => _threeStarExplosiveArrowEnabled;
    public float ExplosionRadius => Mathf.Max(0f, _explosionRadius);
    public float ExplosionDamageMultiplier => Mathf.Max(0f, _explosionDamageMultiplier);

    // 计算额外攻击规则带来的平均输出倍率。
    public float GetExtraAttackPowerMultiplier()
    {
        float extraAttackPower = 0f;
        foreach (DefenseTowerExtraAttackRule extraAttackRule in _defenseTowerExtraAttackRuleList)
        {
            extraAttackPower += (float)extraAttackRule.extraAttackCount / extraAttackRule.triggerAttackCount;
        }

        return DEFAULT_MULTIPLIER + Mathf.Max(0f, extraAttackPower);
    }

    // 计算双倍伤害概率带来的平均输出倍率。
    public float GetCriticalPowerMultiplier()
    {
        return DEFAULT_MULTIPLIER + DoubleDamageChance;
    }

    // 计算爆炸箭带来的粗略范围输出倍率。
    public float GetExplosivePowerMultiplier()
    {
        if (!_threeStarExplosiveArrowEnabled || ExplosionRadius <= 0f || ExplosionDamageMultiplier <= 0f)
        {
            return DEFAULT_MULTIPLIER;
        }

        return DEFAULT_MULTIPLIER + Mathf.Clamp(ExplosionRadius * ExplosionDamageMultiplier * 0.1f, 0f, 1.5f);
    }

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

        _defenseTowerExtraAttackRuleList.Add(new DefenseTowerExtraAttackRule(triggerAttackCount, extraAttackCount));
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

        _attackHealthCostRuleList.Add(new DefenseTowerAttackHealthCostRule(triggerAttackCount, healthCost));
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

    // 判断最终防线奖励当前是否处于激活状态。
    public bool IsFinalDefenseActive()
    {
        if (_finalDefenseAttackDamageBonus <= 0f || _finalDefenseHomeHealthThreshold <= 0f)
        {
            return false;
        }

        return DefenseTowerRegistry.HomeHealthNormalized <= _finalDefenseHomeHealthThreshold;
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        return IsFinalDefenseActive() ? GetAttributeMultiplier(_finalDefenseAttackDamageBonus) : DEFAULT_MULTIPLIER;
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
