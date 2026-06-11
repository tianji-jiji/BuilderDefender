using UnityEngine;

/// <summary>
/// 闃插尽濉斿彈鍒颁激瀹冲彉鍖?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Damage Taken Handler")]
public class DefenseDamageTakenHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鍙楀埌浼ゅ鍙樺寲銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float damageTakenBonus = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DAMAGE_TAKEN_MULTIPLIER, GetValue(config));
            state.AddDamageTakenBonus(damageTakenBonus);
        }
    }
}
