using System.Collections.Generic;

/// <summary>
/// 奖励抽取时的运行时上下文，负责提供当前波次和历史选择次数。
/// </summary>
public readonly struct RewardOfferContext
{
    public int CurrentWaveIndex { get; }
    private readonly IReadOnlyDictionary<string, int> _selectedCardCountDic;

    // 保存本次奖励抽取需要参考的上下文。
    public RewardOfferContext(int currentWaveIndex, IReadOnlyDictionary<string, int> selectedCardCountDic)
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

    // 创建没有历史记录的默认抽卡上下文。
    public static RewardOfferContext Default(int currentWaveIndex)
    {
        return new RewardOfferContext(currentWaveIndex, null);
    }
}
