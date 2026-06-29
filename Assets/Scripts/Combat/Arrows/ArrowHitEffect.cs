using UnityEngine;

/// <summary>
/// 箭矢命中效果基类，定义效果配置、结算顺序和池化状态清理契约。
/// </summary>
public abstract class ArrowHitEffect : MonoBehaviour
{
    public abstract int ExecutionOrder { get; }

    // 从本次发射快照复制效果配置。
    public abstract void Configure(ArrowLaunchData launchData);

    // 对本次命中应用效果。
    public abstract void Apply(ArrowHitContext context);

    // 清理效果持有的运行时状态。
    public abstract void ResetState();
}
