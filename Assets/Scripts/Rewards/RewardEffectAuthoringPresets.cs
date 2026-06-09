using System;
using System.Collections.Generic;

/// <summary>
/// 奖励效果参数显示预设，负责描述某个玩法效果常见的数值配置。
/// </summary>
internal readonly struct RewardEffectParameterDisplayPresetData
{
    public readonly RewardEffectParameterKey parameterKey;
    public readonly RewardEffectValueFormat valueFormat;
    public readonly RewardEffectAutoImpactRule autoImpactRule;
    public readonly string displayNameOverride;

    // 创建参数显示预设。
    public RewardEffectParameterDisplayPresetData(RewardEffectParameterKey parameterKey, RewardEffectValueFormat valueFormat, RewardEffectAutoImpactRule autoImpactRule, string displayNameOverride = "")
    {
        this.parameterKey = parameterKey;
        this.valueFormat = valueFormat;
        this.autoImpactRule = autoImpactRule;
        this.displayNameOverride = displayNameOverride;
    }

    // 获取参数在面板里显示的中文名。
    public string GetDisplayName(string fallbackDisplayName)
    {
        return string.IsNullOrWhiteSpace(displayNameOverride) ? fallbackDisplayName : displayNameOverride;
    }
}

/// <summary>
/// 奖励效果编辑预设，负责把效果类型映射到默认显示名、描述模板和参数显示规则。
/// </summary>
internal readonly struct RewardEffectPreset
{
    public readonly string DisplayName;
    public readonly string DefaultDescriptionTemplate;
    public readonly RewardEffectParameterDisplayPresetData[] ParameterDisplayPresetArray;

    // 创建效果编辑预设。
    public RewardEffectPreset(string displayName, string defaultDescriptionTemplate, params RewardEffectParameterDisplayPresetData[] parameterDisplayPresetArray)
    {
        DisplayName = displayName;
        DefaultDescriptionTemplate = defaultDescriptionTemplate;
        ParameterDisplayPresetArray = parameterDisplayPresetArray ?? Array.Empty<RewardEffectParameterDisplayPresetData>();
    }
}

/// <summary>
/// 奖励效果编辑配置预设，负责提供 Inspector 需要的中文名、默认描述和默认数值作用。
/// </summary>
public static class RewardEffectAuthoringPresets
{
    private const string DEFAULT_EFFECT_DISPLAY_NAME = "奖励效果";
    private const string DEFAULT_PARAMETER_DISPLAY_NAME = "数值";
    private const string DEFAULT_DESCRIPTION_TEMPLATE = "{displayName} {value}";

