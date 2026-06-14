using UnityEngine;

/// <summary>
/// 防御塔攻击速度加成 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Attack Speed Handler")]
public class DefenseTowerAttackSpeedApplierSo : DefenseTowerRewardApplierSo
{
    // 应用攻击速度加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        if (TryGetDefenseTowerRewardState(applyContext, out DefenseTowerRewardState state))
        {
            state.AddAttackSpeedBonus(GetValue(config));
        }
    }
}
