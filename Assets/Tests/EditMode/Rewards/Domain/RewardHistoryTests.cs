using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 奖励历史模型测试，验证纯 C# 记录、清空与抽取上下文构建行为。
/// </summary>
public class RewardHistoryTests
{
    // 验证同一张奖励卡重复记录时会累加叠加次数并保留首次获得波次。
    [Test]
    public void RecordSameCardIncrementsStackCount()
    {
        ScriptableObject rewardCard = null;

        try
        {
            Type historyType = GetRequiredType("RewardHistory");
            Type cardType = GetRequiredType("RewardCardSo");
            object history = Activator.CreateInstance(historyType);
            MethodInfo recordMethod = historyType.GetMethod("Record", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo recordListProperty = historyType.GetProperty("RecordList");
            PropertyInfo totalCardCountProperty = historyType.GetProperty("TotalCardCount");
            PropertyInfo stackCountProperty = GetRequiredType("RewardCardRecord").GetProperty("StackCount");
            PropertyInfo acquiredWaveIndexProperty = GetRequiredType("RewardCardRecord").GetProperty("AcquiredWaveIndex");

            Assert.NotNull(recordMethod);
            Assert.NotNull(recordListProperty);
            Assert.NotNull(totalCardCountProperty);
            Assert.NotNull(stackCountProperty);
            Assert.NotNull(acquiredWaveIndexProperty);

            rewardCard = CreateRewardCard(cardType, "TestCard", "StableCardId");
            object firstRecord = recordMethod.Invoke(history, new object[] { rewardCard, 3 });
            object secondRecord = recordMethod.Invoke(history, new object[] { rewardCard, 4 });
            IList recordList = recordListProperty.GetValue(history) as IList;

            Assert.NotNull(firstRecord);
            Assert.AreSame(firstRecord, secondRecord);
            Assert.NotNull(recordList);
            Assert.AreEqual(1, recordList.Count);
            Assert.AreEqual(2, stackCountProperty.GetValue(firstRecord));
            Assert.AreEqual(3, acquiredWaveIndexProperty.GetValue(firstRecord));
            Assert.AreEqual(2, totalCardCountProperty.GetValue(history));
        }
        finally
        {
            DestroyImmediate(rewardCard);
        }
    }

    // 验证清空历史时会同时移除列表与总计数。
    [Test]
    public void ClearRemovesAllRecords()
    {
        ScriptableObject rewardCard = null;

        try
        {
            Type historyType = GetRequiredType("RewardHistory");
            Type cardType = GetRequiredType("RewardCardSo");
            object history = Activator.CreateInstance(historyType);
            MethodInfo recordMethod = historyType.GetMethod("Record", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo clearMethod = historyType.GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo recordListProperty = historyType.GetProperty("RecordList");
            PropertyInfo totalCardCountProperty = historyType.GetProperty("TotalCardCount");

            Assert.NotNull(recordMethod);
            Assert.NotNull(clearMethod);
            Assert.NotNull(recordListProperty);
            Assert.NotNull(totalCardCountProperty);

            rewardCard = CreateRewardCard(cardType, "TestCard", "StableCardId");
            recordMethod.Invoke(history, new object[] { rewardCard, 3 });
            clearMethod.Invoke(history, Array.Empty<object>());

            IList recordList = recordListProperty.GetValue(history) as IList;
            Assert.NotNull(recordList);
            Assert.AreEqual(0, recordList.Count);
            Assert.AreEqual(0, totalCardCountProperty.GetValue(history));
        }
        finally
        {
            DestroyImmediate(rewardCard);
        }
    }

    // 验证抽取上下文会按叠加次数构建已选计数。
    [Test]
    public void BuildDrawContextUsesStackCounts()
    {
        ScriptableObject rewardCard = null;

        try
        {
            Type historyType = GetRequiredType("RewardHistory");
            Type cardType = GetRequiredType("RewardCardSo");
            Type contextType = GetRequiredType("RewardCardDrawContext");
            object history = Activator.CreateInstance(historyType);
            MethodInfo recordMethod = historyType.GetMethod("Record", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo buildDrawContextMethod = historyType.GetMethod("BuildDrawContext", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo getSelectedCountMethod = contextType.GetMethod("GetSelectedCount", new[] { cardType });
            PropertyInfo currentWaveIndexProperty = contextType.GetProperty("CurrentWaveIndex");

            Assert.NotNull(recordMethod);
            Assert.NotNull(buildDrawContextMethod);
            Assert.NotNull(getSelectedCountMethod);
            Assert.NotNull(currentWaveIndexProperty);

            rewardCard = CreateRewardCard(cardType, "TestCard", "StableCardId");
            recordMethod.Invoke(history, new object[] { rewardCard, 3 });
            recordMethod.Invoke(history, new object[] { rewardCard, 4 });
            object drawContext = buildDrawContextMethod.Invoke(history, new object[] { 8 });

            Assert.NotNull(drawContext);
            Assert.AreEqual(8, currentWaveIndexProperty.GetValue(drawContext));
            Assert.AreEqual(2, getSelectedCountMethod.Invoke(drawContext, new object[] { rewardCard }));
        }
        finally
        {
            DestroyImmediate(rewardCard);
        }
    }

    // 创建用于测试的奖励卡牌资产，并写入稳定卡牌 ID。
    private static ScriptableObject CreateRewardCard(Type cardType, string cardName, string cardId)
    {
        ScriptableObject rewardCard = ScriptableObject.CreateInstance(cardType);
        rewardCard.name = cardName;
        SetField(cardType, rewardCard, "cardId", cardId);
        return rewardCard;
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"未找到字段：{type.Name}.{fieldName}");
        field.SetValue(target, value);
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
