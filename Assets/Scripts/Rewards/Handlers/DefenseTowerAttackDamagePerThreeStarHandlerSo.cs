using UnityEngine;

/// <summary>
/// 三星防御塔数量攻击加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Damage Per Three Star Handler")]
public class DefenseTowerAttackDamagePerThreeStarHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用每座三星塔攻击加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardModifiers(context, out DefenseTowerRewardModifiers state))
        {
            float damageBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_BONUS_PER_THREE_STAR_TOWER, GetValue(config));
            state.AddAttackDamagePerThreeStarTower(damageBonus);
        }
    }
}
