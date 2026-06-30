using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励效果定义资产，保存显示模板、参数显示配置和执行策略引用。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardEffectDefinitionSo")]
public class RewardEffectDefinitionSo : ScriptableObject
{
    private const string DEFAULT_DESCRIPTION_TEMPLATE = "{displayName} {value}";

    [SerializeField] private string displayName;
    [SerializeField] private string descriptionTemplate = DEFAULT_DESCRIPTION_TEMPLATE;
    [SerializeField] private RewardEffectAutoImpactRule autoImpactRule = RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;
    [SerializeField] private RewardEffectApplierSo applier;
    [SerializeField] private List<RewardEffectParameterDisplayDefinition> parameterDisplayDefinitionList = new();

    public string DisplayName => displayName;
    public string DescriptionTemplate => string.IsNullOrWhiteSpace(descriptionTemplate)
        ? DEFAULT_DESCRIPTION_TEMPLATE
        : descriptionTemplate;
    public RewardEffectAutoImpactRule AutoImpactRule => autoImpactRule;
    public RewardEffectApplierSo Applier => applier;

    // 查找指定参数的显示定义。
    public RewardEffectParameterDisplayDefinition GetParameterDisplayDefinition(string parameterId)
    {
        if (parameterDisplayDefinitionList == null)
        {
            return null;
        }

        foreach (RewardEffectParameterDisplayDefinition definition in parameterDisplayDefinitionList)
        {
            if (definition != null
                && string.Equals(definition.ParameterId, parameterId, StringComparison.Ordinal))
            {
                return definition;
            }
        }

        return null;
    }
}
