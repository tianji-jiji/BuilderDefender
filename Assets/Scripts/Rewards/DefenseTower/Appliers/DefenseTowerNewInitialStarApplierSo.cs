using UnityEngine;

/// <summary>
/// 新建防御塔初始星级奖励应用器，负责让新建防御塔获得额外初始星级。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower New Initial Star Applier")]
public class DefenseTowerNewInitialStarApplierSo : DefenseTowerRewardApplierSo
{
    // 应用新塔初始星级加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state))
        {
            int starBonus = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.INITIAL_STAR_BONUS, Mathf.RoundToInt(GetValue(config)));
            state.AddNewTowerInitialStarBonus(starBonus);
        }
    }
}
