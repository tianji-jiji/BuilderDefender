using System;
using UnityEngine;

/// <summary>
/// 奖励效果参数显示定义，保存参数在卡牌文案中的替换配置。
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

    public string ParameterId => string.IsNullOrWhiteSpace(parameterId)
        ? RewardEffectParameterIds.VALUE
        : parameterId.Trim();
    public string TemplateToken => string.IsNullOrWhiteSpace(templateToken)
        ? string.Format(TOKEN_FORMAT, ParameterId)
        : templateToken;
    public RewardEffectValueFormat ValueFormat => valueFormat;
    public RewardEffectAutoImpactRule AutoImpactRule => autoImpactRule;
}
