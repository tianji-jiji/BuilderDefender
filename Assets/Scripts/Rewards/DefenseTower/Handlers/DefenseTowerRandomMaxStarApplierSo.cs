using UnityEngine;

/// <summary>
/// 随机防御塔升满星 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Random Max Star Handler")]
public class DefenseTowerRandomMaxStarApplierSo : DefenseTowerRewardApplierSo
{
    // 应用随机防御塔满星即时效果。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
        DefenseTowerImmediateRewardEffectApplier.UpgradeRandomTowerToMaxStar();
    }
}
