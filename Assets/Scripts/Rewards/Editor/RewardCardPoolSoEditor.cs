using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 奖励卡池的自定义 Inspector，负责按稀有度分组整理卡牌列表。
/// </summary>
[CustomEditor(typeof(RewardCardPoolSo))]
public class RewardCardPoolSoEditor : Editor
{
    private static readonly RewardCardRarity[] RarityOrderArray =
    {
        RewardCardRarity.Normal,
        RewardCardRarity.Rare,
        RewardCardRarity.Epic,
        RewardCardRarity.Legendary,
        RewardCardRarity.Mythic
    };

    private static readonly string[] RarityNameArray =
    {
        "普通",
        "稀有",
        "史诗",
        "传奇",
        "神话"
    };

    private readonly bool[] _rarityFoldoutArray = { true, true, true, true, true };
    private readonly Dictionary<int, RewardCardRarity> _emptySlotRarityDic = new();
    private readonly Dictionary<RewardCardRarity, List<CardSlot>> _cardSlotListDic = new();
    private readonly Dictionary<RewardCardRarity, ReorderableList> _rarityListDic = new();

    private SerializedProperty _rewardCardListProp;
    private SerializedProperty _choiceCountProp;
    private SerializedProperty _allowDuplicateProp;

    // 缓存序列化字段引用。
    private void OnEnable()
    {
        _rewardCardListProp = serializedObject.FindProperty("rewardCardList");
        _choiceCountProp = serializedObject.FindProperty("choiceCount");
        _allowDuplicateProp = serializedObject.FindProperty("allowDuplicate");
        InitializeRarityLists();
        RebuildAllGroupSlotLists();
    }

    // 绘制奖励卡池 Inspector。
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RefreshGroupSlotListsIfIdle();

        DrawPoolSettings();
        EditorGUILayout.Space(8f);
        DrawGroupedRewardCards();
        EditorGUILayout.Space(8f);
        DrawValidationMessages();

