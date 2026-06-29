using UnityEngine;

/// <summary>
/// 最终防线奖励应用器，负责在基地低血量时提高防御塔攻击力。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Final Defense Attack Damage Applier")]
public class TowerFinalAttackDamageApplierSo : TowerRewardApplierSo
{
    // 应用最终防线攻击加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            float threshold = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.HOME_HEALTH_THRESHOLD, 0f, true);
            state.AddFinalDefense(GetValue(config), threshold);
        }
    }
}
