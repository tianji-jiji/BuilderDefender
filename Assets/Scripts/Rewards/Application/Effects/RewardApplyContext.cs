/// <summary>
/// 奖励效果应用上下文，提供效果执行所需的奖励运行时依赖。
/// </summary>
public sealed class RewardApplyContext
{
    public TowerRewardRuntime TowerRewardRuntime { get; }
    public TowerRewardState TowerRewardState => TowerRewardRuntime?.State;
    public ITowerRewardWorld TowerWorld => TowerRewardRuntime?.World;

    // 创建奖励效果应用上下文。
    public RewardApplyContext(TowerRewardRuntime towerRewardRuntime)
    {
        TowerRewardRuntime = towerRewardRuntime;
    }
}