    private static readonly Dictionary<RewardEffectType, RewardEffectPreset> PresetDic = new()
    {
        {
            RewardEffectType.DefenseAttackDamageMultiplier,
            new RewardEffectPreset(
                "防御塔攻击力",
                DEFAULT_DESCRIPTION_TEMPLATE,
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成"))
        },
        {
            RewardEffectType.DefenseAttackSpeedMultiplier,
            new RewardEffectPreset(
                "防御塔攻击速度",
                DEFAULT_DESCRIPTION_TEMPLATE,
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击速度加成"))
        },
        {
            RewardEffectType.DefenseDetectRadiusMultiplier,
            new RewardEffectPreset(
                "防御塔攻击范围",
                DEFAULT_DESCRIPTION_TEMPLATE,
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击范围加成"))
        },
        {
            RewardEffectType.DefenseMaxHealthMultiplier,
            new RewardEffectPreset(
                "防御塔最大生命",
                DEFAULT_DESCRIPTION_TEMPLATE,
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "生命值加成"))
        },
        {
            RewardEffectType.DefenseBuildCostMultiplier,
            new RewardEffectPreset(
                "防御塔建造成本",
                DEFAULT_DESCRIPTION_TEMPLATE,
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive, "建造成本变化"))
        },
        {
            RewardEffectType.DefenseArmorIgnorePercent,
            new RewardEffectPreset(
                "防御塔护甲穿透",
                "{displayName} {ArmorIgnorePercent}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ArmorIgnorePercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive))
        },
        {
            RewardEffectType.DefenseExtraArrowEveryAttackCount,
            new RewardEffectPreset(
                "每 N 次攻击额外攻击",
                "每攻击 {TriggerAttackCount} 次，额外攻击 {ExtraAttackCount} 次",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.TriggerAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExtraAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive))
        },
        {
            RewardEffectType.DefenseDamageTakenMultiplier,
            new RewardEffectPreset(
                "防御塔受到伤害",
                "{displayName} {DamageTakenMultiplier}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DamageTakenMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive))
        },
        {
            RewardEffectType.DefenseWaveEndHealPercent,
            new RewardEffectPreset(
                "防御塔波末回血",
                "每波结束恢复 {WaveEndHealPercent} 最大生命",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.WaveEndHealPercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive))
        },
        {
            RewardEffectType.DefenseKillCountAutoUpgrade,
            new RewardEffectPreset(
                "击杀自动升星",
                "击杀 {KillCountToUpgrade} 个敌人后自动升星",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.KillCountToUpgrade, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral))
        },
        {
            RewardEffectType.DefenseAttackSpeedOverloadMultiplier,
            new RewardEffectPreset(
                "超载攻速变化",
                "{displayName} {AttackSpeedMultiplier}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive))
        },
        {
            RewardEffectType.DefenseAttackHealthCost,
            new RewardEffectPreset(
                "防御塔每 N 次攻击损失生命",
                "每攻击 {TriggerAttackCount} 次，损失 {AttackHealthCost} 生命",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.TriggerAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackHealthCost, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNegative))
        },
        {
            RewardEffectType.DefenseAttackDamagePerThreeStarTower,
            new RewardEffectPreset(
                "三星塔数量加攻击",
                "每座三星塔使攻击力 {DamageBonusPerThreeStarTower}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DamageBonusPerThreeStarTower, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive))
        },
        {
            RewardEffectType.DefenseDoubleDamageChance,
            new RewardEffectPreset(
                "防御塔双倍伤害概率",
                "攻击时有 {DoubleDamageChance} 概率造成双倍伤害",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DoubleDamageChance, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive))
        },
        {
            RewardEffectType.DefenseLinkedAttackSpeedMultiplier,
            new RewardEffectPreset(
                "防御塔靠近提升攻速",
                "{LinkRadius} 范围内有防御塔时，攻击速度 {AttackSpeedMultiplier}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.LinkRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive))
        },
        {
            RewardEffectType.DefenseRandomTowerMaxStar,
            new RewardEffectPreset("随机防御塔升满星", "随机一座防御塔升到满星")
        },
        {
            RewardEffectType.DefenseNewTowerInitialStarBonus,
            new RewardEffectPreset(
                "新建塔初始星级",
                "新建防御塔初始星级 {InitialStarBonus}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.InitialStarBonus, RewardEffectValueFormat.IntegerWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive))
        },
        {
            RewardEffectType.DefenseFinalDefenseAttackDamageMultiplier,
            new RewardEffectPreset(
                "基地低血量加攻击",
                "基地生命低于 {HomeHealthThreshold} 时，防御塔攻击力 {value}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.HomeHealthThreshold, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成"))
        },
        {
            RewardEffectType.DefenseThreeStarExplosiveArrow,
            new RewardEffectPreset(
                "三星塔爆裂箭",
                "三星塔攻击造成 {ExplosionRadius} 范围爆炸，伤害倍率 {ExplosionDamageMultiplier}",
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExplosionRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
                new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExplosionDamageMultiplier, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive))
        }
    };

    private static readonly Lazy<string[]> EffectTypeDisplayNameArrayLazy = new(BuildEffectTypeDisplayNameArray);
    private static readonly Lazy<string[]> ParameterKeyDisplayNameArrayLazy = new(BuildParameterKeyDisplayNameArray);
    private static readonly Lazy<string[]> ValueFormatDisplayNameArrayLazy = new(BuildValueFormatDisplayNameArray);
    private static readonly Lazy<string[]> AutoImpactRuleDisplayNameArrayLazy = new(BuildAutoImpactRuleDisplayNameArray);

    public static string[] EffectTypeDisplayNameArray => EffectTypeDisplayNameArrayLazy.Value;
    public static string[] ParameterKeyDisplayNameArray => ParameterKeyDisplayNameArrayLazy.Value;
    public static string[] ValueFormatDisplayNameArray => ValueFormatDisplayNameArrayLazy.Value;
    public static string[] AutoImpactRuleDisplayNameArray => AutoImpactRuleDisplayNameArrayLazy.Value;

    // 根据玩法效果索引获取中文效果名。
    public static string GetEffectTypeDisplayName(int effectTypeIndex)
    {
        return TryGetEffectTypeByIndex(effectTypeIndex, out RewardEffectType effectType)
            ? GetEffectTypeDisplayName(effectType)
            : DEFAULT_EFFECT_DISPLAY_NAME;
    }

