using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 全局奖励加成管理器，负责接收奖励卡牌并对外提供本局奖励查询入口。
/// </summary>
public class RewardBonusManager : MonoBehaviour
{
    private const float DEFAULT_MULTIPLIER = 1f;
    private const float PERCENT_FACTOR = 100f;
    private const float SUMMARY_VALUE_EPSILON = 0.001f;
    private const string EMPTY_SUMMARY_TEXT = "当前暂无已获得加成";

    public static RewardBonusManager Instance;

    // 奖励数值发生变化时通知所有需要刷新属性的对象。
    public static event Action OnRewardBonusChanged;

    private readonly DefenseRewardState _defenseRewardState = new DefenseRewardState();

    private EnemyWaveManager _waveManager;

    public float DefenseAttackDamageMultiplier => _defenseRewardState.AttackDamageMultiplier;
    public float DefenseAttackIntervalMultiplier => _defenseRewardState.AttackIntervalMultiplier;
    public float DefenseDetectRadiusMultiplier => _defenseRewardState.DetectRadiusMultiplier;
    public float DefenseMaxHealthMultiplier => _defenseRewardState.MaxHealthMultiplier;
    public float DefenseBuildCostMultiplier => _defenseRewardState.BuildCostMultiplier;
    public float DefenseArmorIgnorePercent => _defenseRewardState.ArmorIgnorePercent;
    public IReadOnlyList<DefenseExtraAttackRule> DefenseExtraAttackRuleList => _defenseRewardState.ExtraAttackRuleList;
    public float DefenseDamageTakenMultiplier => _defenseRewardState.DamageTakenMultiplier;
    public float DefenseWaveEndHealPercent => _defenseRewardState.WaveEndHealPercent;
    public int DefenseKillCountAutoUpgrade => _defenseRewardState.KillCountAutoUpgrade;
    public float DefenseOverloadAttackIntervalMultiplier => _defenseRewardState.OverloadAttackIntervalMultiplier;
    public IReadOnlyList<DefenseAttackHealthCostRule> DefenseAttackHealthCostRuleList => _defenseRewardState.AttackHealthCostRuleList;
    public float DefenseAttackDamagePerThreeStarTower => _defenseRewardState.AttackDamagePerThreeStarTower;
    public float DefenseDoubleDamageChance => _defenseRewardState.DoubleDamageChance;
    public float DefenseLinkedAttackIntervalMultiplier => _defenseRewardState.LinkedAttackIntervalMultiplier;
    public float DefenseLinkRadius => _defenseRewardState.LinkRadius;
    public int DefenseNewTowerInitialStarBonus => _defenseRewardState.NewTowerInitialStarBonus;
    public bool DefenseThreeStarExplosiveArrowEnabled => _defenseRewardState.ThreeStarExplosiveArrowEnabled;
    public float DefenseExplosionRadius => _defenseRewardState.ExplosionRadius;
    public float DefenseExplosionDamageMultiplier => _defenseRewardState.ExplosionDamageMultiplier;

    // 初始化全局奖励管理器单例。
    private void Awake()
    {
        Instance = this;
    }

    // 绑定波次结束事件。
    private void Start()
    {
        BindWaveManager();
    }

    // 解绑波次结束事件。
    private void OnDisable()
    {
        UnbindWaveManager();
    }

    // 应用一张奖励卡中配置的全部效果。
    public void ApplyReward(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return;
        }

