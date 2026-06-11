using UnityEngine;

/// <summary>
/// 防御塔最大生命加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Max Health Handler")]
public class DefenseMaxHealthHandlerSo : DefenseRewardHandlerSo
{
    // 应用最大生命加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
