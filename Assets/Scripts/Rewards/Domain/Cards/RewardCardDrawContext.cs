using System.Collections.Generic;

/// <summary>
/// 奖励卡牌抽取时的运行时上下文，负责提供当前波次和历史选择次数。
/// </summary>
public readonly struct RewardCardDrawContext
{
    public int CurrentWaveIndex { get; }
    private readonly IReadOnlyDictionary<string, int> _selectedCardCountDic;

    public RewardCardDrawContext(int currentWaveIndex, IReadOnlyDictionary<string, int> selectedCardCountDic)
    {
        CurrentWaveIndex = currentWaveIndex;
        _selectedCardCountDic = selectedCardCountDic;
    }

    // 查询指定卡牌已经被选择过的次数。
    public int GetSelectedCount(RewardCardSo rewardCard)
    {
        if (!rewardCard || _selectedCardCountDic == null)
        {
            return 0;
        }

        string cardId = !string.IsNullOrWhiteSpace(rewardCard.CardId) ? rewardCard.CardId : rewardCard.name;
        return _selectedCardCountDic.GetValueOrDefault(cardId, 0);
    }
}
