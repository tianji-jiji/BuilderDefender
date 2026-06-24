using System.Collections.Generic;

/// <summary>
/// 奖励效果编辑器预设，提供参数 ID 的默认显示和校验规则。
/// </summary>
public static class RewardEffectAuthoringPresets
{
    public static readonly string[] ValueFormatDisplayNameArray =
    {
        "百分比（带正号）",
        "百分比",
        "整数（带正号）",
        "整数",
        "普通数字"
    };

    public static readonly string[] AutoImpactRuleDisplayNameArray =
    {
        "大于 0 为正面",
        "小于 0 为正面",
        "总是正面",
        "总是负面",
        "总是中性"
    };

    private static readonly Dictionary<string, string> ParameterDisplayNameDic = new()
    {
        { RewardEffectParameterIds.VALUE, "数值" },
        { RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, "触发攻击次数" },
        { RewardEffectParameterIds.EXTRA_ATTACK_COUNT, "额外攻击次数" },
        { RewardEffectParameterIds.ARMOR_IGNORE_PERCENT, "护甲穿透" },
        { RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER, "受到伤害变化" },
        { RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, "回合结束治疗" },
        { RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, "升级击杀数" },
        { RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, "攻击速度加成" },
        { RewardEffectParameterIds.ATTACK_HEALTH_COST, "攻击生命消耗" },
        { RewardEffectParameterIds.DAMAGE_BONUS_PER_THREE_STAR_TOWER, "三星塔攻击加成" },
        { RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE, "双倍伤害概率" },
        { RewardEffectParameterIds.LINK_RADIUS, "联动半径" },
        { RewardEffectParameterIds.INITIAL_STAR_BONUS, "初始星级加成" },
        { RewardEffectParameterIds.HOME_HEALTH_THRESHOLD, "基地血量阈值" },
        { RewardEffectParameterIds.EXPLOSION_RADIUS, "爆炸半径" },
        { RewardEffectParameterIds.EXPLOSION_DAMAGE_MULTIPLIER, "爆炸伤害倍率" },
        { RewardEffectParameterIds.TRIGGER_CHANCE, "触发概率" },
        { RewardEffectParameterIds.STATUS_DURATION, "状态持续时间" },
        { RewardEffectParameterIds.TICK_INTERVAL, "跳伤间隔" },
        { RewardEffectParameterIds.TICK_DAMAGE, "跳伤百分比" },
        { RewardEffectParameterIds.EXPLOSION_DAMAGE, "爆炸伤害" },
        { RewardEffectParameterIds.PIERCE_COUNT, "穿透数量" }
    };

    // 获取参数 ID 的中文显示名。
    public static string GetParameterDisplayName(string parameterId)
    {
        string normalizedParameterId = NormalizeParameterId(parameterId);
        return ParameterDisplayNameDic.TryGetValue(normalizedParameterId, out string displayName)
            ? displayName
            : normalizedParameterId;
    }

    // 获取参数 ID 的默认数值显示格式。
    public static RewardEffectValueFormat GetDefaultValueFormat(string parameterId)
    {
        switch (NormalizeParameterId(parameterId))
        {
            case RewardEffectParameterIds.VALUE:
            case RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER:
            case RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER:
            case RewardEffectParameterIds.DAMAGE_BONUS_PER_THREE_STAR_TOWER:
                return RewardEffectValueFormat.PercentWithSign;
            case RewardEffectParameterIds.ARMOR_IGNORE_PERCENT:
            case RewardEffectParameterIds.WAVE_END_HEAL_PERCENT:
            case RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE:
            case RewardEffectParameterIds.HOME_HEALTH_THRESHOLD:
            case RewardEffectParameterIds.EXPLOSION_DAMAGE_MULTIPLIER:
            case RewardEffectParameterIds.TRIGGER_CHANCE:
            case RewardEffectParameterIds.TICK_DAMAGE:
                return RewardEffectValueFormat.PercentWithoutSign;
            case RewardEffectParameterIds.TRIGGER_ATTACK_COUNT:
            case RewardEffectParameterIds.EXTRA_ATTACK_COUNT:
            case RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE:
            case RewardEffectParameterIds.ATTACK_HEALTH_COST:
            case RewardEffectParameterIds.EXPLOSION_DAMAGE:
            case RewardEffectParameterIds.PIERCE_COUNT:
                return RewardEffectValueFormat.IntegerWithoutSign;
            case RewardEffectParameterIds.INITIAL_STAR_BONUS:
                return RewardEffectValueFormat.IntegerWithSign;
            default:
                return RewardEffectValueFormat.NumberOnly;
        }
    }

