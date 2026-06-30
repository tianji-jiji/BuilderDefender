using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 奖励卡牌抽取服务测试，验证卡池规则与运行时上下文的协作行为。
/// </summary>
public class RewardCardDrawServiceTests
{
    // 验证抽取服务会过滤高于当前波次才解锁的卡牌。
    [Test]
    public void DrawFiltersCardsAboveCurrentWave()
    {
        ScriptableObject pool = null;
        ScriptableObject availableCard = null;
        ScriptableObject lockedCard = null;
        GameObject availableCardPrefab = null;
        GameObject lockedCardPrefab = null;

        try
        {
            Type cardType = GetRequiredType("RewardCardSo");
            Type poolType = GetRequiredType("RewardCardPoolSo");
            Type contextType = GetRequiredType("RewardCardDrawContext");
            Type randomType = GetRequiredType("UnityRewardRandom");
            Type serviceType = GetRequiredType("RewardCardDrawService");

            availableCard = ScriptableObject.CreateInstance(cardType);
            lockedCard = ScriptableObject.CreateInstance(cardType);
            availableCardPrefab = new GameObject("AvailableCardPrefab");
            lockedCardPrefab = new GameObject("LockedCardPrefab");
            ConfigureCard(cardType, availableCard, availableCardPrefab, 1);
            ConfigureCard(cardType, lockedCard, lockedCardPrefab, 5);
            pool = CreatePool(poolType, 1, false, availableCard, lockedCard);

            object context = Activator.CreateInstance(contextType, new object[] { 1, null });
            object random = Activator.CreateInstance(randomType);
            object service = Activator.CreateInstance(serviceType, new[] { random });
            MethodInfo drawMethod = serviceType.GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(drawMethod);
            IList resultList = drawMethod.Invoke(service, new[] { pool, context }) as IList;
            Assert.NotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreSame(availableCard, resultList[0]);
        }
        finally
        {
            DestroyImmediate(pool);
            DestroyImmediate(availableCard);
            DestroyImmediate(lockedCard);
            DestroyImmediate(availableCardPrefab);
            DestroyImmediate(lockedCardPrefab);
        }
    }

    // 验证关闭重复抽取时，只有一张有效卡也只会返回一张结果。
    [Test]
    public void DrawWithoutDuplicatesReturnsSingleAvailableCard()
    {
        ScriptableObject pool = null;
        ScriptableObject availableCard = null;
        GameObject availableCardPrefab = null;

        try
        {
            Type cardType = GetRequiredType("RewardCardSo");
            Type poolType = GetRequiredType("RewardCardPoolSo");
            Type contextType = GetRequiredType("RewardCardDrawContext");
            Type randomType = GetRequiredType("UnityRewardRandom");
            Type serviceType = GetRequiredType("RewardCardDrawService");

            availableCardPrefab = new GameObject("AvailableCardPrefab");
            availableCard = CreateCard(cardType, "AvailableCard", "AvailableCardId", availableCardPrefab, 0, 0, 1);
            pool = CreatePool(poolType, 2, false, availableCard);

            object context = CreateContext(contextType, 1, null);
            object random = Activator.CreateInstance(randomType);
            object service = Activator.CreateInstance(serviceType, new[] { random });
            MethodInfo drawMethod = serviceType.GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(drawMethod);
            IList resultList = drawMethod.Invoke(service, new[] { pool, context }) as IList;
            Assert.NotNull(resultList);
            Assert.AreEqual(1, resultList.Count);
            Assert.AreSame(availableCard, resultList[0]);
        }
        finally
        {
            DestroyImmediate(pool);
            DestroyImmediate(availableCard);
            DestroyImmediate(availableCardPrefab);
        }
    }

    // 验证已达到最大选择次数的卡牌会被排除在抽取池之外。
    [Test]
    public void DrawExcludesCardAtMaxPickCount()
    {
        ScriptableObject pool = null;
        ScriptableObject blockedCard = null;
        GameObject blockedCardPrefab = null;

        try
        {
            Type cardType = GetRequiredType("RewardCardSo");
            Type poolType = GetRequiredType("RewardCardPoolSo");
            Type contextType = GetRequiredType("RewardCardDrawContext");
            Type randomType = GetRequiredType("UnityRewardRandom");
            Type serviceType = GetRequiredType("RewardCardDrawService");

            blockedCardPrefab = new GameObject("BlockedCardPrefab");
            blockedCard = CreateCard(cardType, "BlockedCard", "StableBlockedCardId", blockedCardPrefab, 0, 1, 1);
            pool = CreatePool(poolType, 1, false, blockedCard);

            Dictionary<string, int> selectedCardCountDic = new()
            {
                { "StableBlockedCardId", 1 }
            };

            object context = CreateContext(contextType, 1, selectedCardCountDic);
            object random = Activator.CreateInstance(randomType);
            object service = Activator.CreateInstance(serviceType, new[] { random });
            MethodInfo drawMethod = serviceType.GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(drawMethod);
            IList resultList = drawMethod.Invoke(service, new[] { pool, context }) as IList;
            Assert.NotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
        }
        finally
        {
            DestroyImmediate(pool);
            DestroyImmediate(blockedCard);
            DestroyImmediate(blockedCardPrefab);
        }
    }

