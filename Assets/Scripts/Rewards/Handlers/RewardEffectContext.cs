/// <summary>
/// Reward 效果运行时上下文，负责把 Handler 需要的本局系统引用集中传入。
/// </summary>
public sealed class RewardEffectContext
{
    public RewardBonusManager RewardBonusManager { get; }
    public DefenseTowerRewardModifiers DefenseTowerRewardModifiers { get; }
    public DefenseTowerCardEffectDispatcher DefenseTowerCardEffectDispatcher { get; }
    public ResourceManager ResourceManager { get; }
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildManager BuildManager { get; }

    // 创建 Reward 效果运行时上下文。
    public RewardEffectContext(
        RewardBonusManager rewardBonusManager,
        DefenseTowerRewardModifiers defenseTowerRewardModifiers,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildManager buildManager,
        DefenseTowerCardEffectDispatcher defenseTowerCardEffectDispatcher = null)
    {
        RewardBonusManager = rewardBonusManager;
        DefenseTowerRewardModifiers = defenseTowerRewardModifiers;
        DefenseTowerCardEffectDispatcher = defenseTowerCardEffectDispatcher;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildManager = buildManager;
    }
}
