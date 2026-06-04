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

        StringBuilder descriptionBuilder = new StringBuilder();
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
        string valueText = BuildColoredValueText(effectConfig);
        return effectConfig.BuildDescription(valueText);
    }

    // 构建带颜色的奖励数值文本。
    private static string BuildColoredValueText(RewardEffectConfig effectConfig)
    {
        string valueText = FormatPercent(effectConfig.Value);
        string colorHex = GetImpactColorHex(effectConfig.ResolveDisplayImpact());
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

    // 把倍率增量格式化成百分比文本。
    private static string FormatPercent(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        return percent > 0 ? $"+{percent}%" : $"{percent}%";
    }
}
