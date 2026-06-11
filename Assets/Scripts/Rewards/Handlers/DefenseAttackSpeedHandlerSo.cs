using UnityEngine;

/// <summary>
/// 闃插尽濉旀敾鍑婚€熷害鍔犳垚 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Speed Handler")]
public class DefenseAttackSpeedHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤鏀诲嚮閫熷害鍔犳垚銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
