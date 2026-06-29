/// <summary>
/// Reward 效果应用上下文，负责把 Applier 需要的本局系统和奖励运行时引用集中传入。
/// </summary>
public sealed class RewardEffectApplyContext
{
    public RewardRuntimeCoordinator RewardRuntimeCoordinator { get; }
    public DefenseTowerRewardRuntime DefenseTowerRewardRuntime { get; }
    public ResourceRewardRuntime ResourceRewardRuntime { get; }
    public HomeRewardRuntime HomeRewardRuntime { get; }
    public ResourceManager ResourceManager { get; }
    public WaveManager WaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }
    public DefenseTowerActiveRewards DefenseTowerActiveRewards => DefenseTowerRewardRuntime?.ActiveRewards;

    // 创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        DefenseTowerRewardRuntime defenseTowerRewardRuntime,
        ResourceRewardRuntime resourceRewardRuntime,
        HomeRewardRuntime homeRewardRuntime,
        ResourceManager resourceManager,
        WaveManager waveManager,
        BuildingPlacementManager buildingPlacementManager)
    {
        RewardRuntimeCoordinator = rewardRuntimeCoordinator;
        DefenseTowerRewardRuntime = defenseTowerRewardRuntime;
        ResourceRewardRuntime = resourceRewardRuntime;
        HomeRewardRuntime = homeRewardRuntime;
        ResourceManager = resourceManager;
        WaveManager = waveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }
}
