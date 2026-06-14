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

    private readonly DefenseTowerRewardModifiers _defenseTowerRewardModifiers = new();
    private readonly DefenseTowerCardEffectDispatcher _defenseTowerCardEffectDispatcher = new();

    private EnemyWaveManager _waveManager;

    public float DefenseTowerAttackDamageMultiplier => _defenseTowerRewardModifiers.AttackDamageMultiplier;
    public float DefenseTowerAttackIntervalMultiplier => _defenseTowerRewardModifiers.AttackIntervalMultiplier;
    public float DefenseTowerDetectRadiusMultiplier => _defenseTowerRewardModifiers.DetectRadiusMultiplier;
    public float DefenseTowerMaxHealthMultiplier => _defenseTowerRewardModifiers.MaxHealthMultiplier;
    public float DefenseTowerBuildCostMultiplier => _defenseTowerRewardModifiers.BuildCostMultiplier;
    public float DefenseTowerArmorIgnorePercent => _defenseTowerRewardModifiers.ArmorIgnorePercent;
    public IReadOnlyList<DefenseTowerExtraAttackRule> DefenseTowerExtraAttackRuleList => _defenseTowerRewardModifiers.ExtraAttackRuleList;
    public float DefenseTowerDamageTakenMultiplier => _defenseTowerRewardModifiers.DamageTakenMultiplier;
    public float DefenseTowerWaveEndHealPercent => _defenseTowerRewardModifiers.WaveEndHealPercent;
    public int DefenseTowerKillCountAutoUpgrade => _defenseTowerRewardModifiers.KillCountAutoUpgrade;
    public float DefenseTowerOverloadAttackIntervalMultiplier => _defenseTowerRewardModifiers.OverloadAttackIntervalMultiplier;
    public IReadOnlyList<DefenseTowerAttackHealthCostRule> DefenseTowerAttackHealthCostRuleList => _defenseTowerRewardModifiers.AttackHealthCostRuleList;
    public float DefenseTowerAttackDamagePerThreeStarTower => _defenseTowerRewardModifiers.AttackDamagePerThreeStarTower;
    public float DefenseTowerDoubleDamageChance => _defenseTowerRewardModifiers.DoubleDamageChance;
    public float DefenseTowerLinkedAttackIntervalMultiplier => _defenseTowerRewardModifiers.LinkedAttackIntervalMultiplier;
    public float DefenseTowerLinkRadius => _defenseTowerRewardModifiers.LinkRadius;
    public int DefenseTowerNewInitialStarBonus => _defenseTowerRewardModifiers.NewTowerInitialStarBonus;
    public bool DefenseTowerThreeStarExplosiveArrowEnabled => _defenseTowerRewardModifiers.ThreeStarExplosiveArrowEnabled;
    public float DefenseTowerExplosionRadius => _defenseTowerRewardModifiers.ExplosionRadius;
    public float DefenseTowerExplosionDamageMultiplier => _defenseTowerRewardModifiers.ExplosionDamageMultiplier;
    public DefenseTowerCardEffectDispatcher DefenseTowerCardEffectDispatcher => _defenseTowerCardEffectDispatcher;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BindWaveManager();
    }

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

        RewardEffectContext context = new RewardEffectContext(this, _defenseTowerRewardModifiers, ResourceManager.Instance, EnemyWaveManager.Instance, BuildManager.Instance, _defenseTowerCardEffectDispatcher);
        DefenseTowerRewardConfigApplier.ApplyEffects(rewardCard.EffectConfigList, _defenseTowerRewardModifiers, context);
        RecordRewardSelection(rewardCard);
        OnRewardBonusChanged?.Invoke();
    }

    // 构建当前防御塔奖励累计加成摘要文本。
    public string BuildDefenseTowerRewardSummaryText()
    {
        StringBuilder summaryBuilder = new StringBuilder();

        AppendMultiplierLine(summaryBuilder, "攻击力", DefenseTowerAttackDamageMultiplier);
        AppendAttackSpeedLine(summaryBuilder, "攻击速度", DefenseTowerAttackIntervalMultiplier);
        AppendMultiplierLine(summaryBuilder, "攻击范围", DefenseTowerDetectRadiusMultiplier);
        AppendMultiplierLine(summaryBuilder, "最大生命", DefenseTowerMaxHealthMultiplier);
        AppendMultiplierLine(summaryBuilder, "建造成本", DefenseTowerBuildCostMultiplier);
        AppendPercentLine(summaryBuilder, "护甲穿透", DefenseTowerArmorIgnorePercent);
        AppendMultiplierLine(summaryBuilder, "受到伤害", DefenseTowerDamageTakenMultiplier);
        AppendPercentLine(summaryBuilder, "波次结束回血", DefenseTowerWaveEndHealPercent);
        AppendRuleLine(summaryBuilder, "击杀自动升星", DefenseTowerKillCountAutoUpgrade > 0, $"{DefenseTowerKillCountAutoUpgrade} 击触发");
        AppendAttackSpeedLine(summaryBuilder, "超载攻击速度", DefenseTowerOverloadAttackIntervalMultiplier);
        AppendPercentLine(summaryBuilder, "每座三星塔攻击力", DefenseTowerAttackDamagePerThreeStarTower);
        AppendPercentLine(summaryBuilder, "双倍伤害概率", DefenseTowerDoubleDamageChance);
        AppendAttackSpeedLine(summaryBuilder, "防线联动攻速", DefenseTowerLinkedAttackIntervalMultiplier);
        AppendRuleLine(summaryBuilder, "防线联动半径", DefenseTowerLinkRadius > 0f, DefenseTowerLinkRadius.ToString("0.##"));
        AppendRuleLine(summaryBuilder, "新塔初始星级", DefenseTowerNewInitialStarBonus > 0, $"+{DefenseTowerNewInitialStarBonus}");
        AppendRuleLine(summaryBuilder, "三星爆裂箭", DefenseTowerThreeStarExplosiveArrowEnabled, $"半径 {DefenseTowerExplosionRadius:0.##} / 伤害 {DefenseTowerExplosionDamageMultiplier:0.##}x");

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
            costMultiplier = Instance.DefenseTowerBuildCostMultiplier;
        }

        return Mathf.Max(0, Mathf.RoundToInt(resourceCost.amount * costMultiplier));
    }

    // 判断最终防线奖励当前是否处于激活状态。
    public bool IsFinalDefenseActive()
    {
        return _defenseTowerRewardModifiers.IsFinalDefenseActive();
    }

    // 获取当前防御体系战力快照，供敌人成长系统软响应玩家强度。
    public DefenseTowerPowerSnapshot GetDefenseTowerPowerSnapshot()
    {
        return new DefenseTowerPowerSnapshot(
            DefenseTowerAttackDamageMultiplier,
            GetAttackSpeedPowerMultiplier(),
            DefenseTowerArmorIgnorePercent,
            _defenseTowerRewardModifiers.GetExtraAttackPowerMultiplier(),
            _defenseTowerRewardModifiers.GetCriticalPowerMultiplier(),
            _defenseTowerRewardModifiers.GetExplosivePowerMultiplier());
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        return _defenseTowerRewardModifiers.GetFinalDefenseTowerAttackDamageMultiplier();
    }

    // 将攻击间隔倍率转换为战力倍率。
    private float GetAttackSpeedPowerMultiplier()
    {
        float attackIntervalMultiplier = DefenseTowerAttackIntervalMultiplier
                                         * DefenseTowerOverloadAttackIntervalMultiplier
                                         * DefenseTowerLinkedAttackIntervalMultiplier;
        if (attackIntervalMultiplier <= 0f)
        {
            return DEFAULT_MULTIPLIER;
        }

        return Mathf.Max(DEFAULT_MULTIPLIER, DEFAULT_MULTIPLIER / attackIntervalMultiplier);
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
        _defenseTowerCardEffectDispatcher.OnWaveCompleted(new DefenseTowerWaveContext(waveIndex));
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
