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

[Serializable]
public class RewardEffectConfig
{
    [SerializeField] private RewardEffectType effectType;
    [SerializeField] private float value;

    public RewardEffectType EffectType => effectType;
    public float Value => value;
}