    // 验证空卡池会直接返回空结果列表。
    [Test]
    public void DrawEmptyPoolReturnsEmptyList()
    {
        ScriptableObject pool = null;

        try
        {
            Type poolType = GetRequiredType("RewardCardPoolSo");
            Type contextType = GetRequiredType("RewardCardDrawContext");
            Type randomType = GetRequiredType("UnityRewardRandom");
            Type serviceType = GetRequiredType("RewardCardDrawService");

            pool = CreatePool(poolType, 2, false);

            object context = CreateContext(contextType, 1, null);
            object random = Activator.CreateInstance(randomType);
            object service = Activator.CreateInstance(serviceType, new[] { random });
            MethodInfo drawMethod = serviceType.GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(drawMethod);
            IList resultList = drawMethod.Invoke(service, new[] { pool, context }) as IList;
            Assert.NotNull(resultList);
            Assert.AreEqual(0, resultList.Count);
        }
        finally
        {
            DestroyImmediate(pool);
        }
    }

    // 配置测试卡牌的预制体、权重和最低解锁波次。
    private static void ConfigureCard(Type cardType, ScriptableObject card, GameObject cardPrefab, int minWaveIndex)
    {
        SetField(cardType, card, "cardPrefab", cardPrefab);
        SetField(cardType, card, "weight", 1);
        SetField(cardType, card, "minWaveIndex", minWaveIndex);
    }

    // 创建并配置一张奖励测试卡牌。
    private static ScriptableObject CreateCard(
        Type cardType,
        string cardObjectName,
        string cardId,
        GameObject cardPrefab,
        int minWaveIndex,
        int maxPickCount,
        int weight)
    {
        ScriptableObject card = ScriptableObject.CreateInstance(cardType);
        card.name = cardObjectName;
        SetField(cardType, card, "cardId", cardId);
        SetField(cardType, card, "cardPrefab", cardPrefab);
        SetField(cardType, card, "weight", weight);
        SetField(cardType, card, "minWaveIndex", minWaveIndex);
        SetField(cardType, card, "maxPickCount", maxPickCount);
        return card;
    }

    // 配置测试卡池中的候选卡牌与单次选项数量。
    private static ScriptableObject CreatePool(
        Type poolType,
        int choiceCount,
        bool allowDuplicate,
        params ScriptableObject[] cardArray)
    {
        ScriptableObject pool = ScriptableObject.CreateInstance(poolType);
        FieldInfo rewardCardListField = GetRequiredField(poolType, "rewardCardList");
        IList rewardCardList = Activator.CreateInstance(rewardCardListField.FieldType) as IList;
        Assert.NotNull(rewardCardList);

        foreach (ScriptableObject card in cardArray)
        {
            rewardCardList.Add(card);
        }

        rewardCardListField.SetValue(pool, rewardCardList);
        SetField(poolType, pool, "choiceCount", choiceCount);
        SetField(poolType, pool, "allowDuplicate", allowDuplicate);
        return pool;
    }

    // 创建奖励抽取上下文并注入已选卡牌计数。
    private static object CreateContext(Type contextType, int currentWaveIndex, IReadOnlyDictionary<string, int> selectedCardCountDic)
    {
        return Activator.CreateInstance(contextType, new object[] { currentWaveIndex, selectedCardCountDic });
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        GetRequiredField(type, fieldName).SetValue(target, value);
    }

    // 从生产类型取得指定私有实例字段。
    private static FieldInfo GetRequiredField(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"未找到字段：{type.Name}.{fieldName}");
        return field;
    }

    // 从生产程序集取得指定的当前类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }

    // 立即销毁测试期间创建的 Unity 对象。
    private static void DestroyImmediate(Object unityObject)
    {
        if (unityObject)
        {
            Object.DestroyImmediate(unityObject);
        }
    }
}
