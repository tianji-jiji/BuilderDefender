using System.Collections.Generic;

/// <summary>
/// 奖励卡牌生效上下文，负责把本次选卡结果传递给 UI、音效和反馈表现层。
/// </summary>
public class RewardCardAppliedContext
{
    public RewardCardSo RewardCard { get; }
    public RewardCardSelectionRecord SelectionRecord { get; }
    public IReadOnlyList<RewardCardSelectionRecord> AllRecordList { get; }
    public int TotalCardCount { get; }
    public string LatestDescriptionText { get; }

    public RewardCardAppliedContext(RewardCardSo rewardCard, RewardCardSelectionRecord selectionRecord, IReadOnlyList<RewardCardSelectionRecord> allRecordList, int totalCardCount)
    {
        RewardCard = rewardCard;
        SelectionRecord = selectionRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
        LatestDescriptionText = selectionRecord != null ? selectionRecord.DescriptionText : string.Empty;
    }
}
