using System;
using System.Collections.Generic;

/// <summary>
/// 奖励卡牌抽取服务，负责根据卡池配置和运行时上下文筛选并加权抽取卡牌。
/// </summary>
public sealed class RewardCardDrawService
{
    private readonly IRewardRandom _random;

    // 注入奖励抽取使用的随机数实现。
    public RewardCardDrawService(IRewardRandom random)
    {
        _random = random;
    }

    // 根据卡池配置和抽取上下文生成本次奖励卡牌选项。
    public IReadOnlyList<RewardCardSo> Draw(RewardCardPoolSo pool, RewardCardDrawContext context)
    {
        if (!pool || _random == null || pool.RewardCardList == null || pool.RewardCardList.Count <= 0)
        {
            return Array.Empty<RewardCardSo>();
        }

        List<RewardCardSo> availableCardList = BuildAvailableCardList(pool.RewardCardList, context);
        if (availableCardList.Count <= 0)
        {
            return Array.Empty<RewardCardSo>();
        }

        List<RewardCardSo> resultList = new();
        while (resultList.Count < pool.ChoiceCount && availableCardList.Count > 0)
        {
            RewardCardSo selectedCard = PickWeightedCard(availableCardList);
            if (!selectedCard)
            {
                break;
            }

            resultList.Add(selectedCard);
            if (!pool.AllowDuplicate)
            {
                availableCardList.Remove(selectedCard);
            }
        }

        return resultList;
    }

    // 收集当前可以参与抽取的有效卡牌。
    private static List<RewardCardSo> BuildAvailableCardList(
        IReadOnlyList<RewardCardSo> rewardCardList,
        RewardCardDrawContext context)
    {
        List<RewardCardSo> availableCardList = new();
        foreach (RewardCardSo rewardCard in rewardCardList)
        {
            if (IsCardAvailable(rewardCard, context))
            {
                availableCardList.Add(rewardCard);
            }
        }

        return availableCardList;
    }

    // 根据权重从候选列表中选择一张卡牌。
    private RewardCardSo PickWeightedCard(IReadOnlyList<RewardCardSo> availableCardList)
    {
        int totalWeight = 0;
        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (rewardCard)
            {
                totalWeight += rewardCard.Weight;
            }
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = _random.Range(0, totalWeight);
        int accumulatedWeight = 0;
        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (!rewardCard)
            {
                continue;
            }

            accumulatedWeight += rewardCard.Weight;
            if (roll < accumulatedWeight)
            {
                return rewardCard;
            }
        }

        return null;
    }

    // 判断卡牌在当前上下文中是否可以进入抽取池。
    private static bool IsCardAvailable(RewardCardSo rewardCard, RewardCardDrawContext context)
    {
        if (!rewardCard || rewardCard.Weight <= 0 || !rewardCard.CardPrefab)
        {
            return false;
        }

        if (context.CurrentWaveIndex < rewardCard.MinWaveIndex)
        {
            return false;
        }

        return context.GetSelectedCount(rewardCard) < rewardCard.MaxPickCount;
    }
}
