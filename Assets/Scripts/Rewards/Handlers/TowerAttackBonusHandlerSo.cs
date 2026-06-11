using UnityEngine;

/// <summary>
/// 防御塔攻击力加成 Handler，负责把卡牌参数写入防御塔奖励状态。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Attack Bonus Handler")]
public class TowerAttackBonusHandlerSo : DefenseRewardHandlerSo
{
    // 应用防御塔攻击力加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state) || config == null)
        {
            return;
        }

        state.AddAttackDamageBonus(GetValue(config));
    }
}
