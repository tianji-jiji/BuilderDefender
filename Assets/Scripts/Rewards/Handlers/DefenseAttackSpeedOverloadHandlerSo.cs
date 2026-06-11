using UnityEngine;

/// <summary>
/// 闃插尽濉旇秴杞芥敾鍑婚€熷害 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Speed Overload Handler")]
public class DefenseAttackSpeedOverloadHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤瓒呰浇鏀婚€熷彉鍖栥€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float attackSpeedBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER, GetValue(config));
            state.AddAttackSpeedOverloadBonus(attackSpeedBonus);
        }
    }
}
