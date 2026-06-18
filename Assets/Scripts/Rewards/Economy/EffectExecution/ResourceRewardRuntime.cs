/// <summary>
/// 资源奖励运行时，集中持有资源类生效奖励和触发器。
/// </summary>
public class ResourceRewardRuntime
{
    public ResourceActiveRewards ActiveRewards { get; } = new();
    public ResourceRewardTriggerDispatcher TriggerDispatcher { get; } = new();
}
