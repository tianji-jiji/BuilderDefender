/// <summary>
/// Reward 效果应用上下文，负责把 Applier 需要的本局系统和奖励模块引用集中传入。
/// </summary>
public sealed class RewardEffectApplyContext
{
    public RewardRuntimeCoordinator RewardRuntimeCoordinator { get; }
    public DefenseTowerRewardModule DefenseTowerRewardModule { get; }
    public ResourceRewardModule ResourceRewardModule { get; }
    public HomeRewardModule HomeRewardModule { get; }
    public ResourceManager ResourceManager { get; }
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }
    public DefenseTowerActiveRewards DefenseTowerActiveRewards => DefenseTowerRewardModule?.ActiveRewards;
    public DefenseTowerRewardTriggerDispatcher DefenseTowerRewardTriggerDispatcher => DefenseTowerRewardModule?.TriggerDispatcher;

    // 创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        DefenseTowerRewardModule defenseTowerRewardModule,
        ResourceRewardModule resourceRewardModule,
        HomeRewardModule homeRewardModule,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager)
    {
        RewardRuntimeCoordinator = rewardRuntimeCoordinator;
        DefenseTowerRewardModule = defenseTowerRewardModule;
        ResourceRewardModule = resourceRewardModule;
        HomeRewardModule = homeRewardModule;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }

    // 使用旧防御塔账本参数创建 Reward 效果应用上下文。
    public RewardEffectApplyContext(
        RewardRuntimeCoordinator rewardRuntimeCoordinator,
        DefenseTowerActiveRewards defenseTowerActiveRewards,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager)
        : this(
            rewardRuntimeCoordinator,
            new DefenseTowerRewardModule(defenseTowerActiveRewards, null),
            new ResourceRewardModule(),
            new HomeRewardModule(),
            resourceManager,
            enemyWaveManager,
            buildingPlacementManager)
    {
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
            new DefenseTowerRewardModule(defenseTowerActiveRewards, defenseTowerRewardTriggerDispatcher),
            new ResourceRewardModule(),
            new HomeRewardModule(),
            resourceManager,
            enemyWaveManager,
            buildingPlacementManager)
    {
    }
}
