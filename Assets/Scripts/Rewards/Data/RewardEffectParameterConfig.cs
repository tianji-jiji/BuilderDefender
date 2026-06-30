using System;
using UnityEngine;

/// <summary>
/// 单个奖励效果参数配置。
/// </summary>
[Serializable]
public class RewardEffectParameterConfig
{
    [SerializeField] private string parameterId;
    [SerializeField] private float value;
    [SerializeField] private RewardEffectDisplayImpact displayImpactOverride = RewardEffectDisplayImpact.Auto;

    public string ParameterId => string.IsNullOrWhiteSpace(parameterId)
        ? RewardEffectParameterIds.VALUE
        : parameterId.Trim();
    public float Value => value;
    public RewardEffectDisplayImpact DisplayImpactOverride => displayImpactOverride;
}