        DefenseRewardEffectApplier.ApplyEffects(rewardCard.EffectConfigList, _defenseRewardState);
        RecordRewardSelection(rewardCard);
        OnRewardBonusChanged?.Invoke();
    }

    // 构建当前防御塔奖励累计加成摘要文本。
    public string BuildDefenseRewardSummaryText()
    {
        StringBuilder summaryBuilder = new StringBuilder();

        AppendMultiplierLine(summaryBuilder, "攻击力", DefenseAttackDamageMultiplier);
        AppendAttackSpeedLine(summaryBuilder, "攻击速度", DefenseAttackIntervalMultiplier);
        AppendMultiplierLine(summaryBuilder, "攻击范围", DefenseDetectRadiusMultiplier);
        AppendMultiplierLine(summaryBuilder, "最大生命", DefenseMaxHealthMultiplier);
        AppendMultiplierLine(summaryBuilder, "建造成本", DefenseBuildCostMultiplier);
        AppendPercentLine(summaryBuilder, "护甲穿透", DefenseArmorIgnorePercent);
        AppendMultiplierLine(summaryBuilder, "受到伤害", DefenseDamageTakenMultiplier);
        AppendPercentLine(summaryBuilder, "波次结束回血", DefenseWaveEndHealPercent);
        AppendRuleLine(summaryBuilder, "击杀自动升星", DefenseKillCountAutoUpgrade > 0, $"{DefenseKillCountAutoUpgrade} 击触发");
        AppendAttackSpeedLine(summaryBuilder, "超载攻击速度", DefenseOverloadAttackIntervalMultiplier);
        AppendPercentLine(summaryBuilder, "每座三星塔攻击力", DefenseAttackDamagePerThreeStarTower);
        AppendPercentLine(summaryBuilder, "双倍伤害概率", DefenseDoubleDamageChance);
        AppendAttackSpeedLine(summaryBuilder, "防线联动攻速", DefenseLinkedAttackIntervalMultiplier);
        AppendRuleLine(summaryBuilder, "防线联动半径", DefenseLinkRadius > 0f, DefenseLinkRadius.ToString("0.##"));
        AppendRuleLine(summaryBuilder, "新塔初始星级", DefenseNewTowerInitialStarBonus > 0, $"+{DefenseNewTowerInitialStarBonus}");
        AppendRuleLine(summaryBuilder, "三星爆裂箭", DefenseThreeStarExplosiveArrowEnabled, $"半径 {DefenseExplosionRadius:0.##} / 伤害 {DefenseExplosionDamageMultiplier:0.##}x");

        return summaryBuilder.Length > 0 ? summaryBuilder.ToString() : EMPTY_SUMMARY_TEXT;
    }

    // 计算建筑消耗经过全局奖励后的实际数量。
    public static int GetAdjustedBuildCostAmount(BuildingSo buildingSo, ResourceCost resourceCost)
    {
        if (resourceCost == null)
        {
            return 0;
        }

        float costMultiplier = DEFAULT_MULTIPLIER;
        if (buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Defense && Instance)
        {
            costMultiplier = Instance.DefenseBuildCostMultiplier;
        }

        return Mathf.Max(0, Mathf.RoundToInt(resourceCost.amount * costMultiplier));
    }

    // 判断最终防线奖励当前是否处于激活状态。
    public bool IsFinalDefenseActive()
    {
        return _defenseRewardState.IsFinalDefenseActive();
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseAttackDamageMultiplier()
    {
        return _defenseRewardState.GetFinalDefenseAttackDamageMultiplier();
    }

    // 记录本次奖励选择，缺少历史组件时只输出警告而不阻断数值生效。
    private void RecordRewardSelection(RewardCardSo rewardCard)
    {
        if (RewardSelectionHistory.Instance)
        {
            RewardSelectionHistory.Instance.RecordReward(rewardCard);
            return;
        }

        Debug.LogWarning("RewardSelectionHistory is missing in scene. Reward bonus applied, but visible reward history was not recorded.");
    }

    // 绑定当前场景中的波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !EnemyWaveManager.Instance)
        {
            return;
        }

        _waveManager = EnemyWaveManager.Instance;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
    }

    // 解绑当前场景中的波次管理器。
    private void UnbindWaveManager()
    {
        if (!_waveManager)
        {
            return;
        }

        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
        _waveManager = null;
    }

    // 波次结束时应用波次结算型奖励。
    private void HandleWaveCompleted(int waveIndex)
    {
        DefenseRewardWaveEndApplier.ApplyWaveEndHeal(DefenseWaveEndHealPercent);
    }

    // 追加倍率型奖励摘要行。
    private void AppendMultiplierLine(StringBuilder summaryBuilder, string label, float multiplier)
    {
        float bonus = multiplier - DEFAULT_MULTIPLIER;
        if (Mathf.Abs(bonus) <= SUMMARY_VALUE_EPSILON)
        {
            return;
        }

        AppendLine(summaryBuilder, label, FormatPercent(bonus));
    }

    // 追加攻速型奖励摘要行。
    private void AppendAttackSpeedLine(StringBuilder summaryBuilder, string label, float attackIntervalMultiplier)
    {
        if (Mathf.Abs(attackIntervalMultiplier - DEFAULT_MULTIPLIER) <= SUMMARY_VALUE_EPSILON || attackIntervalMultiplier <= 0f)
        {
            return;
        }

        float attackSpeedBonus = DEFAULT_MULTIPLIER / attackIntervalMultiplier - DEFAULT_MULTIPLIER;
        AppendLine(summaryBuilder, label, FormatPercent(attackSpeedBonus));
    }

    // 追加百分比型奖励摘要行。
    private void AppendPercentLine(StringBuilder summaryBuilder, string label, float percent)
    {
        if (Mathf.Abs(percent) <= SUMMARY_VALUE_EPSILON)
        {
            return;
        }

        AppendLine(summaryBuilder, label, FormatPercent(percent));
    }

    // 追加规则型奖励摘要行。
    private void AppendRuleLine(StringBuilder summaryBuilder, string label, bool shouldAppend, string valueText)
    {
        if (!shouldAppend)
        {
            return;
        }

        AppendLine(summaryBuilder, label, valueText);
    }

    // 追加一行奖励摘要文本。
    private void AppendLine(StringBuilder summaryBuilder, string label, string valueText)
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
    private string FormatPercent(float value)
    {
        int percent = Mathf.RoundToInt(value * PERCENT_FACTOR);
        return percent > 0 ? $"+{percent}%" : $"{percent}%";
    }
}
