using UnityEngine;

/// <summary>
/// 防御塔击杀成长奖励应用器，负责按击杀次数自动提升防御塔星级。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Tower Kill Growth Applier")]
public class DefenseTowerKillGrowthApplierSo : DefenseTowerRuntimeRewardApplierSo
{
    private const string KILL_COUNTER_ID = "KillAutoUpgrade";

    // 应用防御塔击杀成长配置。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state) || config == null)
        {
            return;
        }

        int killCountToUpgrade = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0, true);
        state.AddKillCountAutoUpgrade(killCountToUpgrade);
    }

    // 按击杀次数自动提升防御塔星级。
    public override void OnEnemyKilled(DefenseTowerRewardTriggerInstance instance, DefenseTowerEnemyKillContext context)
    {
        int killCountToUpgrade = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0);
        if (killCountToUpgrade <= 0 || !context.SourceDefenseTowerCombatSystem)
        {
            return;
        }

        int killCount = instance.IncrementCounter(KILL_COUNTER_ID);
        if (killCount < killCountToUpgrade)
        {
            return;
        }

        instance.ResetCounter(KILL_COUNTER_ID);
        BuildingUpgradeButton upgradeButton = DefenseTowerRegistry.GetUpgradeButton(context.SourceDefenseTowerCombatSystem);
        if (upgradeButton)
        {
            upgradeButton.UpgradeOneLevelWithoutCost();
        }
    }
}
