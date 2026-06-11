using UnityEngine;

/// <summary>
/// 防御塔击杀成长 Handler，当前接入已有击杀自动升星奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Kill Growth Handler")]
public class TowerKillGrowthHandlerSo : DefenseRewardHandlerSo
{
    // 应用防御塔击杀成长配置。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state) || config == null)
        {
            return;
        }

        int killCountToUpgrade = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0, true);
        state.AddKillCountAutoUpgrade(killCountToUpgrade);

        // TODO：如果后续要做“单塔击杀后永久加攻击”，在这里接入防御塔击杀事件和运行时成长状态。
    }
}
