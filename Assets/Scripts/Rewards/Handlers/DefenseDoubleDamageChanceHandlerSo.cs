using UnityEngine;

/// <summary>
/// 闃插尽濉斿弻鍊嶄激瀹虫鐜?Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Double Damage Chance Handler")]
public class DefenseDoubleDamageChanceHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鍙屽€嶄激瀹虫鐜囥€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            float doubleDamageChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.DOUBLE_DAMAGE_CHANCE, GetValue(config));
            state.AddDoubleDamageChance(doubleDamageChance);
        }
    }
}
