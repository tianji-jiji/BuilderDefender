using UnityEngine;

/// <summary>
/// 防御塔索敌范围奖励应用器，负责提高防御塔探测和攻击范围。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Detect Radius Applier")]
public class TowerDetectRadiusApplierSo : TowerRewardApplierSo
{
    // 应用攻击范围加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            state.AddDetectRadiusBonus(GetValue(config));
        }
    }
}
