using UnityEngine;

/// <summary>
/// 防御塔攻速奖励应用器，负责把攻击速度加成写入防御塔奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Speed Handler")]
public class DefenseTowerAttackSpeedApplierSo : DefenseTowerRewardApplierSo
{
    // 应用攻击速度加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerActiveRewards state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
