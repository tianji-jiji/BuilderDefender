using System.Collections.Generic;

/// <summary>
/// 资源奖励触发分发器，预留给后续资源类运行时奖励效果。
/// </summary>
public class ResourceRewardTriggerDispatcher
{
    private readonly List<IResourceRewardTrigger> _effectList = new();

    // 注册一条资源类运行时奖励效果。
    public void RegisterEffect(IResourceRewardTrigger effect, RewardCardEffectConfig config)
    {
        if (effect == null || !effect.ShouldRegisterRuntimeEffect)
        {
            return;
        }

        _effectList.Add(effect);
    }
}
