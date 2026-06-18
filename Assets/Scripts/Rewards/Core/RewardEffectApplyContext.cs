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
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }
    public DefenseTowerActiveRewards DefenseTowerActiveRewards => DefenseTowerRewardRuntime?.ActiveRewards;

    // 创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        DefenseTowerRewardRuntime defenseTowerRewardRuntime,
        ResourceRewardRuntime resourceRewardRuntime,
        HomeRewardRuntime homeRewardRuntime,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager)
    {
        RewardRuntimeCoordinator = rewardRuntimeCoordinator;
        DefenseTowerRewardRuntime = defenseTowerRewardRuntime;
        ResourceRewardRuntime = resourceRewardRuntime;
        HomeRewardRuntime = homeRewardRuntime;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }
    

    // 使用旧防御塔账本和触发器参数创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        DefenseTowerActiveRewards defenseTowerActiveRewards,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager,
        DefenseTowerRewardTriggerDispatcher defenseTowerRewardTriggerDispatcher)
        : this(
            rewardRuntimeCoordinator,
            new DefenseTowerRewardRuntime(defenseTowerActiveRewards, defenseTowerRewardTriggerDispatcher),
            new ResourceRewardRuntime(),
            new HomeRewardRuntime(),
            resourceManager,
            enemyWaveManager,
            buildingPlacementManager)
    {
    }
}
