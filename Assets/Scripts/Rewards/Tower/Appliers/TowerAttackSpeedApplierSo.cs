using UnityEngine;

/// <summary>
/// 防御塔攻速奖励应用器，负责把攻击速度加成写入防御塔奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Attack Speed Applier")]
public class TowerAttackSpeedApplierSo : TowerRewardApplierSo
{
    // 应用攻击速度加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
