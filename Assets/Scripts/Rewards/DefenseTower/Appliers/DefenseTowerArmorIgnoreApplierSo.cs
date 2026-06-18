using UnityEngine;

/// <summary>
/// 防御塔护甲穿透奖励应用器，负责提高箭矢无视敌人护甲的比例。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Armor Ignore Applier")]
public class DefenseTowerArmorIgnoreApplierSo : DefenseTowerRewardApplierSo
{
    // 应用护甲穿透加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state))
        {
            float armorIgnorePercent = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ARMOR_IGNORE_PERCENT, GetValue(config));
            state.AddArmorIgnorePercent(armorIgnorePercent);
        }
    }
}
