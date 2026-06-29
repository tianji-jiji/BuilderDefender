/// <summary>
/// Reward 效果应用上下文，负责把 Applier 需要的本局系统和奖励运行时引用集中传入。
/// </summary>
public sealed class RewardEffectApplyContext
{
    public RewardRuntimeCoordinator RewardRuntimeCoordinator { get; }
    public TowerRewardRuntime TowerRewardRuntime { get; }
    public ResourceManager ResourceManager { get; }
    public WaveManager WaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }
    public TowerActiveRewards TowerActiveRewards => TowerRewardRuntime?.ActiveRewards;

    // 创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        TowerRewardRuntime towerRewardRuntime,
        ResourceManager resourceManager,
        WaveManager waveManager,
        BuildingPlacementManager buildingPlacementManager)
    {
        RewardRuntimeCoordinator = rewardRuntimeCoordinator;
        TowerRewardRuntime = towerRewardRuntime;
        ResourceManager = resourceManager;
        WaveManager = waveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }
}
