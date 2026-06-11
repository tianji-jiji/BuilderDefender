using UnityEngine;

/// <summary>
/// 闃插尽濉旀姢鐢茬┛閫?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Armor Ignore Handler")]
public class DefenseArmorIgnoreHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鎶ょ敳绌块€忓姞鎴愩€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float armorIgnorePercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ARMOR_IGNORE_PERCENT, GetValue(config));
            state.AddArmorIgnorePercent(armorIgnorePercent);
        }
    }
}
