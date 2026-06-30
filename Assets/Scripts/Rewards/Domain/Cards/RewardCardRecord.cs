/// <summary>
/// 奖励卡牌历史记录，负责保存单张卡牌的首次获得波次与叠加次数。
/// </summary>
public sealed class RewardCardRecord
{
    public RewardCardSo RewardCard { get; }
    public int AcquiredWaveIndex { get; }
    public int StackCount { get; private set; }

    // 创建一条新的奖励卡牌历史记录。
    public RewardCardRecord(RewardCardSo rewardCard, int acquiredWaveIndex)
    {
        RewardCard = rewardCard;
        AcquiredWaveIndex = acquiredWaveIndex;
        StackCount = 1;
    }

    // 增加同一张奖励卡牌的叠加次数。
    public void AddStack()
    {
        StackCount++;
    }
}
