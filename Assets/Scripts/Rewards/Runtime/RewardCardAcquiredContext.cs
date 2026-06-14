using System.Collections.Generic;

/// <summary>
/// 奖励卡牌生效上下文，负责把本次选卡结果传递给 UI、音效和反馈表现层。
/// </summary>
public class RewardCardAcquiredContext
{
    public RewardCardSo RewardCard { get; }
    public RewardCardAcquisitionRecord AcquisitionRecord { get; }
    public IReadOnlyList<RewardCardAcquisitionRecord> AllRecordList { get; }
    public int TotalCardCount { get; }
    public string LatestDescriptionText { get; }

    public RewardCardAcquiredContext(RewardCardSo rewardCard, RewardCardAcquisitionRecord acquisitionRecord, IReadOnlyList<RewardCardAcquisitionRecord> allRecordList, int totalCardCount)
    {
        RewardCard = rewardCard;
        AcquisitionRecord = acquisitionRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
        LatestDescriptionText = acquisitionRecord != null ? acquisitionRecord.DescriptionText : string.Empty;
    }
}
