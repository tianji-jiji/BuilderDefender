using System.Collections.Generic;

/// <summary>
/// 防御塔运行时奖励状态，保存单个奖励效果的配置和各防御塔独立计数。
/// </summary>
public class DefenseTowerRewardRuntimeState
{
    private readonly Dictionary<DefenseTowerCombatSystem, Dictionary<string, int>> _towerCounterDic = new();

    public RewardCardEffectConfig Config { get; }

    public DefenseTowerRewardRuntimeState(RewardCardEffectConfig config)
    {
        Config = config;
    }

    // 增加指定防御塔的奖励计数并返回增加后的值。
    public int IncrementCounter(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, string counterId)
    {
        if (!sourceDefenseTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return 0;
        }

        if (!_towerCounterDic.TryGetValue(sourceDefenseTowerCombatSystem, out Dictionary<string, int> counterDic))
        {
            counterDic = new();
            _towerCounterDic.Add(sourceDefenseTowerCombatSystem, counterDic);
        }

        counterDic.TryGetValue(counterId, out int counter);
        counter++;
        counterDic[counterId] = counter;
        return counter;
    }

    // 重置指定防御塔的一项奖励计数。
    public void ResetCounter(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem, string counterId)
    {
        if (!sourceDefenseTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return;
        }

        if (_towerCounterDic.TryGetValue(sourceDefenseTowerCombatSystem, out Dictionary<string, int> counterDic))
        {
            counterDic[counterId] = 0;
        }
    }

    // 清理指定防御塔在当前奖励效果中的全部计数。
    public void ClearCounters(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        if (sourceDefenseTowerCombatSystem)
        {
            _towerCounterDic.Remove(sourceDefenseTowerCombatSystem);
        }
    }
}
