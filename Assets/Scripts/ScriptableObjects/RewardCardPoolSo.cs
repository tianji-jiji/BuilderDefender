using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardPoolSo")]
public class RewardCardPoolSo : ScriptableObject
{
    private const int DEFAULT_CHOICE_COUNT = 3;
    private const int EARLY_RARE_UNLOCK_WAVE = 6;
    private const int BUILD_UNLOCK_WAVE = 11;

    [SerializeField] private List<RewardCardSo> rewardCardList = new();
    [SerializeField] private int choiceCount = DEFAULT_CHOICE_COUNT;
    [SerializeField] private bool allowDuplicate;

    public int ChoiceCount => Mathf.Max(1, choiceCount);
    public bool AllowDuplicate => allowDuplicate;
    public IReadOnlyList<RewardCardSo> RewardCardList => rewardCardList;

    // 按权重从卡池中抽取本次可以展示的奖励卡。
    public List<RewardCardSo> DrawCards()
    {
        return DrawCards(ChoiceCount, RewardOfferContext.Default(0));
    }

    // 按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(int count)
    {
        return DrawCards(count, RewardOfferContext.Default(0));
    }

    // 根据抽卡上下文按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(RewardOfferContext context)
    {
        return DrawCards(ChoiceCount, context);
    }

    // 根据抽卡上下文按指定数量从卡池中抽取奖励卡。
    public List<RewardCardSo> DrawCards(int count, RewardOfferContext context)
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
    private List<RewardCardSo> BuildAvailableCardList(RewardOfferContext context)
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
    private RewardCardSo PickWeightedCard(IReadOnlyList<RewardCardSo> availableCardList, RewardOfferContext context)
    {
        int totalWeight = 0;

        foreach (RewardCardSo rewardCard in availableCardList)
        {
            if (!rewardCard)
            {
                continue;
            }

            totalWeight += GetContextualWeight(rewardCard, context.CurrentWaveIndex);
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

            accumulatedWeight += GetContextualWeight(rewardCard, context.CurrentWaveIndex);
            if (roll < accumulatedWeight)
            {
                return rewardCard;
            }
        }

        return null;
    }

    // 判断卡牌在当前上下文中是否可以进入抽取池。
    private bool IsCardAvailable(RewardCardSo rewardCard, RewardOfferContext context)
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

    // 根据波次和稀有度计算本次抽取权重。
    private int GetContextualWeight(RewardCardSo rewardCard, int currentWaveIndex)
    {
        if (!rewardCard)
        {
            return 0;
        }

        float rarityMultiplier = GetRarityWeightMultiplier(rewardCard.Rarity, currentWaveIndex);
        return Mathf.Max(0, Mathf.RoundToInt(rewardCard.Weight * rarityMultiplier));
    }

    // 获取当前波次下指定稀有度的权重倍率。
    private float GetRarityWeightMultiplier(RewardCardRarity rarity, int currentWaveIndex)
    {
        switch (rarity)
        {
            case RewardCardRarity.Rare:
                return currentWaveIndex < EARLY_RARE_UNLOCK_WAVE ? 0.35f : 1.6f;
            case RewardCardRarity.Epic:
                return currentWaveIndex < BUILD_UNLOCK_WAVE ? 0.15f : 1.25f;
            case RewardCardRarity.Legendary:
                return currentWaveIndex < BUILD_UNLOCK_WAVE ? 0f : 0.8f;
            default:
                return currentWaveIndex < EARLY_RARE_UNLOCK_WAVE ? 1.4f : 1f;
        }
    }
}
