using System;
using System.Collections.Generic;
using UnityEngine;

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
/// 奖励效果参数 ID 常量，用字符串和 Handler 约束参数名。
/// </summary>
public static class RewardEffectParameterIds
{
    public const string VALUE = "Value";
    public const string TRIGGER_ATTACK_COUNT = "TriggerAttackCount";
    public const string EXTRA_ATTACK_COUNT = "ExtraAttackCount";
    public const string ARMOR_IGNORE_PERCENT = "ArmorIgnorePercent";
    public const string DAMAGE_TAKEN_MULTIPLIER = "DamageTakenMultiplier";
    public const string WAVE_END_HEAL_PERCENT = "WaveEndHealPercent";
    public const string KILL_COUNT_TO_UPGRADE = "KillCountToUpgrade";
    public const string ATTACK_SPEED_MULTIPLIER = "AttackSpeedMultiplier";
    public const string ATTACK_HEALTH_COST = "AttackHealthCost";
    public const string DAMAGE_BONUS_PER_THREE_STAR_TOWER = "DamageBonusPerThreeStarTower";
    public const string DOUBLE_DAMAGE_CHANCE = "DoubleDamageChance";
    public const string LINK_RADIUS = "LinkRadius";
    public const string INITIAL_STAR_BONUS = "InitialStarBonus";
    public const string HOME_HEALTH_THRESHOLD = "HomeHealthThreshold";
    public const string EXPLOSION_RADIUS = "ExplosionRadius";
    public const string EXPLOSION_DAMAGE_MULTIPLIER = "ExplosionDamageMultiplier";
}

/// <summary>
/// 单个奖励效果参数配置，保存参数 ID、数值和可选显示倾向覆盖。
/// </summary>
[Serializable]
public class RewardEffectParameterConfig
{
    [SerializeField] private string parameterId;
    [SerializeField] private float value;
    [SerializeField] private RewardEffectDisplayImpact displayImpactOverride = RewardEffectDisplayImpact.Auto;

    public string ParameterId => string.IsNullOrWhiteSpace(parameterId) ? RewardEffectParameterIds.VALUE : parameterId.Trim();
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
    [SerializeField] private List<RewardEffectParameterConfig> parameterConfigList = new();
    [HideInInspector] [SerializeField] private float value;
    [HideInInspector] [SerializeField] private RewardEffectDisplayImpact displayImpact = RewardEffectDisplayImpact.Auto;

    public RewardEffectDefinitionSo EffectDefinition => effectDefinition;
    public IReadOnlyList<RewardEffectParameterConfig> ParameterConfigList => parameterConfigList;
    public float LegacyValue => value;
    public RewardEffectDisplayImpact LegacyDisplayImpact => displayImpact;

    // 判断当前效果是否已经迁移到参数列表配置。
    public bool HasParameterList()
    {
        return parameterConfigList != null && parameterConfigList.Count > 0;
    }

    // 构建当前效果配置的显示描述。
    public string BuildDescription(IReadOnlyDictionary<string, string> parameterTextDic)
    {
        if (!effectDefinition)
        {
            return parameterTextDic != null && parameterTextDic.TryGetValue(RewardEffectParameterIds.VALUE, out string valueText)
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
    public static float GetFloat(RewardEffectConfig effectConfig, string parameterId, float defaultValue, bool logMissingWarning = false)
    {
        if (TryGetFloat(effectConfig, parameterId, out float value))
        {
            return value;
        }

        LogMissingParameter(effectConfig, parameterId, logMissingWarning);
        return defaultValue;
    }

    // 读取整数参数，缺失时返回默认值。
    public static int GetInt(RewardEffectConfig effectConfig, string parameterId, int defaultValue, bool logMissingWarning = false)
    {
        if (TryGetFloat(effectConfig, parameterId, out float value))
        {
            return Mathf.RoundToInt(value);
        }

        LogMissingParameter(effectConfig, parameterId, logMissingWarning);
        return defaultValue;
    }

    // 尝试读取浮点参数。
    public static bool TryGetFloat(RewardEffectConfig effectConfig, string parameterId, out float value)
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
                if (parameterConfig == null || !IsSameParameterId(parameterConfig.ParameterId, parameterId))
                {
                    continue;
                }

                value = parameterConfig.Value;
                return true;
            }
        }

        if (IsSameParameterId(parameterId, RewardEffectParameterIds.VALUE) && !effectConfig.HasParameterList())
        {
            value = effectConfig.LegacyValue;
            return true;
        }

        return false;
    }

    // 在需要时输出缺失参数警告。
    private static void LogMissingParameter(RewardEffectConfig effectConfig, string parameterId, bool shouldLog)
    {
        if (!shouldLog)
        {
            return;
        }

        string effectName = effectConfig != null && effectConfig.EffectDefinition ? effectConfig.EffectDefinition.name : "NullEffect";
        Debug.LogWarning($"Reward effect parameter missing: {effectName}.{parameterId}");
    }

    // 判断两个参数 ID 是否一致。
    private static bool IsSameParameterId(string leftId, string rightId)
    {
        return string.Equals(NormalizeParameterId(leftId), NormalizeParameterId(rightId), StringComparison.Ordinal);
    }

    // 标准化参数 ID。
    private static string NormalizeParameterId(string parameterId)
    {
        return string.IsNullOrWhiteSpace(parameterId) ? string.Empty : parameterId.Trim();
    }
}
