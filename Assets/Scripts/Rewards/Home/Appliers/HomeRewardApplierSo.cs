/// <summary>
/// 基地奖励应用器基类，预留给后续基地类卡牌效果。
/// </summary>
public abstract class HomeRewardApplierSo : RewardEffectApplierSo, IHomeRewardTrigger
{
    public virtual bool ShouldRegisterRuntimeEffect => false;

    // 应用基地奖励配置，默认不写入任何加成。
    public override void Apply(RewardEffectApplyContext applyContext, RewardCardEffectConfig config)
    {
    }
}
