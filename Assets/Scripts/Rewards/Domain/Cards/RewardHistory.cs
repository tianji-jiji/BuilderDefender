using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励历史模型，负责记录已获得的奖励卡牌并构建抽取上下文。
/// </summary>
public sealed class RewardHistory
{
    private readonly List<RewardCardRecord> _recordList = new();
    private readonly Dictionary<string, RewardCardRecord> _recordByCardIdDic = new();

    public IReadOnlyList<RewardCardRecord> RecordList => _recordList;
    public int TotalCardCount
    {
        get
        {
            int totalCardCount = 0;
            foreach (RewardCardRecord rewardCardRecord in _recordList)
            {
                if (rewardCardRecord == null)
                {
                    continue;
                }

                totalCardCount += rewardCardRecord.StackCount;
            }

            return totalCardCount;
        }
    }

    // 记录一张奖励卡牌，并返回对应的历史记录。
    public RewardCardRecord Record(RewardCardSo rewardCard, int waveIndex)
    {
        if (!rewardCard)
        {
            return null;
        }

        string cardId = GetStableCardId(rewardCard);
        if (_recordByCardIdDic.TryGetValue(cardId, out RewardCardRecord existingRecord))
        {
            existingRecord.AddStack();
            return existingRecord;
        }

        RewardCardRecord newRecord = new(rewardCard, waveIndex);
        _recordByCardIdDic.Add(cardId, newRecord);
        _recordList.Add(newRecord);
        return newRecord;
    }

    // 构建用于奖励抽取的上下文。
    public RewardCardDrawContext BuildDrawContext(int currentWaveIndex)
    {
        Dictionary<string, int> selectedCardCountDic = new();
        foreach (RewardCardRecord rewardCardRecord in _recordList)
        {
            if (rewardCardRecord == null || !rewardCardRecord.RewardCard)
            {
                continue;
            }

            string cardId = GetStableCardId(rewardCardRecord.RewardCard);
            selectedCardCountDic[cardId] = rewardCardRecord.StackCount;
        }

        return new RewardCardDrawContext(currentWaveIndex, selectedCardCountDic);
    }

    // 获取卡牌的稳定标识，优先使用 CardId，缺失时回退到资源名。
    private static string GetStableCardId(RewardCardSo rewardCard)
    {
        if (!string.IsNullOrWhiteSpace(rewardCard.CardId))
        {
            return rewardCard.CardId;
        }

        return rewardCard.name;
    }
}
