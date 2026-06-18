using System.Collections.Generic;

/// <summary>
/// 奖励卡牌生效上下文，负责把本次选卡结果传递给 UI、音效和反馈表现层。
/// </summary>
public class RewardCardAcquiredContext
{
    public RewardCardSo RewardCard { get; }
    public RewardCardAcquiredRecord AcquiredRecord { get; }
    public IReadOnlyList<RewardCardAcquiredRecord> AllRecordList { get; }
    public int TotalCardCount { get; }
    public string LatestDescriptionText { get; }

    public RewardCardAcquiredContext(RewardCardSo rewardCard, RewardCardAcquiredRecord acquiredRecord, IReadOnlyList<RewardCardAcquiredRecord> allRecordList, int totalCardCount)
    {
        RewardCard = rewardCard;
        AcquiredRecord = acquiredRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
        LatestDescriptionText = acquiredRecord != null ? acquiredRecord.DescriptionText : string.Empty;
    }
}
