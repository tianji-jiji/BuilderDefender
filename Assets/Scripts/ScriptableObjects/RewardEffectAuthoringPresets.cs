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
/// 奖励效果编辑配置预设，负责提供 Inspector 需要的中文名、默认描述和默认数值作用。
/// </summary>
public static class RewardEffectAuthoringPresets
{
    public static readonly string[] EffectTypeDisplayNameArray =
    {
        "防御塔攻击力",
        "防御塔攻击速度",
        "防御塔攻击范围",
        "防御塔最大生命",
        "防御塔建造成本",
        "防御塔护甲穿透",
        "每 N 次攻击额外攻击",
        "防御塔受到伤害",
        "防御塔波末回血",
        "击杀自动升星",
        "超载攻速变化",
        "防御塔每 N 次攻击损失生命",
        "三星塔数量加攻击",
        "防御塔双倍伤害概率",
        "防御塔靠近提升攻速",
        "随机防御塔升满星",
        "新建塔初始星级",
        "基地低血量加攻击",
        "三星塔爆裂箭"
    };

    public static readonly string[] ParameterKeyDisplayNameArray =
    {
        "主数值",
        "触发攻击次数",
        "额外攻击数量",
        "护甲穿透百分比",
        "受到伤害变化",
        "波末回血百分比",
        "升星所需击杀数",
        "攻击速度变化",
        "攻击损失生命",
        "每座三星塔攻击加成",
        "双倍伤害概率",
        "联动距离",
        "初始星级加成",
        "基地生命阈值",
        "爆炸半径",
        "爆炸伤害倍率"
    };

    public static readonly string[] ValueFormatDisplayNameArray =
    {
        "百分比有符号，比如 +10%",
        "百分比无符号，比如 10%",
        "整数有符号，比如 +1",
        "整数无符号，比如 5",
        "普通数字，比如 2.5"
    };

    public static readonly string[] AutoImpactRuleDisplayNameArray =
    {
        "大于 0 显示绿色，小于 0 显示红色",
        "小于 0 显示绿色，大于 0 显示红色",
        "绿色",
        "红色",
        "白色"
    };

    private static readonly RewardEffectParameterDisplayPresetData[][] DefaultParameterDisplayPresetArray =
    {
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成") },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击速度加成") },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击范围加成") },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "生命值加成") },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive, "建造成本变化") },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ArmorIgnorePercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[]
        {
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.TriggerAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExtraAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive)
        },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DamageTakenMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive) },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.WaveEndHealPercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.KillCountToUpgrade, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral) },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[]
        {
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.TriggerAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackHealthCost, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNegative)
        },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DamageBonusPerThreeStarTower, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.DoubleDamageChance, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[]
        {
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.LinkRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive)
        },
        new RewardEffectParameterDisplayPresetData[] { },
        new[] { new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.InitialStarBonus, RewardEffectValueFormat.IntegerWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[]
        {
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.HomeHealthThreshold, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成")
        },
        new[]
        {
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExplosionRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
            new RewardEffectParameterDisplayPresetData(RewardEffectParameterKey.ExplosionDamageMultiplier, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive)
        }
    };

    private static readonly string[] DefaultDescriptionTemplateArray =
    {
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {ArmorIgnorePercent}",
        "每攻击 {TriggerAttackCount} 次，额外攻击 {ExtraAttackCount} 次",
        "{displayName} {DamageTakenMultiplier}",
        "每波结束恢复 {WaveEndHealPercent} 最大生命",
        "击杀 {KillCountToUpgrade} 个敌人后自动升星",
        "{displayName} {AttackSpeedMultiplier}",
        "每攻击 {TriggerAttackCount} 次，损失 {AttackHealthCost} 生命",
        "每座三星塔使攻击力 {DamageBonusPerThreeStarTower}",
        "攻击时有 {DoubleDamageChance} 概率造成双倍伤害",
        "{LinkRadius} 范围内有防御塔时，攻击速度 {AttackSpeedMultiplier}",
        "随机一座防御塔升到满星",
        "新建防御塔初始星级 {InitialStarBonus}",
        "基地生命低于 {HomeHealthThreshold} 时，防御塔攻击力 {value}",
        "三星塔攻击造成 {ExplosionRadius} 范围爆炸，伤害倍率 {ExplosionDamageMultiplier}"
    };

    // 根据玩法效果索引获取中文效果名。
    public static string GetEffectTypeDisplayName(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < EffectTypeDisplayNameArray.Length)
        {
            return EffectTypeDisplayNameArray[effectTypeIndex];
        }

        return "奖励效果";
    }

    // 根据参数枚举索引获取中文显示名。
    public static string GetParameterDisplayName(int parameterKeyIndex)
    {
        if (parameterKeyIndex >= 0 && parameterKeyIndex < ParameterKeyDisplayNameArray.Length)
        {
            return ParameterKeyDisplayNameArray[parameterKeyIndex];
        }

        return "数值";
    }

    // 根据玩法效果索引获取自动描述模板。
    public static string GetDefaultDescriptionTemplate(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < DefaultDescriptionTemplateArray.Length)
        {
            return DefaultDescriptionTemplateArray[effectTypeIndex];
        }

        return "{displayName} {value}";
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
            return parameterDisplayPresetArray[parameterIndex].GetDisplayName(GetParameterDisplayName((int)parameterKey));
        }

        return GetParameterDisplayName((int)parameterKey);
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

    // 获取当前玩法效果的默认数值预设数组。
    private static RewardEffectParameterDisplayPresetData[] GetDefaultParameterDisplayPresetArray(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < DefaultParameterDisplayPresetArray.Length)
        {
            return DefaultParameterDisplayPresetArray[effectTypeIndex];
        }

        return DefaultParameterDisplayPresetArray[0];
    }
}
