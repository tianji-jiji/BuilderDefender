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
        foreach (RewardCardEffectConfig effectConfig in rewardCard.EffectConfigList)
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
    private static string BuildEffectDescription(RewardCardEffectConfig cardEffectConfig)
    {
        Dictionary<string, string> parameterTextDic = BuildParameterTextDic(cardEffectConfig);
        RewardEffectDefinitionSo definition = cardEffectConfig.EffectDefinition;
        if (!definition)
        {
            return parameterTextDic.TryGetValue(RewardEffectParameterIds.VALUE, out string valueText)
                ? valueText
                : string.Empty;
        }

        string description = definition.DescriptionTemplate.Replace(
            "{displayName}",
            definition.DisplayName ?? string.Empty);
        foreach (KeyValuePair<string, string> parameterTextPair in parameterTextDic)
        {
            RewardEffectParameterDisplayDefinition displayDefinition =
                definition.GetParameterDisplayDefinition(parameterTextPair.Key);
            string token = displayDefinition != null
                ? displayDefinition.TemplateToken
                : $"{{{parameterTextPair.Key}}}";
            description = description.Replace(token, parameterTextPair.Value);

            if (parameterTextPair.Key == RewardEffectParameterIds.VALUE)
            {
                description = description.Replace("{value}", parameterTextPair.Value);
            }
        }

        return description;
    }

    // 构建当前效果全部参数的富文本字典。
    private static Dictionary<string, string> BuildParameterTextDic(RewardCardEffectConfig cardEffectConfig)
    {
        Dictionary<string, string> parameterTextDic = new();
        if (!cardEffectConfig.HasParameterList())
        {
            return parameterTextDic;
        }

        foreach (RewardEffectParameterConfig parameterConfig in cardEffectConfig.ParameterConfigList)
        {
            if (parameterConfig == null)
            {
                continue;
            }

            parameterTextDic[parameterConfig.ParameterId] =
                BuildColoredParameterText(cardEffectConfig, parameterConfig);
        }

        return parameterTextDic;
    }

    // 构建单个参数的富文本。
    private static string BuildColoredParameterText(
        RewardCardEffectConfig cardEffectConfig,
        RewardEffectParameterConfig parameterConfig)
    {
        return BuildColoredParameterText(
            cardEffectConfig,
            parameterConfig.ParameterId,
            parameterConfig.Value,
            parameterConfig.DisplayImpactOverride);
    }

    // 构建指定参数 ID 和值的富文本。
    private static string BuildColoredParameterText(
        RewardCardEffectConfig cardEffectConfig,
        string parameterId,
        float value,
        RewardEffectDisplayImpact displayImpactOverride)
    {
        RewardEffectDefinitionSo definition = cardEffectConfig.EffectDefinition;
        RewardEffectParameterDisplayDefinition displayDefinition =
            definition ? definition.GetParameterDisplayDefinition(parameterId) : null;
        RewardEffectValueFormat valueFormat = displayDefinition?.ValueFormat
            ?? RewardEffectValueFormat.PercentWithSign;
        RewardEffectAutoImpactRule fallbackRule = definition
            ? definition.AutoImpactRule
            : RewardEffectAutoImpactRule.AlwaysNeutral;
        RewardEffectDisplayImpact displayImpact = ResolveDisplayImpact(
            displayDefinition,
            fallbackRule,
            value,
            displayImpactOverride);
        string valueText = FormatValue(value, valueFormat);
        return $"<color={GetImpactColorHex(displayImpact)}>{valueText}</color>";
    }

    // 解析参数显示倾向。
    private static RewardEffectDisplayImpact ResolveDisplayImpact(
        RewardEffectParameterDisplayDefinition displayDefinition,
        RewardEffectAutoImpactRule fallbackRule,
        float value,
        RewardEffectDisplayImpact displayImpactOverride)
    {
        if (displayImpactOverride != RewardEffectDisplayImpact.Auto)
        {
            return displayImpactOverride;
        }

        if (Mathf.Approximately(value, 0f))
        {
            return RewardEffectDisplayImpact.Neutral;
        }

        RewardEffectAutoImpactRule rule = displayDefinition?.AutoImpactRule ?? fallbackRule;
        return rule switch
        {
            RewardEffectAutoImpactRule.GreaterThanZeroIsPositive => value > 0f
                ? RewardEffectDisplayImpact.Positive
                : RewardEffectDisplayImpact.Negative,
            RewardEffectAutoImpactRule.LessThanZeroIsPositive => value < 0f
                ? RewardEffectDisplayImpact.Positive
                : RewardEffectDisplayImpact.Negative,
            RewardEffectAutoImpactRule.AlwaysPositive => RewardEffectDisplayImpact.Positive,
            RewardEffectAutoImpactRule.AlwaysNegative => RewardEffectDisplayImpact.Negative,
            _ => RewardEffectDisplayImpact.Neutral
        };
    }

    // 获取显示倾向对应的富文本颜色。
    private static string GetImpactColorHex(RewardEffectDisplayImpact displayImpact)
    {
        return displayImpact switch
        {
            RewardEffectDisplayImpact.Positive => POSITIVE_COLOR_HEX,
            RewardEffectDisplayImpact.Negative => NEGATIVE_COLOR_HEX,
            _ => NEUTRAL_COLOR_HEX
        };
    }

    // 按定义格式化参数数值。
    private static string FormatValue(float value, RewardEffectValueFormat valueFormat)
    {
        return valueFormat switch
        {
            RewardEffectValueFormat.PercentWithSign => FormatPercent(value, true),
            RewardEffectValueFormat.PercentWithoutSign => FormatPercent(value, false),
            RewardEffectValueFormat.IntegerWithSign => FormatInteger(value, true),
            RewardEffectValueFormat.IntegerWithoutSign => FormatInteger(value, false),
            _ => FormatNumber(value)
        };
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
