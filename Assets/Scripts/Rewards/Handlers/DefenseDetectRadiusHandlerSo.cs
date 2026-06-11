using UnityEngine;

/// <summary>
/// 闃插尽濉旀敾鍑昏寖鍥村姞鎴?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Detect Radius Handler")]
public class DefenseDetectRadiusHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏀诲嚮鑼冨洿鍔犳垚銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddDetectRadiusBonus(GetValue(config));
        }
    }
}