    // 获取参数 ID 的默认显示倾向规则。
    public static RewardEffectAutoImpactRule GetDefaultAutoImpactRule(string parameterId)
    {
        switch (NormalizeParameterId(parameterId))
        {
            case RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER:
                return RewardEffectAutoImpactRule.LessThanZeroIsPositive;
            case RewardEffectParameterIds.ATTACK_HEALTH_COST:
                return RewardEffectAutoImpactRule.AlwaysNegative;
            case RewardEffectParameterIds.TRIGGER_ATTACK_COUNT:
            case RewardEffectParameterIds.LINK_RADIUS:
            case RewardEffectParameterIds.HOME_HEALTH_THRESHOLD:
            case RewardEffectParameterIds.EXPLOSION_RADIUS:
            case RewardEffectParameterIds.STATUS_DURATION:
            case RewardEffectParameterIds.TICK_INTERVAL:
                return RewardEffectAutoImpactRule.AlwaysNeutral;
            default:
                return RewardEffectAutoImpactRule.GreaterThanZeroIsPositive;
        }
    }

    // 判断参数是否应限制在 0 到 1。
    public static bool IsRatioLimitedParameter(string parameterId)
    {
        switch (NormalizeParameterId(parameterId))
        {
            case RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE:
            case RewardEffectParameterIds.HOME_HEALTH_THRESHOLD:
            case RewardEffectParameterIds.TRIGGER_CHANCE:
            case RewardEffectParameterIds.TICK_DAMAGE:
                return true;
            default:
                return false;
        }
    }

    // 判断参数是否应为正整数。
    public static bool IsPositiveIntegerParameter(string parameterId)
    {
        switch (NormalizeParameterId(parameterId))
        {
            case RewardEffectParameterIds.TRIGGER_ATTACK_COUNT:
            case RewardEffectParameterIds.EXTRA_ATTACK_COUNT:
            case RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE:
            case RewardEffectParameterIds.ATTACK_HEALTH_COST:
            case RewardEffectParameterIds.EXPLOSION_DAMAGE:
            case RewardEffectParameterIds.PIERCE_COUNT:
                return true;
            default:
                return false;
        }
    }

    // 判断参数是否应为非负整数。
    public static bool IsNonNegativeIntegerParameter(string parameterId)
    {
        return NormalizeParameterId(parameterId) == RewardEffectParameterIds.INITIAL_STAR_BONUS;
    }

    // 判断参数是否应为正数。
    public static bool IsPositiveNumberParameter(string parameterId)
    {
        switch (NormalizeParameterId(parameterId))
        {
            case RewardEffectParameterIds.LINK_RADIUS:
            case RewardEffectParameterIds.EXPLOSION_RADIUS:
            case RewardEffectParameterIds.STATUS_DURATION:
            case RewardEffectParameterIds.TICK_INTERVAL:
            case RewardEffectParameterIds.TICK_DAMAGE:
                return true;
            default:
                return false;
        }
    }

    // 标准化参数 ID。
    private static string NormalizeParameterId(string parameterId)
    {
        return string.IsNullOrWhiteSpace(parameterId) ? RewardEffectParameterIds.VALUE : parameterId.Trim();
    }
}
