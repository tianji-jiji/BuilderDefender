/// <summary>
/// 箭矢完成一次命中后的只读处理结果，描述投射物是否继续飞行。
/// </summary>
public readonly struct ArrowHitResolution
{
    public bool ShouldContinueFlight { get; }

    // 创建一次箭矢命中处理结果。
    public ArrowHitResolution(bool shouldContinueFlight)
    {
        ShouldContinueFlight = shouldContinueFlight;
    }
}
