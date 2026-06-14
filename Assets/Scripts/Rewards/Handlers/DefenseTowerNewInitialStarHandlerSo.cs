using UnityEngine;

/// <summary>
/// 新建防御塔初始星级 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower New Initial Star Handler")]
public class DefenseTowerNewInitialStarHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用新塔初始星级加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            int starBonus = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.INITIAL_STAR_BONUS, Mathf.RoundToInt(GetValue(config)));
            state.AddNewTowerInitialStarBonus(starBonus);
        }
    }
}
