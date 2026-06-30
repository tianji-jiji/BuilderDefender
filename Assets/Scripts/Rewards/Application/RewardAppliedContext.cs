using System.Collections.Generic;

/// <summary>
/// 奖励卡牌生效上下文，负责传递当前奖励卡牌与历史记录。
/// </summary>
public sealed class RewardAppliedContext
{
    public RewardCardSo RewardCard { get; }
    public RewardCardRecord AppliedRecord { get; }
    public IReadOnlyList<RewardCardRecord> AllRecordList { get; }
    public int TotalCardCount { get; }

    // 创建奖励卡牌生效上下文。
    public RewardAppliedContext(
        RewardCardSo rewardCard,
        RewardCardRecord appliedRecord,
        IReadOnlyList<RewardCardRecord> allRecordList,
        int totalCardCount)
    {
        RewardCard = rewardCard;
        AppliedRecord = appliedRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
    }
}
