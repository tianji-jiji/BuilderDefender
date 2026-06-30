using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 奖励运行时协调器流程测试，验证事件顺序和空卡池行为。
/// </summary>
public class RewardRuntimeCoordinatorTests
{
    // 验证奖励应用后先发送应用事件，再发送状态变化事件。
    [Test]
    public void ApplyRewardRaisesEventsInDocumentedOrder()
    {
        using TestGameObjectScope scope = new();
        ScriptableObject rewardCard = null;

        try
        {
            Type coordinatorType = GetRequiredType("RewardRuntimeCoordinator");
            Component coordinator = scope.Create("Coordinator").AddComponent(coordinatorType);
            object history = Activator.CreateInstance(GetRequiredType("RewardHistory"));
            object towerRewards = Activator.CreateInstance(
                GetRequiredType("TowerRewardRuntime"),
                new object[] { null });
            SetField(coordinatorType, coordinator, "_history", history);
            SetField(coordinatorType, coordinator, "_towerRewards", towerRewards);

            List<string> eventNameList = new();
            EventInfo appliedEvent = GetRequiredEvent(coordinatorType, "OnRewardApplied");
            appliedEvent.AddEventHandler(
                coordinator,
                CreateSingleArgumentHandler(appliedEvent.EventHandlerType, () => eventNameList.Add("Applied")));
            GetRequiredEvent(coordinatorType, "OnActiveRewardsChanged")
                .AddEventHandler(coordinator, new Action(() => eventNameList.Add("Changed")));

            Type cardType = GetRequiredType("RewardCardSo");
            rewardCard = ScriptableObject.CreateInstance(cardType);
            rewardCard.name = "TestCard";
            GetRequiredMethod(coordinatorType, "ApplyReward").Invoke(
                coordinator,
                new object[] { rewardCard });

            CollectionAssert.AreEqual(new[] { "Applied", "Changed" }, eventNameList);
            Assert.AreEqual(1, GetRequiredProperty(history.GetType(), "TotalCardCount").GetValue(history));
        }
        finally
        {
            DestroyImmediate(rewardCard);
        }
    }

    // 验证空卡池不会发送候选卡事件。
    [Test]
    public void HandleWaveCompletedEmptyPoolDoesNotRaiseChoices()
    {
        using TestGameObjectScope scope = new();
        ScriptableObject rewardCardPool = null;

        try
        {
            Type coordinatorType = GetRequiredType("RewardRuntimeCoordinator");
            Component coordinator = scope.Create("Coordinator").AddComponent(coordinatorType);
            object history = Activator.CreateInstance(GetRequiredType("RewardHistory"));
            object random = Activator.CreateInstance(GetRequiredType("UnityRewardRandom"));
            object drawService = Activator.CreateInstance(
                GetRequiredType("RewardCardDrawService"),
                new[] { random });
            object towerRewards = Activator.CreateInstance(
                GetRequiredType("TowerRewardRuntime"),
                new object[] { null });
            rewardCardPool = ScriptableObject.CreateInstance(GetRequiredType("RewardCardPoolSo"));

            SetField(coordinatorType, coordinator, "_rewardCardPool", rewardCardPool);
            SetField(coordinatorType, coordinator, "_history", history);
            SetField(coordinatorType, coordinator, "_drawService", drawService);
            SetField(coordinatorType, coordinator, "_towerRewards", towerRewards);

            bool choicesRaised = false;
            EventInfo choicesEvent = GetRequiredEvent(coordinatorType, "OnRewardChoicesReady");
            choicesEvent.AddEventHandler(
                coordinator,
                CreateSingleArgumentHandler(choicesEvent.EventHandlerType, () => choicesRaised = true));

            GetRequiredMethod(coordinatorType, "HandleWaveCompleted", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(coordinator, new object[] { 1 });

            Assert.IsFalse(choicesRaised);
        }
        finally
        {
            DestroyImmediate(rewardCardPool);
        }
    }

    // 创建忽略事件参数并执行指定回调的委托。
    private static Delegate CreateSingleArgumentHandler(Type eventHandlerType, Action callback)
    {
        Type argumentType = eventHandlerType.GetGenericArguments()[0];
        MethodInfo factoryMethod = typeof(RewardRuntimeCoordinatorTests)
            .GetMethod(nameof(CreateActionHandler), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(argumentType);
        Assert.NotNull(factoryMethod);
        return (Delegate)factoryMethod.Invoke(null, new object[] { callback });
    }

    // 创建指定参数类型的 Action 委托。
    private static Delegate CreateActionHandler<T>(Action callback)
    {
        return new Action<T>(_ => callback());
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"未找到字段：{type.Name}.{fieldName}");
        field.SetValue(target, value);
    }

    // 获取指定类型的事件。
    private static EventInfo GetRequiredEvent(Type type, string eventName)
    {
        EventInfo eventInfo = type.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(eventInfo, $"未找到事件：{type.Name}.{eventName}");
        return eventInfo;
    }

    // 获取指定类型的方法。
    private static MethodInfo GetRequiredMethod(
        Type type,
        string methodName,
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
    {
        MethodInfo method = type.GetMethod(methodName, bindingFlags);
        Assert.NotNull(method, $"未找到方法：{type.Name}.{methodName}");
        return method;
    }

    // 获取指定类型的公开属性。
    private static PropertyInfo GetRequiredProperty(Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property, $"未找到属性：{type.Name}.{propertyName}");
        return property;
    }

    // 从生产程序集取得指定类型。
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
