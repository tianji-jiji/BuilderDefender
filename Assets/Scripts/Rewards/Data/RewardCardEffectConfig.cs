using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一张卡里的单个效果配置，保存效果定义和这张卡上的参数列表。
/// </summary>
[Serializable]
public class RewardCardEffectConfig
{
    [SerializeField] private RewardEffectDefinitionSo effectDefinition;
    [SerializeField] private List<RewardEffectParameterConfig> parameterConfigList = new();

    public RewardEffectDefinitionSo EffectDefinition => effectDefinition;
    public IReadOnlyList<RewardEffectParameterConfig> ParameterConfigList => parameterConfigList;

    // 判断当前效果是否已经迁移到参数列表配置。
    public bool HasParameterList()
    {
        return parameterConfigList is { Count: > 0 };
    }
}
