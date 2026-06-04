using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌奖励类型，描述奖励在玩法系统中的实际生效入口。
/// </summary>
public enum RewardEffectType
{
    DefenseAttackDamageMultiplier,
    DefenseAttackSpeedMultiplier,
    DefenseDetectRadiusMultiplier,
    DefenseMaxHealthMultiplier,
    DefenseBuildCostMultiplier,
    DefenseArmorIgnorePercent,
    DefenseExtraArrowEveryAttackCount,
    DefenseDamageTakenMultiplier,
    DefenseWaveEndHealPercent,
    DefenseKillCountAutoUpgrade,
    DefenseAttackSpeedOverloadMultiplier,
    DefenseAttackHealthCost,
    DefenseAttackDamagePerThreeStarTower,
    DefenseDoubleDamageChance,
    DefenseLinkedAttackSpeedMultiplier,
    DefenseRandomTowerMaxStar,
    DefenseNewTowerInitialStarBonus,
    DefenseFinalDefenseAttackDamageMultiplier,
    DefenseThreeStarExplosiveArrow
}

/// <summary>
/// 奖励效果参数键，避免在配置和代码中手写字符串。
/// </summary>
public enum RewardEffectParameterKey
{
    Value,
    TriggerAttackCount,
    ExtraAttackCount,
    ArmorIgnorePercent,
    DamageTakenMultiplier,
    WaveEndHealPercent,
    KillCountToUpgrade,
    AttackSpeedMultiplier,
    AttackHealthCost,
    DamageBonusPerThreeStarTower,
    DoubleDamageChance,
    LinkRadius,
    InitialStarBonus,
    HomeHealthThreshold,
    ExplosionRadius,
    ExplosionDamageMultiplier
}

/// <summary>
/// 卡牌奖励参数的显示倾向，用于决定富文本颜色。
/// </summary>
public enum RewardEffectDisplayImpact
{
    Auto,
    Positive,
    Negative,
    Neutral
}

/// <summary>
/// 单个奖励效果参数配置，保存参数键、数值和可选显示倾向覆盖。
/// </summary>
[Serializable]
public class RewardEffectParameterConfig
{
    [SerializeField] private RewardEffectParameterKey parameterKey = RewardEffectParameterKey.Value;
    [SerializeField] private float value;
    [SerializeField] private RewardEffectDisplayImpact displayImpactOverride = RewardEffectDisplayImpact.Auto;

    public RewardEffectParameterKey ParameterKey => parameterKey;
    public float Value => value;
    public RewardEffectDisplayImpact DisplayImpactOverride => displayImpactOverride;
}

/// <summary>
/// 单个卡牌奖励效果配置，保存效果定义和这张卡上的参数列表。
/// </summary>
[Serializable]
public class RewardEffectConfig
{
    [SerializeField] private RewardEffectDefinitionSo effectDefinition;
    [SerializeField] private List<RewardEffectParameterConfig> parameterConfigList = new List<RewardEffectParameterConfig>();
    [HideInInspector] [SerializeField] private RewardEffectType effectType;
    [HideInInspector] [SerializeField] private float value;
    [HideInInspector] [SerializeField] private RewardEffectDisplayImpact displayImpact = RewardEffectDisplayImpact.Auto;

    public RewardEffectDefinitionSo EffectDefinition => effectDefinition;
    public RewardEffectType EffectType => effectDefinition ? effectDefinition.EffectType : effectType;
    public IReadOnlyList<RewardEffectParameterConfig> ParameterConfigList => parameterConfigList;
    public float LegacyValue => value;
    public RewardEffectDisplayImpact LegacyDisplayImpact => displayImpact;

    // 判断当前效果是否已经迁移到参数列表配置。
    public bool HasParameterList()
    {
        return parameterConfigList != null && parameterConfigList.Count > 0;
    }

    // 构建当前效果配置的显示描述。
    public string BuildDescription(IReadOnlyDictionary<RewardEffectParameterKey, string> parameterTextDic)
    {
        if (!effectDefinition)
        {
            return parameterTextDic != null && parameterTextDic.TryGetValue(RewardEffectParameterKey.Value, out string valueText)
                ? valueText
                : string.Empty;
        }

        return effectDefinition.BuildDescription(parameterTextDic);
    }
}

/// <summary>
/// 奖励效果参数读取器，负责从配置中安全读取玩法数值。
/// </summary>
public static class RewardEffectParameterReader
{
    // 读取浮点参数，缺失时返回默认值。
    public static float GetFloat(RewardEffectConfig effectConfig, RewardEffectParameterKey parameterKey, float defaultValue, bool logMissingWarning = false)
    {
        if (TryGetFloat(effectConfig, parameterKey, out float value))
        {
            return value;
        }

        LogMissingParameter(effectConfig, parameterKey, logMissingWarning);
        return defaultValue;
    }

    // 读取整数参数，缺失时返回默认值。
    public static int GetInt(RewardEffectConfig effectConfig, RewardEffectParameterKey parameterKey, int defaultValue, bool logMissingWarning = false)
    {
        if (TryGetFloat(effectConfig, parameterKey, out float value))
        {
            return Mathf.RoundToInt(value);
        }

        LogMissingParameter(effectConfig, parameterKey, logMissingWarning);
        return defaultValue;
    }

    // 尝试读取浮点参数。
    public static bool TryGetFloat(RewardEffectConfig effectConfig, RewardEffectParameterKey parameterKey, out float value)
    {
        value = 0f;
        if (effectConfig == null)
        {
            return false;
        }

        if (effectConfig.ParameterConfigList != null)
        {
            foreach (RewardEffectParameterConfig parameterConfig in effectConfig.ParameterConfigList)
            {
                if (parameterConfig == null || parameterConfig.ParameterKey != parameterKey)
                {
                    continue;
                }

                value = parameterConfig.Value;
                return true;
            }
        }

        if (parameterKey == RewardEffectParameterKey.Value && !effectConfig.HasParameterList())
        {
            value = effectConfig.LegacyValue;
            return true;
        }

        return false;
    }

    // 判断效果配置中是否存在指定参数。
    public static bool HasParameter(RewardEffectConfig effectConfig, RewardEffectParameterKey parameterKey)
    {
        return TryGetFloat(effectConfig, parameterKey, out _);
    }

    // 在需要时输出缺失参数警告。
    private static void LogMissingParameter(RewardEffectConfig effectConfig, RewardEffectParameterKey parameterKey, bool shouldLog)
    {
        if (!shouldLog)
        {
            return;
        }

        string effectName = effectConfig != null ? effectConfig.EffectType.ToString() : "NullEffect";
        Debug.LogWarning($"Reward effect parameter missing: {effectName}.{parameterKey}");
    }
}
