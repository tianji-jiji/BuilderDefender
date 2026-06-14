using UnityEngine;

/// <summary>
/// 防御塔护甲穿透 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Armor Ignore Handler")]
public class DefenseTowerArmorIgnoreApplierSo : DefenseTowerRewardApplierSo
{
    // 应用护甲穿透加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            float armorIgnorePercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ARMOR_IGNORE_PERCENT, GetValue(config));
            state.AddArmorIgnorePercent(armorIgnorePercent);
        }
    }
}
