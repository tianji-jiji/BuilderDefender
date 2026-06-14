using UnityEngine;

/// <summary>
/// 随机防御塔升满星 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Tower Random Max Star Handler")]
public class DefenseTowerRandomMaxStarHandlerSo : DefenseTowerRewardHandlerSo
{
    // 应用随机防御塔满星即时效果。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        DefenseTowerRewardImmediateEffectApplier.UpgradeRandomTowerToMaxStar();
    }
}
