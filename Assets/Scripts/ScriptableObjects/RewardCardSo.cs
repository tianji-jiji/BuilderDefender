using System.Collections.Generic;
using UnityEngine;

public enum RewardCardRarity
{
    Normal,
    Rare,
    Epic,
    Legendary
}

public enum RewardCardCategory
{
    Defense,
    Resources,
    Home,
    Risk
}

/// <summary>
/// 奖励卡牌数据资产，负责配置单张卡牌的基础信息、显示预制体和奖励效果。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardSo")]
public class RewardCardSo : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int weight = 1;
    [SerializeField] private RewardCardRarity rarity = RewardCardRarity.Normal;
    [SerializeField] private RewardCardCategory category = RewardCardCategory.Defense;
    [SerializeField] private List<RewardEffectConfig> effectConfigList = new List<RewardEffectConfig>();

    public string CardId => cardId;
    public string CardName => string.IsNullOrWhiteSpace(cardName) ? cardId : cardName;
    public GameObject CardPrefab => cardPrefab;
    public int Weight => Mathf.Max(0, weight);
    public RewardCardRarity Rarity => rarity;
    public RewardCardCategory Category => category;
    public IReadOnlyList<RewardEffectConfig> EffectConfigList => effectConfigList;
}
