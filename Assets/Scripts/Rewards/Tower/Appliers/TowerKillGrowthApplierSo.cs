using UnityEngine;

/// <summary>
/// 防御塔击杀成长奖励应用器，负责按击杀次数自动提升防御塔星级。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Tower Kill Growth Applier")]
public class TowerKillGrowthApplierSo : TowerRewardApplierSo, ITowerEnemyKilledTrigger
{
    private const string KILL_COUNTER_ID = "KillAutoUpgrade";

    // 应用防御塔击杀成长配置。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state) || config == null)
        {
            return;
        }

        int killCountToUpgrade = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0, true);
        state.AddKillCountAutoUpgrade(killCountToUpgrade);
    }

    // 按击杀次数自动提升防御塔星级。
    public void OnEnemyKilled(TowerRewardRuntimeState runtimeState, TowerEnemyKilledContext context)
    {
        int killCountToUpgrade = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0);
        if (killCountToUpgrade <= 0 || !context.SourceTowerCombatSystem)
        {
            return;
        }

        TowerCombatSystem sourceTowerCombatSystem = context.SourceTowerCombatSystem;
        int killCount = runtimeState.IncrementCounter(sourceTowerCombatSystem, KILL_COUNTER_ID);
        if (killCount < killCountToUpgrade)
        {
            return;
        }

        runtimeState.ResetCounter(sourceTowerCombatSystem, KILL_COUNTER_ID);
        BuildingUpgradeButton upgradeButton = TowerRegistry.GetUpgradeButton(sourceTowerCombatSystem);
        if (upgradeButton)
        {
            upgradeButton.UpgradeOneLevelWithoutCost();
        }
    }
}
