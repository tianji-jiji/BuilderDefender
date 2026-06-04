using UnityEngine;

public enum RewardEffectAutoImpactRule
{
    GreaterThanZeroIsPositive,
    LessThanZeroIsPositive,
    AlwaysPositive,
    AlwaysNegative,
    AlwaysNeutral
}

/// <summary>
/// 奖励效果定义资产，负责描述单个奖励效果的显示文案和自动好坏判断规则。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardEffectDefinitionSo")]
public class RewardEffectDefinitionSo : ScriptableObject
{
    private const string DISPLAY_NAME_TOKEN = "{displayName}";
    private const string VALUE_TOKEN = "{value}";
    private const string DEFAULT_DESCRIPTION_TEMPLATE = "{displayName} {value}";

    [SerializeField] private RewardEffectType effectType;
    [SerializeField] private string displayName;
    [SerializeField] private string descriptionTemplate = DEFAULT_DESCRIPTION_TEMPLATE;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;

    public RewardEffectType EffectType => effectType;
    public string DisplayName => displayName;

    // 根据数值文本构建完整效果描述。
    public string BuildDescription(string valueText)
    {
        string template = string.IsNullOrWhiteSpace(descriptionTemplate) ? DEFAULT_DESCRIPTION_TEMPLATE : descriptionTemplate;
        return template
            .Replace(DISPLAY_NAME_TOKEN, displayName)
            .Replace(VALUE_TOKEN, valueText);
    }

    // 根据覆盖设置和自动规则解析显示倾向。
    public RewardEffectDisplayImpact ResolveDisplayImpact(float value, RewardEffectDisplayImpact displayImpactOverride)
    {
        if (displayImpactOverride != RewardEffectDisplayImpact.Auto)
        {
            return displayImpactOverride;
        }

        return ResolveAutoDisplayImpact(value);
    }

    // 根据定义资产中的自动规则判断收益、惩罚或中性。
    private RewardEffectDisplayImpact ResolveAutoDisplayImpact(float value)
    {
        if (Mathf.Approximately(value, 0f))
        {
            return RewardEffectDisplayImpact.Neutral;
        }

        switch (autoImpactRule)
        {
            case RewardEffectAutoImpactRule.GreaterThanZeroIsPositive:
                return value > 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.LessThanZeroIsPositive:
                return value < 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.AlwaysPositive:
                return RewardEffectDisplayImpact.Positive;
            case RewardEffectAutoImpactRule.AlwaysNegative:
                return RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.AlwaysNeutral:
                return RewardEffectDisplayImpact.Neutral;
            default:
                return RewardEffectDisplayImpact.Neutral;
        }
    }
}
