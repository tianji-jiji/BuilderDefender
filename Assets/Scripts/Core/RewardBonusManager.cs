using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardBonusManager : MonoBehaviour
{
    private const float MIN_ATTRIBUTE_MULTIPLIER = 0.01f;
    private const float MIN_COST_MULTIPLIER = 0f;
    private const float DEFAULT_MULTIPLIER = 1f;

    public static RewardBonusManager Instance;

    // 奖励倍率发生变化时通知所有需要刷新属性的对象。
    public static event Action OnRewardBonusChanged;

    private float _defenseAttackDamageBonus;
    private float _defenseAttackSpeedBonus;
    private float _defenseDetectRadiusBonus;
    private float _defenseMaxHealthBonus;
    private float _defenseBuildCostBonus;

    public float DefenseAttackDamageMultiplier => GetAttributeMultiplier(_defenseAttackDamageBonus);
    public float DefenseAttackIntervalMultiplier => GetAttackIntervalMultiplier();
    public float DefenseDetectRadiusMultiplier => GetAttributeMultiplier(_defenseDetectRadiusBonus);
    public float DefenseMaxHealthMultiplier => GetAttributeMultiplier(_defenseMaxHealthBonus);
    public float DefenseBuildCostMultiplier => GetCostMultiplier();

    // 初始化全局奖励管理器单例。
    private void Awake()
    {
        Instance = this;
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
                _defenseAttackDamageBonus += effectConfig.Value;
                break;

            case RewardEffectType.DefenseAttackSpeedMultiplier:
                _defenseAttackSpeedBonus += effectConfig.Value;
                break;

            case RewardEffectType.DefenseDetectRadiusMultiplier:
                _defenseDetectRadiusBonus += effectConfig.Value;
                break;

            case RewardEffectType.DefenseMaxHealthMultiplier:
                _defenseMaxHealthBonus += effectConfig.Value;
                break;

            case RewardEffectType.DefenseBuildCostMultiplier:
                _defenseBuildCostBonus += effectConfig.Value;
                break;
        }
    }

    // 根据奖励加成计算属性倍率。
    private float GetAttributeMultiplier(float bonus)
    {
        return Mathf.Max(MIN_ATTRIBUTE_MULTIPLIER, DEFAULT_MULTIPLIER + bonus);
    }

    // 根据攻速加成计算攻击间隔倍率。
    private float GetAttackIntervalMultiplier()
    {
        float attackSpeedMultiplier = Mathf.Max(MIN_ATTRIBUTE_MULTIPLIER, DEFAULT_MULTIPLIER + _defenseAttackSpeedBonus);
        return DEFAULT_MULTIPLIER / attackSpeedMultiplier;
    }

    // 根据建造成本加成计算成本倍率。
    private float GetCostMultiplier()
    {
        return Mathf.Max(MIN_COST_MULTIPLIER, DEFAULT_MULTIPLIER + _defenseBuildCostBonus);
    }
}
