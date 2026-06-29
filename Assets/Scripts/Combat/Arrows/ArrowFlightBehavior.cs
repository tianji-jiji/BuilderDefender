using UnityEngine;

/// <summary>
/// 箭矢飞行行为基类，定义单支箭的初始化、移动、续飞和清理契约。
/// </summary>
public abstract class ArrowFlightBehavior : MonoBehaviour
{
    public abstract ArrowFlightBehaviorType BehaviorType { get; }

    // 使用本次发射数据初始化飞行行为。
    public abstract bool InitializeFlight(Rigidbody2D rigidbody2D, Enemy targetEnemy, float flySpeed);

    // 推进一次物理帧飞行，并返回当前是否仍可继续。
    public abstract bool TickFlight();

    // 处理命中后的续飞状态。
    public abstract void ContinueAfterHit();

    // 清理飞行行为持有的运行时状态。
    public abstract void ResetState();
}
