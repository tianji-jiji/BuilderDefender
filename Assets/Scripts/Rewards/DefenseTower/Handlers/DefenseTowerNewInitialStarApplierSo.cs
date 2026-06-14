using UnityEngine;

/// <summary>
/// 新建防御塔初始星级 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower New Initial Star Handler")]
public class DefenseTowerNewInitialStarApplierSo : DefenseTowerRewardApplierSo
{
    // 应用新塔初始星级加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            int starBonus = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.INITIAL_STAR_BONUS, Mathf.RoundToInt(GetValue(config)));
            state.AddNewTowerInitialStarBonus(starBonus);
        }
    }
}