        bool hasModifiedProperties = serializedObject.ApplyModifiedProperties();
        if (hasModifiedProperties)
        {
            RefreshGroupSlotListsIfIdle();
        }
    }

    // 绘制卡池基础设置。
    private void DrawPoolSettings()
    {
        EditorGUILayout.LabelField("抽取设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_choiceCountProp, new GUIContent("展示选项数量"));
        EditorGUILayout.PropertyField(_allowDuplicateProp, new GUIContent("允许同批重复"));
    }

    // 绘制按稀有度分组的卡牌列表。
    private void DrawGroupedRewardCards()
    {
        EditorGUILayout.LabelField($"奖励卡牌列表 ({_rewardCardListProp.arraySize})", EditorStyles.boldLabel);

        for (int i = 0; i < RarityOrderArray.Length; i++)
        {
            DrawRarityGroup(i, RarityOrderArray[i], RarityNameArray[i]);
        }
    }

    // 绘制单个稀有度分组。
    private void DrawRarityGroup(int foldoutIndex, RewardCardRarity rarity, string rarityName)
    {
        List<CardSlot> cardSlotList = _cardSlotListDic[rarity];
        _rarityFoldoutArray[foldoutIndex] = EditorGUILayout.Foldout(
            _rarityFoldoutArray[foldoutIndex],
            $"{rarityName} ({cardSlotList.Count})",
            true);

        if (!_rarityFoldoutArray[foldoutIndex])
        {
            return;
        }

        ReorderableList reorderableList = _rarityListDic[rarity];
        reorderableList.DoLayoutList();
    }

    // 初始化每个稀有度分组的可拖拽列表。
    private void InitializeRarityLists()
    {
        _cardSlotListDic.Clear();
        _rarityListDic.Clear();

        for (int i = 0; i < RarityOrderArray.Length; i++)
        {
            RewardCardRarity rarity = RarityOrderArray[i];
            string rarityName = RarityNameArray[i];
            List<CardSlot> cardSlotList = new();
            ReorderableList reorderableList = CreateRarityReorderableList(cardSlotList, rarity, rarityName);

            _cardSlotListDic[rarity] = cardSlotList;
            _rarityListDic[rarity] = reorderableList;
        }
    }

    // 创建指定稀有度分组的可拖拽列表。
    private ReorderableList CreateRarityReorderableList(List<CardSlot> cardSlotList, RewardCardRarity rarity, string rarityName)
    {
        ReorderableList reorderableList = new(cardSlotList, typeof(CardSlot), true, false, true, true);
        reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => DrawCardSlot(rect, cardSlotList, index, isActive, isFocused);
        reorderableList.onAddCallback = list => AddCardSlot(rarity, list);
        reorderableList.onRemoveCallback = list => RemoveCardSlot(cardSlotList, list.index);
        reorderableList.onReorderCallbackWithDetails = (list, _, _) => RewriteGroupSlots(rarity, (List<CardSlot>)list.list);
        reorderableList.onSelectCallback = _ => Repaint();
        reorderableList.drawNoneElementCallback = rect => EditorGUI.LabelField(rect, $"暂无{rarityName}卡牌");
        return reorderableList;
    }

    // 绘制单张卡牌引用槽位。
    private void DrawCardSlot(Rect rect, IReadOnlyList<CardSlot> cardSlotList, int index, bool isActive, bool isFocused)
    {
        if (index < 0 || index >= cardSlotList.Count)
        {
            return;
        }

        if (isActive || isFocused)
        {
            Rect selectionRect = new(rect.x, rect.y + 1f, rect.width, rect.height - 2f);
            EditorGUI.DrawRect(selectionRect, new Color(0.24f, 0.49f, 0.9f, 0.25f));
        }

        CardSlot cardSlot = cardSlotList[index];
        SerializedProperty cardProp = _rewardCardListProp.GetArrayElementAtIndex(cardSlot.cardIndex);
        Rect fieldRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, cardProp, GUIContent.none);
    }

    // 在指定稀有度分组末尾新增空位。
    private void AddCardSlot(RewardCardRarity rarity, ReorderableList reorderableList)
    {
        int insertIndex = FindInsertIndex(rarity);
        _rewardCardListProp.InsertArrayElementAtIndex(insertIndex);
        _rewardCardListProp.GetArrayElementAtIndex(insertIndex).objectReferenceValue = null;
        RemapEmptySlotIndexesAfterInsert(insertIndex);
        _emptySlotRarityDic[insertIndex] = rarity;
        serializedObject.ApplyModifiedProperties();
        RebuildAllGroupSlotLists();
        reorderableList.index = _cardSlotListDic[rarity].FindIndex(cardSlot => cardSlot.cardIndex == insertIndex);
        Repaint();
    }

    // 移除指定分组中的卡牌槽位。
    private void RemoveCardSlot(List<CardSlot> cardSlotList, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cardSlotList.Count)
        {
            return;
        }

        int cardIndex = cardSlotList[slotIndex].cardIndex;
        _rewardCardListProp.GetArrayElementAtIndex(cardIndex).objectReferenceValue = null;
        _rewardCardListProp.DeleteArrayElementAtIndex(cardIndex);
        RemapEmptySlotIndexesAfterDelete(cardIndex);
        serializedObject.ApplyModifiedProperties();
        RebuildAllGroupSlotLists();
        Repaint();
    }

    // 使用拖拽后的分组顺序重写底层数组。
    private void RewriteGroupSlots(RewardCardRarity rarity, List<CardSlot> newCardSlotList)
    {
        List<RewardCardSo> allCardList = BuildAllCardList();
        List<int> groupIndexList = new();
        foreach (CardSlot cardSlot in newCardSlotList)
        {
            groupIndexList.Add(cardSlot.cardIndex);
        }

        groupIndexList.Sort();

        List<RewardCardSo> groupCardList = new();
        foreach (CardSlot cardSlot in newCardSlotList)
        {
            groupCardList.Add(allCardList[cardSlot.cardIndex]);
        }

        for (int i = 0; i < groupIndexList.Count; i++)
        {
            allCardList[groupIndexList[i]] = groupCardList[i];
        }

        WriteAllCardList(allCardList);
        RebuildEmptySlotRarityIndexes(rarity, groupIndexList, allCardList);
        serializedObject.ApplyModifiedProperties();
        RebuildAllGroupSlotLists();
        Repaint();
    }

    // 在非拖拽交互时刷新分组槽位缓存。
    private void RefreshGroupSlotListsIfIdle()
    {
        if (GUIUtility.hotControl != 0)
        {
            return;
        }

        RebuildAllGroupSlotLists();
    }

    // 重建所有稀有度分组的槽位缓存。
    private void RebuildAllGroupSlotLists()
    {
        foreach (RewardCardRarity rarity in RarityOrderArray)
        {
            List<CardSlot> cardSlotList = _cardSlotListDic[rarity];
            cardSlotList.Clear();
        }

        for (int i = 0; i < _rewardCardListProp.arraySize; i++)
        {
            RewardCardSo rewardCard = _rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue as RewardCardSo;
            RewardCardRarity rarity = rewardCard ? rewardCard.Rarity : GetEmptySlotRarity(i);

            if (!_cardSlotListDic.TryGetValue(rarity, out List<CardSlot> cardSlotList))
            {
                continue;
            }

            cardSlotList.Add(new CardSlot(i));
        }
    }

    // 查找新卡牌空位的插入位置。
    private int FindInsertIndex(RewardCardRarity rarity)
    {
        int insertIndex = _rewardCardListProp.arraySize;
        int targetOrder = GetRarityOrder(rarity);

        for (int i = 0; i < _rewardCardListProp.arraySize; i++)
        {
            RewardCardSo rewardCard = _rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue as RewardCardSo;
            int currentOrder = rewardCard ? GetRarityOrder(rewardCard.Rarity) : GetEmptySlotOrder(i);
            if (currentOrder > targetOrder)
            {
                return i;
            }

            if (currentOrder == targetOrder)
            {
                insertIndex = i + 1;
            }
        }

        return insertIndex;
    }

    // 读取底层卡牌数组。
    private List<RewardCardSo> BuildAllCardList()
    {
        List<RewardCardSo> allCardList = new();
        for (int i = 0; i < _rewardCardListProp.arraySize; i++)
        {
            allCardList.Add(_rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue as RewardCardSo);
        }

        return allCardList;
    }

    // 写回底层卡牌数组。
    private void WriteAllCardList(IReadOnlyList<RewardCardSo> allCardList)
    {
        _rewardCardListProp.arraySize = allCardList.Count;
        for (int i = 0; i < allCardList.Count; i++)
        {
            _rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue = allCardList[i];
        }
    }

    // 插入槽位后重映射空位索引。
    private void RemapEmptySlotIndexesAfterInsert(int insertIndex)
    {
        Dictionary<int, RewardCardRarity> remappedDic = new();
        foreach (KeyValuePair<int, RewardCardRarity> pair in _emptySlotRarityDic)
        {
            int remappedIndex = pair.Key >= insertIndex ? pair.Key + 1 : pair.Key;
            remappedDic[remappedIndex] = pair.Value;
        }

        _emptySlotRarityDic.Clear();
        foreach (KeyValuePair<int, RewardCardRarity> pair in remappedDic)
        {
            _emptySlotRarityDic[pair.Key] = pair.Value;
        }
    }

    // 删除槽位后重映射空位索引。
    private void RemapEmptySlotIndexesAfterDelete(int deletedIndex)
    {
        Dictionary<int, RewardCardRarity> remappedDic = new();
        foreach (KeyValuePair<int, RewardCardRarity> pair in _emptySlotRarityDic)
        {
            if (pair.Key == deletedIndex)
            {
                continue;
            }

            int remappedIndex = pair.Key > deletedIndex ? pair.Key - 1 : pair.Key;
            remappedDic[remappedIndex] = pair.Value;
        }

        _emptySlotRarityDic.Clear();
        foreach (KeyValuePair<int, RewardCardRarity> pair in remappedDic)
        {
            _emptySlotRarityDic[pair.Key] = pair.Value;
        }
    }

    // 拖拽排序后重建指定分组的空位索引。
    private void RebuildEmptySlotRarityIndexes(RewardCardRarity rarity, IReadOnlyList<int> groupIndexList, IReadOnlyList<RewardCardSo> allCardList)
    {
        foreach (int groupIndex in groupIndexList)
        {
            if (allCardList[groupIndex])
            {
                _emptySlotRarityDic.Remove(groupIndex);
                continue;
            }

            _emptySlotRarityDic[groupIndex] = rarity;
        }
    }

    // 获取空位稀有度排序值。
    private int GetEmptySlotOrder(int cardIndex)
    {
        return GetRarityOrder(GetEmptySlotRarity(cardIndex));
    }

    // 获取空位所属的稀有度分组。
    private RewardCardRarity GetEmptySlotRarity(int cardIndex)
    {
        if (_emptySlotRarityDic.TryGetValue(cardIndex, out RewardCardRarity rarity))
        {
            return rarity;
        }

        for (int i = cardIndex - 1; i >= 0; i--)
        {
            RewardCardSo previousCard = _rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue as RewardCardSo;
            if (previousCard)
            {
                return previousCard.Rarity;
            }
        }

        for (int i = cardIndex + 1; i < _rewardCardListProp.arraySize; i++)
        {
            RewardCardSo nextCard = _rewardCardListProp.GetArrayElementAtIndex(i).objectReferenceValue as RewardCardSo;
            if (nextCard)
            {
                return nextCard.Rarity;
            }
        }

        return RewardCardRarity.Normal;
    }

    // 获取稀有度排序值。
    private int GetRarityOrder(RewardCardRarity rarity)
    {
        for (int i = 0; i < RarityOrderArray.Length; i++)
        {
            if (RarityOrderArray[i] == rarity)
            {
                return i;
            }
        }

        return int.MaxValue - 1;
    }

    // 绘制卡池配置校验信息。
    private void DrawValidationMessages()
    {
        if (_rewardCardListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("卡池为空，不会生成任何奖励选项。", MessageType.Warning);
        }
    }

    /// <summary>
    /// 卡牌槽位快照，用于分组列表拖拽排序。
    /// </summary>
    private readonly struct CardSlot
    {
        public readonly int cardIndex;

        // 创建卡牌槽位快照。
        public CardSlot(int cardIndex)
        {
            this.cardIndex = cardIndex;
        }
    }
}
