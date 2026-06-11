using UnityEngine;

/// <summary>
/// 新建防御塔初始星级 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense New Tower Initial Star Handler")]
public class DefenseNewTowerInitialStarHandlerSo : DefenseRewardHandlerSo
{
    // 应用新塔初始星级加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            int starBonus = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.INITIAL_STAR_BONUS, Mathf.RoundToInt(GetValue(config)));
            state.AddNewTowerInitialStarBonus(starBonus);
        }
    }
}
