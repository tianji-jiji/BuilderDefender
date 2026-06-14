using UnityEngine;

/// <summary>
/// 防御塔击杀成长 Handler，当前接入已有击杀自动升星奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Kill Growth Handler")]
public class DefenseTowerKillGrowthHandlerSo : DefenseTowerRewardHandlerSo
{
    private const string KILL_COUNTER_ID = "KillAutoUpgrade";

    public override bool ShouldRegisterRuntimeEffect => true;

    // 应用防御塔击杀成长配置。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state) || config == null)
        {
            return;
        }

        int killCountToUpgrade = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0, true);
        state.AddKillCountAutoUpgrade(killCountToUpgrade);
    }

    // 按击杀次数自动提升防御塔星级。
    public override void OnEnemyKilled(DefenseTowerRuntimeEffectInstance instance, DefenseTowerEnemyKillContext context)
    {
        int killCountToUpgrade = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0);
        if (killCountToUpgrade <= 0 || !context.SourceDefenseTowerSystem)
        {
            return;
        }

        int killCount = instance.IncrementCounter(KILL_COUNTER_ID);
        if (killCount < killCountToUpgrade)
        {
            return;
        }

        instance.ResetCounter(KILL_COUNTER_ID);
        BuildingUpgradeButton upgradeButton = DefenseTowerTracker.GetUpgradeButton(context.SourceDefenseTowerSystem);
        if (upgradeButton)
        {
            upgradeButton.UpgradeOneLevelWithoutCost();
        }
    }
}
