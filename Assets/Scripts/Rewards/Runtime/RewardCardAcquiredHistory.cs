using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本局玩家获得过哪些奖励卡的历史管理器
/// </summary>
public class RewardCardAcquiredHistory : MonoBehaviour
{
    public static RewardCardAcquiredHistory Instance { get; private set; }

    // 当玩家选择的奖励卡牌已经被记录并生效时触发。
    public static event Action<RewardCardAcquiredContext> OnRewardCardApplied;

    private readonly List<RewardCardAcquiredRecord> _rewardCardRecordList = new();
    private readonly Dictionary<string, RewardCardAcquiredRecord> _recordByCardIdDic = new();

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 记录玩家选择的一张奖励卡牌并广播奖励卡牌生效上下文。
    public RewardCardAcquiredContext RecordRewardCard(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return null;
        }

        RewardCardAcquiredRecord acquiredRecord = GetOrCreateRecord(rewardCard);
        RewardCardAcquiredContext context = new(rewardCard, acquiredRecord, GetRecordList(), GetTotalCardCount());
        OnRewardCardApplied?.Invoke(context);
        return context;
    }

    // 获取当前全部奖励卡牌选择记录。
    public IReadOnlyList<RewardCardAcquiredRecord> GetRecordList()
    {
        return _rewardCardRecordList.AsReadOnly();
    }

    // 构建当前波次使用的奖励卡牌抽取上下文。
    public RewardCardDrawContext BuildRewardCardOfferContext()
    {
        return new RewardCardDrawContext(GetCurrentWaveIndex(), BuildSelectedCardCountDic());
    }

    // 清空当前全部奖励卡牌选择记录。
    public void ClearHistory()
    {
        _rewardCardRecordList.Clear();
        _recordByCardIdDic.Clear();
    }

    // 获取或创建指定卡牌对应的选择记录。
    private RewardCardAcquiredRecord GetOrCreateRecord(RewardCardSo rewardCard)
    {
        string cardId = GetStableCardId(rewardCard);
        if (_recordByCardIdDic.TryGetValue(cardId, out RewardCardAcquiredRecord existingRecord))
        {
            existingRecord.AddStack();
            return existingRecord;
        }

        RewardCardAcquiredRecord newRecord = new(
            cardId,
            rewardCard.CardName,
            rewardCard.Rarity,
            rewardCard.Category,
            RewardCardDescriptionFormatter.BuildDescriptionText(rewardCard),
            GetCurrentWaveIndex());

        _recordByCardIdDic.Add(cardId, newRecord);
        _rewardCardRecordList.Add(newRecord);
        return newRecord;
    }

    // 获取卡牌稳定 ID，缺失时使用资源名兜底。
    private string GetStableCardId(RewardCardSo rewardCard)
    {
        if (!string.IsNullOrWhiteSpace(rewardCard.CardId))
        {
            return rewardCard.CardId;
        }

        return rewardCard.name;
    }

    // 获取当前奖励获得时的波次。
    private int GetCurrentWaveIndex()
    {
        return EnemyWaveManager.Instance ? EnemyWaveManager.Instance.waveIndex : 0;
    }

    // 构建每张卡牌已经被选择过的次数索引。
    private Dictionary<string, int> BuildSelectedCardCountDic()
    {
        Dictionary<string, int> selectedCardCountDic = new Dictionary<string, int>();
        foreach (RewardCardAcquiredRecord rewardCardRecord in _rewardCardRecordList)
        {
            if (rewardCardRecord == null || string.IsNullOrWhiteSpace(rewardCardRecord.CardId))
            {
                continue;
            }

            selectedCardCountDic[rewardCardRecord.CardId] = rewardCardRecord.StackCount;
        }

        return selectedCardCountDic;
    }

    // 统计玩家本局累计选择过的卡牌次数。
    private int GetTotalCardCount()
    {
        int totalCardCount = 0;
        foreach (RewardCardAcquiredRecord rewardCardRecord in _rewardCardRecordList)
        {
            totalCardCount += rewardCardRecord.StackCount;
        }

        return totalCardCount;
    }
}
