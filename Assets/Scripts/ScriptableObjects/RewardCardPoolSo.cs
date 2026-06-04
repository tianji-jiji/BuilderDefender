using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RewardCardPoolSo")]
public class RewardCardPoolSo : ScriptableObject
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
        return DrawCards(ChoiceCount);
    }

    // 按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(int count)
    {
        List<RewardCardSo> resultList = new List<RewardCardSo>();
        List<RewardCardSo> availableCardList = BuildAvailableCardList();
        int targetCount = Mathf.Max(1, count);

        while (resultList.Count < targetCount && availableCardList.Count > 0)
        {
            RewardCardSo selectedCard = PickWeightedCard(availableCardList);
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
    private List<RewardCardSo> BuildAvailableCardList()
    {
        List<RewardCardSo> availableCardList = new List<RewardCardSo>();

        foreach (RewardCardSo rewardCard in rewardCardList)
        {
            if (!rewardCard || rewardCard.Weight <= 0 || !rewardCard.CardPrefab)
            {
                continue;
            }

            availableCardList.Add(rewardCard);
        }

        return availableCardList;
    }

    // 根据权重从候选列表中选择一张卡。
    private RewardCardSo PickWeightedCard(IReadOnlyList<RewardCardSo> availableCardList)
    {
        int totalWeight = 0;

        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (!rewardCard)
            {
                continue;
            }

            totalWeight += rewardCard.Weight;
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

            accumulatedWeight += rewardCard.Weight;
            if (roll < accumulatedWeight)
            {
                return rewardCard;
            }
        }

        return null;
    }
}
