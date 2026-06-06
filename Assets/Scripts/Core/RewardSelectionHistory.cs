using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励选择历史，负责记录本局玩家已经选择的卡牌并广播可感知的奖励生效事件。
/// </summary>
public class RewardSelectionHistory : MonoBehaviour
{
    public static RewardSelectionHistory Instance { get; private set; }

    // 当玩家选择的奖励已经被记录并生效时触发。
    public static event Action<RewardAppliedContext> OnRewardApplied;

    private readonly List<RewardSelectionRecord> _rewardRecordList = new();
    private readonly Dictionary<string, RewardSelectionRecord> _recordByCardIdDic = new();

    // 初始化奖励选择历史单例。
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 清理奖励选择历史单例引用。
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 记录玩家选择的一张奖励卡并广播奖励生效上下文。
    public RewardAppliedContext RecordReward(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return null;
        }

        RewardSelectionRecord selectionRecord = GetOrCreateRecord(rewardCard);
        RewardAppliedContext context = new(rewardCard, selectionRecord, GetRecordList(), GetTotalCardCount());
        OnRewardApplied?.Invoke(context);
        return context;
    }

    // 获取当前全部奖励选择记录。
    public IReadOnlyList<RewardSelectionRecord> GetRecordList()
    {
        return _rewardRecordList.AsReadOnly();
    }

    // 清空当前全部奖励选择记录。
    public void ClearHistory()
    {
        _rewardRecordList.Clear();
        _recordByCardIdDic.Clear();
    }

    // 获取或创建指定卡牌对应的选择记录。
    private RewardSelectionRecord GetOrCreateRecord(RewardCardSo rewardCard)
    {
        string cardId = GetStableCardId(rewardCard);
        if (_recordByCardIdDic.TryGetValue(cardId, out RewardSelectionRecord existingRecord))
        {
            existingRecord.AddStack();
            return existingRecord;
        }

        RewardSelectionRecord newRecord = new(
            cardId,
            rewardCard.CardName,
            rewardCard.Rarity,
            rewardCard.Category,
            RewardCardDescriptionFormatter.BuildDescriptionText(rewardCard),
            GetCurrentWaveIndex());

        _recordByCardIdDic.Add(cardId, newRecord);
        _rewardRecordList.Add(newRecord);
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

    // 统计玩家本局累计选择过的卡牌次数。
    private int GetTotalCardCount()
    {
        int totalCardCount = 0;
        foreach (RewardSelectionRecord rewardRecord in _rewardRecordList)
        {
            totalCardCount += rewardRecord.StackCount;
        }

        return totalCardCount;
    }
}
