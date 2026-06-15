using System.Collections.Generic;

/// <summary>
/// 基地奖励触发分发器，预留给后续基地类运行时奖励效果。
/// </summary>
public class HomeRewardTriggerDispatcher
{
    private readonly List<IHomeRewardTrigger> _effectList = new();

    public int Count => _effectList.Count;

    // 注册一条基地类运行时奖励效果。
    public void RegisterEffect(IHomeRewardTrigger effect, RewardCardEffectConfig config)
    {
        if (effect == null || !effect.ShouldRegisterRuntimeEffect)
        {
            return;
        }

        _effectList.Add(effect);
    }
}
