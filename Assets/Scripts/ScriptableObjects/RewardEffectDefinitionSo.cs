using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardEffectAutoImpactRule
{
    GreaterThanZeroIsPositive,
    LessThanZeroIsPositive,
    AlwaysPositive,
    AlwaysNegative,
    AlwaysNeutral
}

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

    [SerializeField] private RewardEffectParameterKey parameterKey = RewardEffectParameterKey.Value;
    [SerializeField] private string displayName;
    [SerializeField] private string templateToken;
    [SerializeField] private RewardEffectValueFormat valueFormat = RewardEffectValueFormat.PercentWithSign;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;

    public RewardEffectParameterKey ParameterKey => parameterKey;
    public string DisplayName => displayName;
    public string TemplateToken => string.IsNullOrWhiteSpace(templateToken) ? string.Format(TOKEN_FORMAT, parameterKey) : templateToken;
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
/// 奖励效果定义资产，负责描述单个奖励效果的显示文案和参数显示规则。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardEffectDefinitionSo")]
public class RewardEffectDefinitionSo : ScriptableObject
{
    private const string DISPLAY_NAME_TOKEN = "{displayName}";
    private const string VALUE_TOKEN = "{value}";
    private const string DEFAULT_DESCRIPTION_TEMPLATE = "{displayName} {value}";

    [SerializeField] private RewardEffectType effectType;
    [SerializeField] private string displayName;
    [SerializeField] private bool useCustomDescription;
    [SerializeField] private string descriptionTemplate = DEFAULT_DESCRIPTION_TEMPLATE;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;
    [SerializeField] private List<RewardEffectParameterDisplayDefinition> parameterDisplayDefinitionList = new List<RewardEffectParameterDisplayDefinition>();

    public RewardEffectType EffectType => effectType;
    public string DisplayName => displayName;
    public bool UseCustomDescription => useCustomDescription;
    public IReadOnlyList<RewardEffectParameterDisplayDefinition> ParameterDisplayDefinitionList => parameterDisplayDefinitionList;

    // 根据参数富文本构建完整效果描述。
    public string BuildDescription(IReadOnlyDictionary<RewardEffectParameterKey, string> parameterTextDic)
    {
        string template = string.IsNullOrWhiteSpace(descriptionTemplate) ? DEFAULT_DESCRIPTION_TEMPLATE : descriptionTemplate;
        string description = template.Replace(DISPLAY_NAME_TOKEN, displayName);

        if (parameterTextDic == null || parameterTextDic.Count <= 0)
        {
            return description;
        }

        foreach (KeyValuePair<RewardEffectParameterKey, string> parameterTextPair in parameterTextDic)
        {
            RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterTextPair.Key);
            string token = displayDefinition != null ? displayDefinition.TemplateToken : GetDefaultToken(parameterTextPair.Key);
            description = description.Replace(token, parameterTextPair.Value);

            if (parameterTextPair.Key == RewardEffectParameterKey.Value)
            {
                description = description.Replace(VALUE_TOKEN, parameterTextPair.Value);
            }
        }

        return description;
    }

    // 获取指定参数的显示格式。
    public RewardEffectValueFormat GetValueFormat(RewardEffectParameterKey parameterKey)
    {
        RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterKey);
        return displayDefinition != null ? displayDefinition.ValueFormat : RewardEffectValueFormat.PercentWithSign;
    }

    // 根据参数定义解析显示倾向。
    public RewardEffectDisplayImpact ResolveDisplayImpact(RewardEffectParameterKey parameterKey, float value, RewardEffectDisplayImpact displayImpactOverride)
    {
        RewardEffectParameterDisplayDefinition displayDefinition = GetParameterDisplayDefinition(parameterKey);
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
    private RewardEffectParameterDisplayDefinition GetParameterDisplayDefinition(RewardEffectParameterKey parameterKey)
    {
        if (parameterDisplayDefinitionList == null)
        {
            return null;
        }

        foreach (RewardEffectParameterDisplayDefinition displayDefinition in parameterDisplayDefinitionList)
        {
            if (displayDefinition != null && displayDefinition.ParameterKey == parameterKey)
            {
                return displayDefinition;
            }
        }

        return null;
    }

    // 根据旧版自动规则解析兜底显示倾向。
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
    private string GetDefaultToken(RewardEffectParameterKey parameterKey)
    {
        return $"{{{parameterKey}}}";
    }
}
