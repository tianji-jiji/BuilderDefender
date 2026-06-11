/// <summary>
/// Reward 鏁堟灉杩愯鏃朵笂涓嬫枃锛岃礋璐ｆ妸 Handler 闇€瑕佺殑鏈眬绯荤粺寮曠敤闆嗕腑浼犲叆銆?/// </summary>
public sealed class RewardEffectContext
{
    public RewardBonusManager RewardBonusManager { get; }
    public DefenseRewardState DefenseRewardState { get; }
    public ResourceManager ResourceManager { get; }
    public EnemyWaveManager EnemyWaveManager { get; }
    public BuildManager BuildManager { get; }

    // 鍒涘缓 Reward 鏁堟灉杩愯鏃朵笂涓嬫枃銆?
    public RewardEffectContext(
        RewardBonusManager rewardBonusManager,
        DefenseRewardState defenseRewardState,
        ResourceManager resourceManager,
        EnemyWaveManager enemyWaveManager,
        BuildManager buildManager)
    {
        RewardBonusManager = rewardBonusManager;
        DefenseRewardState = defenseRewardState;
        ResourceManager = resourceManager;
        EnemyWaveManager = enemyWaveManager;
        BuildManager = buildManager;
    }
}
