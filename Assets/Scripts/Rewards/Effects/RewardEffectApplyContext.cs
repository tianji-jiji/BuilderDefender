/// <summary>
/// Reward 效果运行时上下文，负责把 Handler 需要的本局系统引用集中传入。
/// </summary>
public sealed class RewardEffectApplyContext
{
    public RewardRuntimeStateManager RewardRuntimeStateManager { get; }
    public DefenseTowerRewardState DefenseTowerRewardState { get; }
    public DefenseTowerRuntimeEffectDispatcher DefenseTowerRuntimeEffectDispatcher { get; }
    public ResourceManager ResourceManager { get; }
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildingPlacementManager BuildingPlacementManager { get; }

    // 创建 Reward 效果运行时上下文。
    public RewardEffectApplyContext(
        RewardRuntimeStateManager rewardRuntimeStateManager,
        DefenseTowerRewardState defenseTowerRewardState,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildingPlacementManager buildingPlacementManager,
        DefenseTowerRuntimeEffectDispatcher defenseTowerRuntimeEffectDispatcher = null)
    {
        RewardRuntimeStateManager = rewardRuntimeStateManager;
        DefenseTowerRewardState = defenseTowerRewardState;
        DefenseTowerRuntimeEffectDispatcher = defenseTowerRuntimeEffectDispatcher;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildingPlacementManager = buildingPlacementManager;
    }
}
