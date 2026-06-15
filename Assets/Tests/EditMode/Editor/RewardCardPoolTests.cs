using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RewardCardPoolTests
{
    private readonly List<GameObject> _createdGameObjectList = new List<GameObject>();
    private readonly List<ScriptableObject> _createdScriptableObjectList = new List<ScriptableObject>();

    // 清理测试期间创建的 Unity 对象。
    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdGameObject in _createdGameObjectList)
        {
            if (createdGameObject)
            {
                Object.DestroyImmediate(createdGameObject);
            }
        }

        foreach (ScriptableObject scriptableObject in _createdScriptableObjectList)
        {
            if (scriptableObject)
            {
                Object.DestroyImmediate(scriptableObject);
            }
        }

        _createdGameObjectList.Clear();
        _createdScriptableObjectList.Clear();
    }

    // 验证未到最小波数的卡牌不会进入抽取结果。
    [Test]
    public void DrawCards_FiltersByMinWave()
    {
        RewardCardDrawPoolSo rewardCardDrawPool = CreatePool();
        RewardCardSo earlyCard = CreateCard("EarlyCard", 0, false, 0);
        RewardCardSo lateCard = CreateCard("LateCard", 11, false, 0);
        SetField(rewardCardDrawPool, "rewardCardList", new List<RewardCardSo> { earlyCard, lateCard });

        List<RewardCardSo> resultList = rewardCardDrawPool.DrawCards(1, RewardCardDrawContext.Default(5));

        Assert.AreEqual(1, resultList.Count);
        Assert.AreSame(earlyCard, resultList[0]);
    }

    // 验证唯一卡已经选择过后不会再次进入抽取结果。
    [Test]
    public void DrawCards_FiltersUniqueSelectedCard()
    {
        RewardCardDrawPoolSo rewardCardDrawPool = CreatePool();
        RewardCardSo uniqueCard = CreateCard("UniqueCard", 0, true, 0);
        RewardCardSo fallbackCard = CreateCard("FallbackCard", 0, false, 0);
        SetField(rewardCardDrawPool, "rewardCardList", new List<RewardCardSo> { uniqueCard, fallbackCard });

        Dictionary<string, int> selectedCountDic = new Dictionary<string, int>
        {
            { uniqueCard.name, 1 }
        };
        RewardCardDrawContext context = new RewardCardDrawContext(12, selectedCountDic);
        List<RewardCardSo> resultList = rewardCardDrawPool.DrawCards(1, context);

        Assert.AreEqual(1, resultList.Count);
        Assert.AreSame(fallbackCard, resultList[0]);
    }

    // 验证稀有卡会使用自身配置的抽取权重。
    [Test]
    public void DrawCards_UsesConfiguredWeightForRareCard()
    {
        RewardCardDrawPoolSo rewardCardDrawPool = CreatePool();
        RewardCardSo rareCard = CreateCard("RareCard", 0, false, 0);
        SetField(rareCard, "rarity", RewardCardRarity.Rare);
        SetField(rewardCardDrawPool, "rewardCardList", new List<RewardCardSo> { rareCard });

        List<RewardCardSo> resultList = rewardCardDrawPool.DrawCards(1, RewardCardDrawContext.Default(0));

        Assert.AreEqual(1, resultList.Count);
        Assert.AreSame(rareCard, resultList[0]);
    }

    // 验证稀有度不会覆盖卡牌自身配置的最小波次。
    [Test]
    public void DrawCards_DoesNotLockRarityBehindHiddenWaveMultiplier()
    {
        RewardCardDrawPoolSo rewardCardDrawPool = CreatePool();
        RewardCardSo legendaryCard = CreateCard("LegendaryCard", 0, false, 0);
        SetField(legendaryCard, "rarity", RewardCardRarity.Legendary);
        SetField(rewardCardDrawPool, "rewardCardList", new List<RewardCardSo> { legendaryCard });

        List<RewardCardSo> resultList = rewardCardDrawPool.DrawCards(1, RewardCardDrawContext.Default(0));

        Assert.AreEqual(1, resultList.Count);
        Assert.AreSame(legendaryCard, resultList[0]);
    }

    // 创建测试用奖励池。
    private RewardCardDrawPoolSo CreatePool()
    {
        RewardCardDrawPoolSo rewardCardDrawPool = ScriptableObject.CreateInstance<RewardCardDrawPoolSo>();
        _createdScriptableObjectList.Add(rewardCardDrawPool);
        return rewardCardDrawPool;
    }

    // 创建测试用奖励卡。
    private RewardCardSo CreateCard(string cardName, int minWaveIndex, bool isUnique, int maxPickCount)
    {
        RewardCardSo rewardCard = ScriptableObject.CreateInstance<RewardCardSo>();
        rewardCard.name = cardName;
        _createdScriptableObjectList.Add(rewardCard);

        GameObject cardPrefab = new GameObject(cardName + "Prefab");
        _createdGameObjectList.Add(cardPrefab);

        SetField(rewardCard, "cardName", cardName);
        SetField(rewardCard, "cardPrefab", cardPrefab);
        SetField(rewardCard, "weight", 1);
        SetField(rewardCard, "minWaveIndex", minWaveIndex);
        SetField(rewardCard, "isUnique", isUnique);
        SetField(rewardCard, "maxPickCount", maxPickCount);
        return rewardCard;
    }

    // 设置私有序列化字段以构建测试数据。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }
}
