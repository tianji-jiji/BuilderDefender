using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励效果参数的自动显示倾向规则。
/// </summary>
public enum RewardEffectAutoImpactRule
{
    GreaterThanZeroIsPositive,
    LessThanZeroIsPositive,
    AlwaysPositive,
    AlwaysNegative,
    AlwaysNeutral
}

/// <summary>
/// 奖励效果参数的数值显示格式。
/// </summary>
public enum RewardEffectValueFormat
{
    PercentWithSign,
    PercentWithoutSign,
    IntegerWithSign,
    IntegerWithoutSign,
    NumberOnly
}

/// <summary>
/// 奖励效果参数显示定义，负责描述单个参数在卡牌文案中的替换规则。
/// </summary>
[Serializable]
public class RewardEffectParameterDisplayDefinition
{
    private const string TOKEN_FORMAT = "{{{0}}}";

    [SerializeField] private string parameterId;
    [SerializeField] private string displayName;
    [SerializeField] private string templateToken;
    [SerializeField] private RewardEffectValueFormat valueFormat = RewardEffectValueFormat.PercentWithSign;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;

    public string ParameterId => string.IsNullOrWhiteSpace(parameterId) ? RewardEffectParameterIds.VALUE : parameterId.Trim();
    public string TemplateToken => string.IsNullOrWhiteSpace(templateToken) ? string.Format(TOKEN_FORMAT, ParameterId) : templateToken;
    public RewardEffectValueFormat ValueFormat => valueFormat;

    // 根据覆盖设置和自动规则解析参数显示倾向。
    public RewardEffectDisplayImpact ResolveDisplayImpact(float value, RewardEffectDisplayImpact displayImpactOverride)
    {
        if (displayImpactOverride != RewardEffectDisplayImpact.Auto)
        {
            return displayImpactOverride;
        }

        return ResolveAutoDisplayImpact(value);
    }

    // 根据参数定义中的自动规则判断收益、惩罚或中性。
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

/// <summary>
/// 奖励效果定义资产，负责描述单个奖励效果的显示文案、参数显示规则和执行 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardEffectDefinitionSo")]
public class RewardEffectDefinitionSo : ScriptableObject
{
    private const string DISPLAY_NAME_TOKEN = "{displayName}";
    private const string VALUE_TOKEN = "{value}";
    private const string DEFAULT_DESCRIPTION_TEMPLATE = "{displayName} {value}";

    [SerializeField] private string displayName;
    [SerializeField] private string descriptionTemplate = DEFAULT_DESCRIPTION_TEMPLATE;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;
    [SerializeField] private RewardEffectApplierSo applier;
    [SerializeField] private List<RewardEffectParameterDisplayDefinition> parameterDisplayDefinitionList = new();

    public string DisplayName => displayName;
    public RewardEffectApplierSo Applier => applier;

    // 根据参数富文本构建完整效果描述。
    public string BuildDescription(IReadOnlyDictionary<string, string> parameterTextDic)
    {
        string template = string.IsNullOrWhiteSpace(descriptionTemplate) ? DEFAULT_DESCRIPTION_TEMPLATE : descriptionTemplate;
        string description = template.Replace(DISPLAY_NAME_TOKEN, displayName);

        if (parameterTextDic == null || parameterTextDic.Count <= 0)
        {
            return description;
        }

        foreach (KeyValuePair<string, string> parameterTextPair in parameterTextDic)
        {
            RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterTextPair.Key);
            string token = displayDefinition != null ? displayDefinition.TemplateToken : GetDefaultToken(parameterTextPair.Key);
            description = description.Replace(token, parameterTextPair.Value);

            if (parameterTextPair.Key == RewardEffectParameterIds.VALUE)
            {
                description = description.Replace(VALUE_TOKEN, parameterTextPair.Value);
            }
        }

        return description;
    }

    // 获取指定参数的显示格式。
    public RewardEffectValueFormat GetValueFormat(string parameterId)
    {
        RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterId);
        return displayDefinition != null ? displayDefinition.ValueFormat : RewardEffectValueFormat.PercentWithSign;
    }

    // 根据参数定义解析显示倾向。
    public RewardEffectDisplayImpact ResolveDisplayImpact(string parameterId, float value, RewardEffectDisplayImpact displayImpactOverride)
    {
        RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterId);
        if (displayDefinition != null)
        {
            return displayDefinition.ResolveDisplayImpact(value, displayImpactOverride);
        }

        if (displayImpactOverride != RewardEffectDisplayImpact.Auto)
        {
            return displayImpactOverride;
        }

        return ResolveFallbackDisplayImpact(value);
    }

    // 查找指定参数的显示定义。
    private RewardEffectParameterDisplayDefinition GetParameterDisplayDefinition(string parameterId)
    {
        if (parameterDisplayDefinitionList == null)
        {
            return null;
        }

        foreach (RewardEffectParameterDisplayDefinition displayDefinition in parameterDisplayDefinitionList)
        {
            if (displayDefinition != null && string.Equals(displayDefinition.ParameterId, parameterId, StringComparison.Ordinal))
            {
                return displayDefinition;
            }
        }

        return null;
    }

    // 根据默认自动规则解析兜底显示倾向。
    private RewardEffectDisplayImpact ResolveFallbackDisplayImpact(float value)
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

    // 获取参数默认模板 token。
    private string GetDefaultToken(string parameterId)
    {
        return $"{{{parameterId}}}";
    }
}
