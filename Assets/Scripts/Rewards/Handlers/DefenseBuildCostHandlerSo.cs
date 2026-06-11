using UnityEngine;

/// <summary>
/// 闃插尽濉斿缓閫犳垚鏈彉鍖?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Build Cost Handler")]
public class DefenseBuildCostHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤寤洪€犳垚鏈彉鍖栥€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddBuildCostBonus(GetValue(config));
        }
    }
}
