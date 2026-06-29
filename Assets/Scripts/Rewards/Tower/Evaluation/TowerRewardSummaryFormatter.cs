using System.Text;
using UnityEngine;

/// <summary>
/// 防御塔奖励摘要格式化器，负责把当前生效奖励转换为 UI 展示文本。
/// </summary>
public static class TowerRewardSummaryFormatter
{
    private const float DEFAULT_MULTIPLIER = 1f;
    private const float PERCENT_FACTOR = 100f;
    private const float SUMMARY_VALUE_EPSILON = 0.001f;
    private const string EMPTY_SUMMARY_TEXT = "当前暂无已获得加成";

    // 构建当前防御塔奖励累计加成摘要文本。
    public static string BuildSummaryText(TowerActiveRewards activeRewards)
    {
        if (activeRewards == null)
        {
            return EMPTY_SUMMARY_TEXT;
        }

        StringBuilder summaryBuilder = new();

        AppendMultiplierLine(summaryBuilder, "攻击力", activeRewards.AttackDamageMultiplier);
        AppendAttackSpeedLine(summaryBuilder, "攻击速度", activeRewards.AttackIntervalMultiplier);
        AppendMultiplierLine(summaryBuilder, "攻击范围", activeRewards.DetectRadiusMultiplier);
        AppendMultiplierLine(summaryBuilder, "最大生命", activeRewards.MaxHealthMultiplier);
        AppendMultiplierLine(summaryBuilder, "建造成本", activeRewards.BuildCostMultiplier);
        AppendPercentLine(summaryBuilder, "护甲穿透", activeRewards.ArmorIgnorePercent);
        AppendMultiplierLine(summaryBuilder, "受到伤害", activeRewards.DamageTakenMultiplier);
        AppendPercentLine(summaryBuilder, "波次结束回血", activeRewards.WaveEndHealPercent);
        AppendRuleLine(summaryBuilder, "击杀自动升星", activeRewards.KillCountAutoUpgrade > 0, $"{activeRewards.KillCountAutoUpgrade} 击触发");
        AppendAttackSpeedLine(summaryBuilder, "超载攻击速度", activeRewards.OverloadAttackIntervalMultiplier);
        AppendPercentLine(summaryBuilder, "每座三星塔攻击力", activeRewards.AttackDamagePerThreeStarTower);
        AppendPercentLine(summaryBuilder, "双倍伤害概率", activeRewards.DoubleDamageChance);
        AppendAttackSpeedLine(summaryBuilder, "防线联动攻速", activeRewards.LinkedAttackIntervalMultiplier);
        AppendRuleLine(summaryBuilder, "防线联动半径", activeRewards.LinkRadius > 0f, activeRewards.LinkRadius.ToString("0.##"));
        AppendRuleLine(summaryBuilder, "新塔初始星级", activeRewards.NewTowerInitialStarBonus > 0, $"+{activeRewards.NewTowerInitialStarBonus}");
        AppendRuleLine(summaryBuilder, "三星爆裂箭", activeRewards.ThreeStarExplosiveArrowEnabled, $"半径 {activeRewards.ExplosionRadius:0.##} / 伤害 {activeRewards.ExplosionDamageMultiplier:0.##}x");

        return summaryBuilder.Length > 0 ? summaryBuilder.ToString() : EMPTY_SUMMARY_TEXT;
    }

    // 追加倍率型奖励摘要行。
    private static void AppendMultiplierLine(StringBuilder summaryBuilder, string label, float multiplier)
    {
        float bonus = multiplier - DEFAULT_MULTIPLIER;
        if (Mathf.Abs(bonus) <= SUMMARY_VALUE_EPSILON)
        {
            return;
        }

        AppendLine(summaryBuilder, label, FormatPercent(bonus));
    }

    // 追加攻速型奖励摘要行。
    private static void AppendAttackSpeedLine(StringBuilder summaryBuilder, string label, float attackIntervalMultiplier)
    {
        if (Mathf.Abs(attackIntervalMultiplier - DEFAULT_MULTIPLIER) <= SUMMARY_VALUE_EPSILON || attackIntervalMultiplier <= 0f)
        {
            return;
        }

        float attackSpeedBonus = DEFAULT_MULTIPLIER / attackIntervalMultiplier - DEFAULT_MULTIPLIER;
        AppendLine(summaryBuilder, label, FormatPercent(attackSpeedBonus));
    }

    // 追加百分比型奖励摘要行。
    private static void AppendPercentLine(StringBuilder summaryBuilder, string label, float percent)
    {
        if (Mathf.Abs(percent) <= SUMMARY_VALUE_EPSILON)
        {
            return;
        }

        AppendLine(summaryBuilder, label, FormatPercent(percent));
    }

    // 追加规则型奖励摘要行。
    private static void AppendRuleLine(StringBuilder summaryBuilder, string label, bool shouldAppend, string valueText)
    {
        if (shouldAppend)
        {
            AppendLine(summaryBuilder, label, valueText);
        }
    }

    // 追加一行奖励摘要文本。
    private static void AppendLine(StringBuilder summaryBuilder, string label, string valueText)
    {
        if (summaryBuilder.Length > 0)
        {
            summaryBuilder.AppendLine();
        }

        summaryBuilder.Append(label);
        summaryBuilder.Append("：");
        summaryBuilder.Append(valueText);
    }

    // 将浮点增量格式化为带符号百分比文本。
    private static string FormatPercent(float value)
    {
        int percent = Mathf.RoundToInt(value * PERCENT_FACTOR);
        return percent > 0 ? $"+{percent}%" : $"{percent}%";
    }
}
