/// <summary>
/// 奖励选择记录，负责保存玩家已经获得的一张卡牌及其可展示状态。
/// </summary>
public class RewardSelectionRecord
{
    public string CardId { get; }
    public string CardName { get; }
    public RewardCardRarity Rarity { get; }
    public RewardCardCategory Category { get; }
    public string DescriptionText { get; }
    public int AcquiredWaveIndex { get; }
    public int StackCount { get; private set; }

    public RewardSelectionRecord(string cardId, string cardName, RewardCardRarity rarity, RewardCardCategory category, string descriptionText, int acquiredWaveIndex)
    {
        CardId = cardId;
        CardName = cardName;
        Rarity = rarity;
        Category = category;
        DescriptionText = descriptionText;
        AcquiredWaveIndex = acquiredWaveIndex;
        StackCount = 1;
    }

    // 增加同一张奖励卡的叠加次数。
    public void AddStack()
    {
        StackCount++;
    }
}
