using UnityEngine;

/// <summary>
/// 闃插尽濉旀渶澶х敓鍛藉姞鎴?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Max Health Handler")]
public class DefenseMaxHealthHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏈€澶х敓鍛藉姞鎴愩€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddMaxHealthBonus(GetValue(config));
        }
    }
}
