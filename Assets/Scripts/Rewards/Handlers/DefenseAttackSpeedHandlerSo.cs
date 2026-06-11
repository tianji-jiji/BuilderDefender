using UnityEngine;

/// <summary>
/// 防御塔攻击速度加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Attack Speed Handler")]
public class DefenseAttackSpeedHandlerSo : DefenseRewardHandlerSo
{
    // 应用攻击速度加成。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
