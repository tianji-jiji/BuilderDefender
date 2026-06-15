/// <summary>
/// Reward 效果运行时上下文，负责把 Handler 需要的本局系统引用集中传入。
/// </summary>
public sealed class RewardEffectApplyContext
{
    public RewardRuntimeStateManager RewardRuntimeStateManager { get; }
    public DefenseTowerActiveRewards DefenseTowerActiveRewards { get; }
    public DefenseTowerRewardTriggerDispatcher DefenseTowerRewardTriggerDispatcher { get; }
    public ResourceManager ResourceManager { get; }
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }

    // 创建 Reward 效果运行时上下文。
    public RewardEffectApplyContext(
        RewardRuntimeStateManager rewardRuntimeStateManager,
        DefenseTowerActiveRewards defenseTowerActiveRewards,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager,
        DefenseTowerRewardTriggerDispatcher defenseTowerRewardTriggerDispatcher = null)
    {
        RewardRuntimeStateManager = rewardRuntimeStateManager;
        DefenseTowerActiveRewards = defenseTowerActiveRewards;
        DefenseTowerRewardTriggerDispatcher = defenseTowerRewardTriggerDispatcher;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }
}
