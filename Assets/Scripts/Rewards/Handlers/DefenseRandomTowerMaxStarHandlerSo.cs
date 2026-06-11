using UnityEngine;

/// <summary>
/// 闅忔満闃插尽濉斿崌婊℃槦 Handler銆?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Random Tower Max Star Handler")]
public class DefenseRandomTowerMaxStarHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤闅忔満闃插尽濉旀弧鏄熷嵆鏃舵晥鏋溿€?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        DefenseRewardImmediateEffectApplier.UpgradeRandomTowerToMaxStar();
    }
}
