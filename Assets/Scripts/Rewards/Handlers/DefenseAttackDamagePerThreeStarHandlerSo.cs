using UnityEngine;

/// <summary>
/// 涓夋槦闃插尽濉旀暟閲忔敾鍑诲姞鎴?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Damage Per Three Star Handler")]
public class DefenseAttackDamagePerThreeStarHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤姣忓骇涓夋槦濉旀敾鍑诲姞鎴愩€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float damageBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_BONUS_PER_THREE_STAR_TOWER, GetValue(config));
            state.AddAttackDamagePerThreeStarTower(damageBonus);
        }
    }
}
