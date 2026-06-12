using System.Collections.Generic;

/// <summary>
/// 单次获得的防御塔卡牌运行时效果实例，保存配置和独立计数器状态。
/// </summary>
public class DefenseCardEffectInstance
{
    private readonly Dictionary<string, int> _counterDic = new();

    public IDefenseCardEffect Effect { get; }
    public RewardEffectConfig Config { get; }

    // 创建一条运行时效果实例。
    public DefenseCardEffectInstance(IDefenseCardEffect effect, RewardEffectConfig config)
    {
        Effect = effect;
        Config = config;
    }

    // 增加指定计数器并返回增加后的值。
    public int IncrementCounter(string counterId)
    {
        string safeCounterId = string.IsNullOrWhiteSpace(counterId) ? "Default" : counterId;
        _counterDic.TryGetValue(safeCounterId, out int counter);
        counter++;
        _counterDic[safeCounterId] = counter;
        return counter;
    }

    // 重置指定计数器。
    public void ResetCounter(string counterId)
    {
        string safeCounterId = string.IsNullOrWhiteSpace(counterId) ? "Default" : counterId;
        _counterDic[safeCounterId] = 0;
    }
}