    // 根据玩法效果获取中文效果名。
    public static string GetEffectTypeDisplayName(RewardEffectType effectType)
    {
        return PresetDic.TryGetValue(effectType, out RewardEffectPreset preset)
            ? preset.DisplayName
            : DEFAULT_EFFECT_DISPLAY_NAME;
    }

    // 根据参数枚举索引获取中文显示名。
    public static string GetParameterDisplayName(int parameterKeyIndex)
    {
        return TryGetParameterKeyByIndex(parameterKeyIndex, out RewardEffectParameterKey parameterKey)
            ? GetParameterDisplayName(parameterKey)
            : DEFAULT_PARAMETER_DISPLAY_NAME;
    }

    // 根据参数枚举获取中文显示名。
    public static string GetParameterDisplayName(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.Value:
                return "主数值";
            case RewardEffectParameterKey.TriggerAttackCount:
                return "触发攻击次数";
            case RewardEffectParameterKey.ExtraAttackCount:
                return "额外攻击数量";
            case RewardEffectParameterKey.ArmorIgnorePercent:
                return "护甲穿透百分比";
            case RewardEffectParameterKey.DamageTakenMultiplier:
                return "受到伤害变化";
            case RewardEffectParameterKey.WaveEndHealPercent:
                return "波末回血百分比";
            case RewardEffectParameterKey.KillCountToUpgrade:
                return "升星所需击杀数";
            case RewardEffectParameterKey.AttackSpeedMultiplier:
                return "攻击速度变化";
            case RewardEffectParameterKey.AttackHealthCost:
                return "攻击损失生命";
            case RewardEffectParameterKey.DamageBonusPerThreeStarTower:
                return "每座三星塔攻击加成";
            case RewardEffectParameterKey.DoubleDamageChance:
                return "双倍伤害概率";
            case RewardEffectParameterKey.LinkRadius:
                return "联动距离";
            case RewardEffectParameterKey.InitialStarBonus:
                return "初始星级加成";
            case RewardEffectParameterKey.HomeHealthThreshold:
                return "基地生命阈值";
            case RewardEffectParameterKey.ExplosionRadius:
                return "爆炸半径";
            case RewardEffectParameterKey.ExplosionDamageMultiplier:
                return "爆炸伤害倍率";
            default:
                return DEFAULT_PARAMETER_DISPLAY_NAME;
        }
    }

    // 根据玩法效果索引获取自动描述模板。
    public static string GetDefaultDescriptionTemplate(int effectTypeIndex)
    {
        return TryGetEffectTypeByIndex(effectTypeIndex, out RewardEffectType effectType)
            ? GetDefaultDescriptionTemplate(effectType)
            : DEFAULT_DESCRIPTION_TEMPLATE;
    }

    // 根据玩法效果获取自动描述模板。
    public static string GetDefaultDescriptionTemplate(RewardEffectType effectType)
    {
        return PresetDic.TryGetValue(effectType, out RewardEffectPreset preset)
            ? preset.DefaultDescriptionTemplate
            : DEFAULT_DESCRIPTION_TEMPLATE;
    }

    // 获取当前玩法效果默认需要显示几个数值。
    public static int GetDefaultParameterDisplayPresetCount(int effectTypeIndex)
    {
        return GetDefaultParameterDisplayPresetArray(effectTypeIndex).Length;
    }

    // 获取当前玩法效果第几个默认数值的实际作用。
    public static RewardEffectParameterKey GetDefaultParameterKey(int effectTypeIndex, int parameterIndex)
    {
        RewardEffectParameterDisplayPresetData[] parameterDisplayPresetArray = GetDefaultParameterDisplayPresetArray(effectTypeIndex);
        if (parameterIndex >= 0 && parameterIndex < parameterDisplayPresetArray.Length)
        {
            return parameterDisplayPresetArray[parameterIndex].parameterKey;
        }

        return RewardEffectParameterKey.Value;
    }

    // 获取当前玩法效果第几个默认数值的显示名。
    public static string GetDefaultParameterDisplayName(int effectTypeIndex, int parameterIndex)
    {
        RewardEffectParameterKey parameterKey = GetDefaultParameterKey(effectTypeIndex, parameterIndex);
        RewardEffectParameterDisplayPresetData[] parameterDisplayPresetArray = GetDefaultParameterDisplayPresetArray(effectTypeIndex);
        if (parameterIndex >= 0 && parameterIndex < parameterDisplayPresetArray.Length)
        {
            return parameterDisplayPresetArray[parameterIndex].GetDisplayName(GetParameterDisplayName(parameterKey));
        }

        return GetParameterDisplayName(parameterKey);
    }

    // 获取当前玩法效果第几个默认数值的显示格式。
    public static RewardEffectValueFormat GetDefaultParameterValueFormat(int effectTypeIndex, int parameterIndex)
    {
        RewardEffectParameterDisplayPresetData[] parameterDisplayPresetArray = GetDefaultParameterDisplayPresetArray(effectTypeIndex);
        if (parameterIndex >= 0 && parameterIndex < parameterDisplayPresetArray.Length)
        {
            return parameterDisplayPresetArray[parameterIndex].valueFormat;
        }

        return RewardEffectValueFormat.NumberOnly;
    }

    // 获取当前玩法效果第几个默认数值的颜色规则。
    public static RewardEffectAutoImpactRule GetDefaultParameterAutoImpactRule(int effectTypeIndex, int parameterIndex)
    {
        RewardEffectParameterDisplayPresetData[] parameterDisplayPresetArray = GetDefaultParameterDisplayPresetArray(effectTypeIndex);
        if (parameterIndex >= 0 && parameterIndex < parameterDisplayPresetArray.Length)
        {
            return parameterDisplayPresetArray[parameterIndex].autoImpactRule;
        }

        return RewardEffectAutoImpactRule.AlwaysNeutral;
    }

    // 根据参数类型给新增数值选择默认显示格式。
    public static RewardEffectValueFormat GetDefaultValueFormat(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.Value:
            case RewardEffectParameterKey.DamageTakenMultiplier:
            case RewardEffectParameterKey.AttackSpeedMultiplier:
            case RewardEffectParameterKey.DamageBonusPerThreeStarTower:
                return RewardEffectValueFormat.PercentWithSign;

            case RewardEffectParameterKey.ArmorIgnorePercent:
            case RewardEffectParameterKey.WaveEndHealPercent:
            case RewardEffectParameterKey.DoubleDamageChance:
            case RewardEffectParameterKey.HomeHealthThreshold:
            case RewardEffectParameterKey.ExplosionDamageMultiplier:
                return RewardEffectValueFormat.PercentWithoutSign;

            case RewardEffectParameterKey.TriggerAttackCount:
            case RewardEffectParameterKey.ExtraAttackCount:
            case RewardEffectParameterKey.KillCountToUpgrade:
            case RewardEffectParameterKey.AttackHealthCost:
                return RewardEffectValueFormat.IntegerWithoutSign;

            case RewardEffectParameterKey.InitialStarBonus:
                return RewardEffectValueFormat.IntegerWithSign;

            default:
                return RewardEffectValueFormat.NumberOnly;
        }
    }

    // 根据参数类型给新增数值选择默认颜色规则。
    public static RewardEffectAutoImpactRule GetDefaultAutoImpactRule(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.DamageTakenMultiplier:
                return RewardEffectAutoImpactRule.LessThanZeroIsPositive;

            case RewardEffectParameterKey.AttackHealthCost:
                return RewardEffectAutoImpactRule.AlwaysNegative;

            case RewardEffectParameterKey.TriggerAttackCount:
            case RewardEffectParameterKey.LinkRadius:
            case RewardEffectParameterKey.HomeHealthThreshold:
            case RewardEffectParameterKey.ExplosionRadius:
                return RewardEffectAutoImpactRule.AlwaysNeutral;

            default:
                return RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;
        }
    }

    // 获取当前玩法效果的默认数值预设。
    private static RewardEffectParameterDisplayPresetData[] GetDefaultParameterDisplayPresetArray(int effectTypeIndex)
    {
        return TryGetEffectTypeByIndex(effectTypeIndex, out RewardEffectType effectType)
            ? GetDefaultParameterDisplayPresetArray(effectType)
            : Array.Empty<RewardEffectParameterDisplayPresetData>();
    }

    // 获取当前玩法效果的默认数值预设。
    private static RewardEffectParameterDisplayPresetData[] GetDefaultParameterDisplayPresetArray(RewardEffectType effectType)
    {
        return PresetDic.TryGetValue(effectType, out RewardEffectPreset preset)
            ? preset.ParameterDisplayPresetArray
            : Array.Empty<RewardEffectParameterDisplayPresetData>();
    }

    // 构建玩法效果下拉框中文名。
    private static string[] BuildEffectTypeDisplayNameArray()
    {
        Array effectTypeArray = Enum.GetValues(typeof(RewardEffectType));
        string[] displayNameArray = new string[effectTypeArray.Length];
        for (int i = 0; i < effectTypeArray.Length; i++)
        {
            displayNameArray[i] = GetEffectTypeDisplayName((RewardEffectType)effectTypeArray.GetValue(i));
        }

        return displayNameArray;
    }

    // 构建参数类型下拉框中文名。
    private static string[] BuildParameterKeyDisplayNameArray()
    {
        Array parameterKeyArray = Enum.GetValues(typeof(RewardEffectParameterKey));
        string[] displayNameArray = new string[parameterKeyArray.Length];
        for (int i = 0; i < parameterKeyArray.Length; i++)
        {
            displayNameArray[i] = GetParameterDisplayName((RewardEffectParameterKey)parameterKeyArray.GetValue(i));
        }

        return displayNameArray;
    }

    // 构建数值显示格式下拉框中文名。
    private static string[] BuildValueFormatDisplayNameArray()
    {
        Array valueFormatArray = Enum.GetValues(typeof(RewardEffectValueFormat));
        string[] displayNameArray = new string[valueFormatArray.Length];
        for (int i = 0; i < valueFormatArray.Length; i++)
        {
            displayNameArray[i] = GetValueFormatDisplayName((RewardEffectValueFormat)valueFormatArray.GetValue(i));
        }

        return displayNameArray;
    }

    // 构建颜色规则下拉框中文名。
    private static string[] BuildAutoImpactRuleDisplayNameArray()
    {
        Array autoImpactRuleArray = Enum.GetValues(typeof(RewardEffectAutoImpactRule));
        string[] displayNameArray = new string[autoImpactRuleArray.Length];
        for (int i = 0; i < autoImpactRuleArray.Length; i++)
        {
            displayNameArray[i] = GetAutoImpactRuleDisplayName((RewardEffectAutoImpactRule)autoImpactRuleArray.GetValue(i));
        }

        return displayNameArray;
    }

    // 获取数值显示格式中文名。
    private static string GetValueFormatDisplayName(RewardEffectValueFormat valueFormat)
    {
        switch (valueFormat)
        {
            case RewardEffectValueFormat.PercentWithSign:
                return "百分比有符号，比如 +10%";
            case RewardEffectValueFormat.PercentWithoutSign:
                return "百分比无符号，比如 10%";
            case RewardEffectValueFormat.IntegerWithSign:
                return "整数有符号，比如 +1";
            case RewardEffectValueFormat.IntegerWithoutSign:
                return "整数无符号，比如 5";
            case RewardEffectValueFormat.NumberOnly:
                return "普通数字，比如 2.5";
            default:
                return "普通数字";
        }
    }

    // 获取颜色规则中文名。
    private static string GetAutoImpactRuleDisplayName(RewardEffectAutoImpactRule autoImpactRule)
    {
        switch (autoImpactRule)
        {
            case RewardEffectAutoImpactRule.GreaterThanZeroIsPositive:
                return "大于 0 显示绿色，小于 0 显示红色";
            case RewardEffectAutoImpactRule.LessThanZeroIsPositive:
                return "小于 0 显示绿色，大于 0 显示红色";
            case RewardEffectAutoImpactRule.AlwaysPositive:
                return "绿色";
            case RewardEffectAutoImpactRule.AlwaysNegative:
                return "红色";
            case RewardEffectAutoImpactRule.AlwaysNeutral:
                return "白色";
            default:
                return "白色";
        }
    }

    // 根据序列化枚举索引读取玩法效果。
    private static bool TryGetEffectTypeByIndex(int effectTypeIndex, out RewardEffectType effectType)
    {
        Array effectTypeArray = Enum.GetValues(typeof(RewardEffectType));
        if (effectTypeIndex >= 0 && effectTypeIndex < effectTypeArray.Length)
        {
            effectType = (RewardEffectType)effectTypeArray.GetValue(effectTypeIndex);
            return true;
        }

        effectType = default;
        return false;
    }

    // 根据序列化枚举索引读取参数类型。
    private static bool TryGetParameterKeyByIndex(int parameterKeyIndex, out RewardEffectParameterKey parameterKey)
    {
        Array parameterKeyArray = Enum.GetValues(typeof(RewardEffectParameterKey));
        if (parameterKeyIndex >= 0 && parameterKeyIndex < parameterKeyArray.Length)
        {
            parameterKey = (RewardEffectParameterKey)parameterKeyArray.GetValue(parameterKeyIndex);
            return true;
        }

        parameterKey = default;
        return false;
    }
}
