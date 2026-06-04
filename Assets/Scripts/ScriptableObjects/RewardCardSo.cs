using System.Collections.Generic;
using UnityEngine;

public enum RewardCardRarity
{
    Common,
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

[CreateAssetMenu(menuName = "ScriptableObjects/RewardCardSo")]
public class RewardCardSo : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int weight = 1;
    [SerializeField] private RewardCardRarity rarity = RewardCardRarity.Common;
    [SerializeField] private RewardCardCategory category = RewardCardCategory.Defense;
    [SerializeField] private List<RewardEffectConfig> effectConfigList = new List<RewardEffectConfig>();

    public string CardId => cardId;
    public GameObject CardPrefab => cardPrefab;
    public int Weight => Mathf.Max(0, weight);
    public RewardCardRarity Rarity => rarity;
    public RewardCardCategory Category => category;
    public IReadOnlyList<RewardEffectConfig> EffectConfigList => effectConfigList;
}
