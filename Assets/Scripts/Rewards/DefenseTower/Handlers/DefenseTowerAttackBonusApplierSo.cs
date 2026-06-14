using UnityEngine;

/// <summary>
/// 防御塔攻击力加成 Handler，负责把卡牌参数写入防御塔奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Attack Bonus Handler")]
public class DefenseTowerAttackBonusApplierSo : DefenseTowerRewardApplierSo
{
    // 应用防御塔攻击力加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (!TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state) || config == null)
        {
            return;
        }

        state.AddAttackDamageBonus(GetValue(config));
    }
}
