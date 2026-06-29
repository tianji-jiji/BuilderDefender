using UnityEngine;

/// <summary>
/// 防御塔攻击力奖励应用器，负责把攻击力加成写入防御塔奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Tower Attack Bonus Applier")]
public class TowerAttackBonusApplierSo : TowerRewardApplierSo
{
    // 应用防御塔攻击力加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerActiveRewards(applyContext, out TowerActiveRewards state) || config == null)
        {
            return;
        }

        state.AddAttackDamageBonus(GetValue(config));
    }
}
