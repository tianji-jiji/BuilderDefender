using UnityEngine;

/// <summary>
/// 鏂板缓闃插尽濉斿垵濮嬫槦绾?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense New Tower Initial Star Handler")]
public class DefenseNewTowerInitialStarHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏂板鍒濆鏄熺骇鍔犳垚銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            int starBonus = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.INITIAL_STAR_BONUS, Mathf.RoundToInt(GetValue(config)));
            state.AddNewTowerInitialStarBonus(starBonus);
        }
    }
}
