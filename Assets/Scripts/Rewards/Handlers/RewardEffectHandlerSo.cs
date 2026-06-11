using UnityEngine;

/// <summary>
/// Reward 效果运行时处理器，每一种复杂效果可以通过独立 Handler 实现。
/// </summary>
public abstract class RewardEffectHandlerSo : ScriptableObject
{
    // 应用当前 Reward 效果配置。
    public abstract void Apply(RewardEffectContext context, RewardEffectConfig config);
}
