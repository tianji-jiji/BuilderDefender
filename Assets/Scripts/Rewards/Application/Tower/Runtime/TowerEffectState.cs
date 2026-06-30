using System.Collections.Generic;

/// <summary>
/// 单个 Tower 运行时奖励效果的配置、场景入口和分 Tower 计数状态。
/// </summary>
public sealed class TowerEffectState
{
    private readonly Dictionary<TowerCombatSystem, Dictionary<string, int>> _towerCounterDic = new();

    public RewardCardEffectConfig Config { get; }
    public ITowerRewardWorld World { get; }

    // 创建运行时奖励效果状态。
    public TowerEffectState(RewardCardEffectConfig config, ITowerRewardWorld world)
    {
        Config = config;
        World = world;
    }

    // 增加指定防御塔的奖励计数并返回增加后的值。
    public int IncrementCounter(TowerCombatSystem sourceTowerCombatSystem, string counterId)
    {
        if (!sourceTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return 0;
        }

        if (!_towerCounterDic.TryGetValue(sourceTowerCombatSystem, out Dictionary<string, int> counterDic))
        {
            counterDic = new();
            _towerCounterDic.Add(sourceTowerCombatSystem, counterDic);
        }

        counterDic.TryGetValue(counterId, out int counter);
        counter++;
        counterDic[counterId] = counter;
        return counter;
    }

    // 重置指定防御塔的一项奖励计数。
    public void ResetCounter(TowerCombatSystem sourceTowerCombatSystem, string counterId)
    {
        if (!sourceTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return;
        }

        if (_towerCounterDic.TryGetValue(sourceTowerCombatSystem, out Dictionary<string, int> counterDic))
        {
            counterDic[counterId] = 0;
        }
    }

    // 清理指定防御塔在当前奖励效果中的全部计数。
    public void ClearCounters(TowerCombatSystem sourceTowerCombatSystem)
    {
        if (sourceTowerCombatSystem)
        {
            _towerCounterDic.Remove(sourceTowerCombatSystem);
        }
    }
}
