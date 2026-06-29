using UnityEngine;

/// <summary>
/// 箭矢命中续飞规则基类，负责根据本次发射能力决定命中后是否继续飞行。
/// </summary>
public abstract class ArrowHitContinuation : MonoBehaviour
{
    // 从本次发射快照复制续飞配置。
    public abstract void Configure(ArrowLaunchData launchData);

    // 计算本次命中后是否继续飞行。
    public abstract bool ShouldContinue(ArrowHitContext context);

    // 清理续飞规则持有的运行时状态。
    public abstract void ResetState();
}
