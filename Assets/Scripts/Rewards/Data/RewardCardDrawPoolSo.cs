using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardPoolSo")]
public class RewardCardDrawPoolSo : ScriptableObject
{
    private const int DEFAULT_CHOICE_COUNT = 3;

    [SerializeField] private List<RewardCardSo> rewardCardList = new();
    [SerializeField] private int choiceCount = DEFAULT_CHOICE_COUNT;
    [SerializeField] private bool allowDuplicate;

    public int ChoiceCount => Mathf.Max(1, choiceCount);
    public bool AllowDuplicate => allowDuplicate;
    public IReadOnlyList<RewardCardSo> RewardCardList => rewardCardList;

    // 按权重从卡池中抽取本次可以展示的奖励卡。
    public List<RewardCardSo> DrawCards()
    {
        return DrawCards(ChoiceCount, RewardCardDrawContext.Default(0));
    }

    // 按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(int count)
    {
        return DrawCards(count, RewardCardDrawContext.Default(0));
    }

    // 根据抽卡上下文按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(RewardCardDrawContext context)
    {
        return DrawCards(ChoiceCount, context);
    }

    // 根据抽卡上下文按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(int count, RewardCardDrawContext context)
    {
        List<RewardCardSo> resultList = new List<RewardCardSo>();
        List<RewardCardSo> availableCardList = BuildAvailableCardList(context);
        int targetCount = Mathf.Max(1, count);

        while (resultList.Count < targetCount && availableCardList.Count > 0)
        {
            RewardCardSo selectedCard = PickWeightedCard(availableCardList, context);
            if (!selectedCard)
            {
                break;
            }

            resultList.Add(selectedCard);

            if (!allowDuplicate)
            {
                availableCardList.Remove(selectedCard);
            }
        }

        return resultList;
    }

    // 收集当前可以参与抽取的有效卡牌。
    private List<RewardCardSo> BuildAvailableCardList(RewardCardDrawContext context)
    {
        List<RewardCardSo> availableCardList = new List<RewardCardSo>();

        foreach (RewardCardSo rewardCard in rewardCardList)
        {
            if (!IsCardAvailable(rewardCard, context))
            {
                continue;
            }

            availableCardList.Add(rewardCard);
        }

        return availableCardList;
    }

    // 根据权重从候选列表中选择一张卡。
    private RewardCardSo PickWeightedCard(IReadOnlyList<RewardCardSo> availableCardList, RewardCardDrawContext context)
    {
        int totalWeight = 0;

        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (!rewardCard)
            {
                continue;
            }

            totalWeight += GetDrawWeight(rewardCard);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (!rewardCard)
            {
                continue;
            }

            accumulatedWeight += GetDrawWeight(rewardCard);
            if (roll < accumulatedWeight)
            {
                return rewardCard;
            }
        }

        return null;
    }

    // 判断卡牌在当前上下文中是否可以进入抽取池。
    private bool IsCardAvailable(RewardCardSo rewardCard, RewardCardDrawContext context)
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

    // 根据卡牌配置计算本次抽取权重。
    private int GetDrawWeight(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return 0;
        }

        return rewardCard.Weight;
    }
}
