using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 奖励卡牌描述格式化器，负责把奖励效果配置转换为可显示的富文本。
/// </summary>
public static class RewardCardDescriptionFormatter
{
    private const string POSITIVE_COLOR_HEX = "#55FF77";
    private const string NEGATIVE_COLOR_HEX = "#FF5A5A";
    private const string NEUTRAL_COLOR_HEX = "#FFFFFF";

    // 构建奖励卡牌描述文本。
    public static string BuildDescriptionText(RewardCardSo rewardCard)
    {
        if (!rewardCard || rewardCard.EffectConfigList == null || rewardCard.EffectConfigList.Count <= 0)
        {
            return string.Empty;
        }

        StringBuilder descriptionBuilder = new();
        foreach (RewardEffectConfig effectConfig in rewardCard.EffectConfigList)
        {
            if (effectConfig == null)
            {
                continue;
            }

            if (descriptionBuilder.Length > 0)
            {
                descriptionBuilder.AppendLine();
            }

            descriptionBuilder.Append(BuildEffectDescription(effectConfig));
        }

        return descriptionBuilder.ToString();
    }

    // 构建单个奖励效果的描述文本。
    private static string BuildEffectDescription(RewardEffectConfig effectConfig)
    {
        Dictionary<string, string> parameterTextDic = BuildParameterTextDic(effectConfig);
        return effectConfig.BuildDescription(parameterTextDic);
    }

    // 构建当前效果全部参数的富文本字典。
    private static Dictionary<string, string> BuildParameterTextDic(RewardEffectConfig effectConfig)
    {
        Dictionary<string, string> parameterTextDic = new();
        bool hasParameterList = effectConfig.HasParameterList();

        if (hasParameterList)
        {
            foreach (RewardEffectParameterConfig parameterConfig in effectConfig.ParameterConfigList)
            {
                if (parameterConfig == null)
                {
                    continue;
                }

                parameterTextDic[parameterConfig.ParameterId] = BuildColoredParameterText(effectConfig, parameterConfig);
            }

            return parameterTextDic;
        }

        string valueText = BuildColoredParameterText(effectConfig, RewardEffectParameterIds.VALUE, effectConfig.LegacyValue, effectConfig.LegacyDisplayImpact);
        parameterTextDic[RewardEffectParameterIds.VALUE] = valueText;
        return parameterTextDic;
    }

    // 构建单个参数的富文本。
    private static string BuildColoredParameterText(RewardEffectConfig effectConfig, RewardEffectParameterConfig parameterConfig)
    {
        return BuildColoredParameterText(effectConfig, parameterConfig.ParameterId, parameterConfig.Value, parameterConfig.DisplayImpactOverride);
    }

    // 构建指定参数 ID 和值的富文本。
    private static string BuildColoredParameterText(RewardEffectConfig effectConfig, string parameterId, float value, RewardEffectDisplayImpact displayImpactOverride)
    {
        RewardEffectDefinitionSo effectDefinition = effectConfig.EffectDefinition;
        RewardEffectValueFormat valueFormat = effectDefinition ? effectDefinition.GetValueFormat(parameterId) : RewardEffectValueFormat.PercentWithSign;
        RewardEffectDisplayImpact displayImpact = effectDefinition
            ? effectDefinition.ResolveDisplayImpact(parameterId, value, displayImpactOverride)
            : displayImpactOverride;
        string valueText = FormatValue(value, valueFormat);
        string colorHex = GetImpactColorHex(displayImpact);
        return $"<color={colorHex}>{valueText}</color>";
    }

    // 获取显示倾向对应的富文本颜色。
    private static string GetImpactColorHex(RewardEffectDisplayImpact displayImpact)
    {
        switch (displayImpact)
        {
            case RewardEffectDisplayImpact.Positive:
                return POSITIVE_COLOR_HEX;
            case RewardEffectDisplayImpact.Negative:
                return NEGATIVE_COLOR_HEX;
            case RewardEffectDisplayImpact.Neutral:
                return NEUTRAL_COLOR_HEX;
            default:
                return NEUTRAL_COLOR_HEX;
        }
    }

    // 按定义格式化参数数值。
    private static string FormatValue(float value, RewardEffectValueFormat valueFormat)
    {
        switch (valueFormat)
        {
            case RewardEffectValueFormat.PercentWithSign:
                return FormatPercent(value, true);
            case RewardEffectValueFormat.PercentWithoutSign:
                return FormatPercent(value, false);
            case RewardEffectValueFormat.IntegerWithSign:
                return FormatInteger(value, true);
            case RewardEffectValueFormat.IntegerWithoutSign:
                return FormatInteger(value, false);
            case RewardEffectValueFormat.NumberOnly:
                return FormatNumber(value);
            default:
                return FormatNumber(value);
        }
    }

    // 把倍率增量格式化成百分比文本。
    private static string FormatPercent(float value, bool includePositiveSign)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        return includePositiveSign && percent > 0 ? $"+{percent}%" : $"{percent}%";
    }

    // 把数值格式化成整数文本。
    private static string FormatInteger(float value, bool includePositiveSign)
    {
        int integerValue = Mathf.RoundToInt(value);
        return includePositiveSign && integerValue > 0 ? $"+{integerValue}" : integerValue.ToString();
    }

    // 把数值格式化成普通数字文本。
    private static string FormatNumber(float value)
    {
        return value.ToString("0.##");
    }
}
