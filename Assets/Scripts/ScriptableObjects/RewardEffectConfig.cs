using System;
using UnityEngine;

/// <summary>
/// 卡牌奖励类型（攻击力、攻速）
/// </summary>
public enum RewardEffectType
{
    DefenseAttackDamageMultiplier,
    DefenseAttackSpeedMultiplier,
    DefenseDetectRadiusMultiplier,
    DefenseMaxHealthMultiplier,
    DefenseBuildCostMultiplier
}

/// <summary>
/// 卡牌奖励是否正向
/// </summary>
public enum RewardEffectDisplayImpact
{
    Auto,
    Positive,
    Negative,
    Neutral
}

[Serializable]
public class RewardEffectConfig
{
    [SerializeField] private RewardEffectDefinitionSo effectDefinition;
    [HideInInspector]
    [SerializeField] private RewardEffectType effectType;
    [SerializeField] private float value;
    [SerializeField] private RewardEffectDisplayImpact displayImpact = RewardEffectDisplayImpact.Auto;

    public RewardEffectDefinitionSo EffectDefinition => effectDefinition;
    public RewardEffectType EffectType => effectDefinition ? effectDefinition.EffectType : effectType;
    public float Value => value;
    public RewardEffectDisplayImpact DisplayImpact => displayImpact;

    // 构建当前效果配置的显示描述。
    public string BuildDescription(string valueText)
    {
        return effectDefinition ? effectDefinition.BuildDescription(valueText) : valueText;
    }

    // 解析当前效果配置的显示倾向。
    public RewardEffectDisplayImpact ResolveDisplayImpact()
    {
        return effectDefinition ? effectDefinition.ResolveDisplayImpact(value, displayImpact) : displayImpact;
    }
}
