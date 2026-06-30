using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 奖励卡牌数据资产，负责配置单张卡牌的基础信息、显示预制体和奖励效果。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardSo")]
public class RewardCardSo : ScriptableObject
{
    private const string CARD_ID_PREFIX = "RewardCard_";

    // 基础信息
    [HideInInspector] [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int weight = 1;
    [SerializeField] private RewardCardRarity rarity = RewardCardRarity.Normal;
    [SerializeField] private RewardCardCategory category = RewardCardCategory.Defense;
    [SerializeField] private int minWaveIndex;
    [SerializeField] private int maxPickCount;
    [SerializeField] private bool isUnique;
    
    //奖励效果列表
    [SerializeField] private List<RewardCardEffectConfig> effectConfigList = new();

    public string CardId => cardId;
    public string CardName => string.IsNullOrWhiteSpace(cardName) ? cardId : cardName;
    public GameObject CardPrefab => cardPrefab;
    public int Weight => Mathf.Max(0, weight);
    public RewardCardRarity Rarity => rarity;
    public int MinWaveIndex => Mathf.Max(0, minWaveIndex);
    public int MaxPickCount => isUnique ? 1 : maxPickCount <= 0 ? int.MaxValue : maxPickCount;
    public IReadOnlyList<RewardCardEffectConfig> EffectConfigList => effectConfigList;

#if UNITY_EDITOR
    // 在编辑器中根据资产 GUID 自动刷新卡牌 ID。
    private void OnValidate()
    {
        RefreshGeneratedCardIdInEditor();
    }

    // 根据 Unity 资产 GUID 生成稳定卡牌 ID。
    public bool RefreshGeneratedCardIdInEditor()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return false;
        }

        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(assetGuid))
        {
            return false;
        }

        string generatedCardId = $"{CARD_ID_PREFIX}{assetGuid}";
        if (cardId == generatedCardId)
        {
            return false;
        }

        cardId = generatedCardId;
        EditorUtility.SetDirty(this);
        return true;
    }
#endif
}
