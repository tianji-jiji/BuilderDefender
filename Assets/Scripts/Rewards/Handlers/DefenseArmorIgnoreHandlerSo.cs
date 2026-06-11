using UnityEngine;

/// <summary>
/// 防御塔护甲穿透 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Armor Ignore Handler")]
public class DefenseArmorIgnoreHandlerSo : DefenseRewardHandlerSo
{
    // 应用护甲穿透加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float armorIgnorePercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ARMOR_IGNORE_PERCENT, GetValue(config));
            state.AddArmorIgnorePercent(armorIgnorePercent);
        }
    }
}
