/// <summary>
/// 基地奖励运行时，集中持有基地类生效奖励和触发器。
/// </summary>
public class HomeRewardRuntime
{
    public HomeActiveRewards ActiveRewards { get; } = new();
    public HomeRewardTriggerDispatcher TriggerDispatcher { get; } = new();
}
