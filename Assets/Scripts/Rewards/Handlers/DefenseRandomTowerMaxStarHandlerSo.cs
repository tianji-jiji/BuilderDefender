using UnityEngine;

/// <summary>
/// 随机防御塔升满星 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Random Tower Max Star Handler")]
public class DefenseRandomTowerMaxStarHandlerSo : DefenseRewardHandlerSo
{
    // 应用随机防御塔满星即时效果。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        DefenseRewardImmediateEffectApplier.UpgradeRandomTowerToMaxStar();
    }
}
