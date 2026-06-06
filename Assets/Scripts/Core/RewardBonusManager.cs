using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局奖励加成管理器，负责接收奖励卡牌并对外提供本局奖励查询入口。
/// </summary>
public class RewardBonusManager : MonoBehaviour
{
    private const float DEFAULT_MULTIPLIER = 1f;

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
        OnRewardBonusChanged?.Invoke();
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
}
