using System;
using UnityEngine;

public enum RewardEffectType
{
    DefenseAttackDamageMultiplier,
    DefenseAttackSpeedMultiplier,
    DefenseDetectRadiusMultiplier,
    DefenseMaxHealthMultiplier,
    DefenseBuildCostMultiplier
}

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
    [SerializeField] private RewardEffectType effectType;
    [SerializeField] private float value;
    [SerializeField] private RewardEffectDisplayImpact displayImpact = RewardEffectDisplayImpact.Auto;

    public RewardEffectType EffectType => effectType;
    public float Value => value;
    public RewardEffectDisplayImpact DisplayImpact => displayImpact;
}
