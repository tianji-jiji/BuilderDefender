using System.Collections.Generic;

/// <summary>
/// 卡牌中某一个“需要运行时参与战斗流程”的效果
/// 例如：额外箭、击杀升星、波末回血
/// </summary>
public class DefenseTowerRuntimeEffectInstance
{
    // 运行时效果用的计数器
    private readonly Dictionary<string, int> _counterDic = new();
    // 这是什么效果逻辑
    public IDefenseTowerRuntimeEffect Effect { get; }
    // 这次效果用的具体参数
    public RewardCardEffectConfig Config { get; }

    public DefenseTowerRuntimeEffectInstance(IDefenseTowerRuntimeEffect effect, RewardCardEffectConfig config)
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
