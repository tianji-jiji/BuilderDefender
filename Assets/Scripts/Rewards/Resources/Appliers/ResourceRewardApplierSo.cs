/// <summary>
/// 资源奖励应用器基类，预留给后续资源类卡牌效果。
/// </summary>
public abstract class ResourceRewardApplierSo : RewardEffectApplierSo, IResourceRewardTrigger
{
    public virtual bool ShouldRegisterRuntimeEffect => false;

    // 应用资源奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
    }
}
