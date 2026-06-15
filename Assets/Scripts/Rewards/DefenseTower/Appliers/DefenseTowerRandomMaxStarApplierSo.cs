using UnityEngine;

/// <summary>
/// 随机满星奖励应用器，负责立刻把一座可升级防御塔升到满星。
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
