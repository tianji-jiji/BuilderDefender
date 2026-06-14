using System.Collections.Generic;

/// <summary>
/// 奖励生效上下文，负责把本次选卡结果传递给 UI、音效和反馈表现层。
/// </summary>
public class RewardAppliedContext
{
    public RewardCardSo RewardCard { get; }
    public RewardSelectionRecord SelectionRecord { get; }
    public IReadOnlyList<RewardSelectionRecord> AllRecordList { get; }
    public int TotalCardCount { get; }
    public string LatestDescriptionText { get; }

    public RewardAppliedContext(RewardCardSo rewardCard, RewardSelectionRecord selectionRecord, IReadOnlyList<RewardSelectionRecord> allRecordList, int totalCardCount)
    {
        RewardCard = rewardCard;
        SelectionRecord = selectionRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
        LatestDescriptionText = selectionRecord != null ? selectionRecord.DescriptionText : string.Empty;
    }
}
