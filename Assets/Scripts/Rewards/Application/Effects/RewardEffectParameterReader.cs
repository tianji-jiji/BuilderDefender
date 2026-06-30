using System;
using UnityEngine;

/// <summary>
/// 奖励效果参数读取器，负责从配置中安全读取玩法数值。
/// </summary>
public static class RewardEffectParameterReader
{
    // 读取浮点参数，缺失时返回默认值。
    public static float GetFloat(
        RewardCardEffectConfig cardEffectConfig,
        string parameterId,
        float defaultValue,
        bool logMissingWarning = false)
    {
        if (TryGetFloat(cardEffectConfig, parameterId, out float value))
        {
            return value;
        }

        if (logMissingWarning)
        {
            Debug.LogWarning($"奖励参数缺失：{parameterId}");
        }

        return defaultValue;
    }

    // 读取整数参数，缺失时返回默认值。
    public static int GetInt(
        RewardCardEffectConfig cardEffectConfig,
        string parameterId,
        int defaultValue,
        bool logMissingWarning = false)
    {
        if (TryGetFloat(cardEffectConfig, parameterId, out float value))
        {
            return Mathf.RoundToInt(value);
        }

        if (logMissingWarning)
        {
            Debug.LogWarning($"奖励参数缺失：{parameterId}");
        }

        return defaultValue;
    }

    // 尝试读取浮点参数。
    private static bool TryGetFloat(
        RewardCardEffectConfig cardEffectConfig,
        string parameterId,
        out float value)
    {
        value = 0f;
        if (cardEffectConfig?.ParameterConfigList == null)
        {
            return false;
        }

        foreach (RewardEffectParameterConfig parameterConfig in cardEffectConfig.ParameterConfigList)
        {
            if (parameterConfig == null || !IsSameParameterId(parameterConfig.ParameterId, parameterId))
            {
                continue;
            }

            value = parameterConfig.Value;
            return true;
        }

        return false;
    }

    // 判断两个参数 ID 是否一致。
    private static bool IsSameParameterId(string leftId, string rightId)
    {
        return string.Equals(
            NormalizeParameterId(leftId),
            NormalizeParameterId(rightId),
            StringComparison.Ordinal);
    }

    // 标准化参数 ID。
    private static string NormalizeParameterId(string parameterId)
    {
        return string.IsNullOrWhiteSpace(parameterId) ? string.Empty : parameterId.Trim();
    }
}
