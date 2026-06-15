/// <summary>
/// 资源奖励运行时模块，集中持有资源类生效奖励和触发器。
/// </summary>
public class ResourceRewardModule
{
    public ResourceActiveRewards ActiveRewards { get; } = new();
    public ResourceRewardTriggerDispatcher TriggerDispatcher { get; } = new();
}
