using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tower 奖励触发器现有行为测试，为后续重命名和移动提供保护。
/// </summary>
public class TowerRewardTriggerCharacterizationTests
{
    // 验证单个效果的计数按来源 Tower 隔离。
    [Test]
    public void RuntimeStateTracksCounterPerTower()
    {
        using TestGameObjectScope scope = new();
        Type towerType = GetRequiredType("TowerCombatSystem");
        Type stateType = GetRequiredType("TowerEffectState");
        Component firstTower = scope.Create("FirstTower").AddComponent(towerType);
        Component secondTower = scope.Create("SecondTower").AddComponent(towerType);
        object state = Activator.CreateInstance(stateType, new object[] { null, null });
        MethodInfo incrementCounter = stateType.GetMethod(
            "IncrementCounter",
            new[] { towerType, typeof(string) });

        Assert.NotNull(incrementCounter);
        Assert.AreEqual(1, incrementCounter.Invoke(state, new object[] { firstTower, "Attack" }));
        Assert.AreEqual(2, incrementCounter.Invoke(state, new object[] { firstTower, "Attack" }));
        Assert.AreEqual(1, incrementCounter.Invoke(state, new object[] { secondTower, "Attack" }));
    }

    // 验证攻击完成上下文累计额外攻击次数。
    [Test]
    public void AttackCompletedContextAccumulatesExtraAttacks()
    {
        Type contextType = GetRequiredType("TowerAttackCompletedContext");
        object context = Activator.CreateInstance(contextType, new object[] { null, null });
        MethodInfo requestExtraAttack = contextType.GetMethod(
            "RequestExtraAttack",
            new[] { typeof(int) });
        PropertyInfo extraAttackCount = contextType.GetProperty("ExtraAttackCount");

        Assert.NotNull(requestExtraAttack);
        Assert.NotNull(extraAttackCount);
        requestExtraAttack.Invoke(context, new object[] { 1 });
        requestExtraAttack.Invoke(context, new object[] { 2 });

        Assert.AreEqual(3, extraAttackCount.GetValue(context));
    }

    // 验证清理指定 Tower 不会影响其他 Tower 的计数。
    [Test]
    public void RuntimeStateClearCountersDoesNotAffectOtherTower()
    {
        using TestGameObjectScope scope = new();
        Type towerType = GetRequiredType("TowerCombatSystem");
        Type stateType = GetRequiredType("TowerEffectState");
        Component firstTower = scope.Create("FirstTower").AddComponent(towerType);
        Component secondTower = scope.Create("SecondTower").AddComponent(towerType);
        object state = Activator.CreateInstance(stateType, new object[] { null, null });
        MethodInfo incrementCounter = stateType.GetMethod(
            "IncrementCounter",
            new[] { towerType, typeof(string) });
        MethodInfo clearCounters = stateType.GetMethod(
            "ClearCounters",
            new[] { towerType });

        Assert.NotNull(incrementCounter);
        Assert.NotNull(clearCounters);
        incrementCounter.Invoke(state, new object[] { firstTower, "Kill" });
        incrementCounter.Invoke(state, new object[] { secondTower, "Kill" });
        clearCounters.Invoke(state, new object[] { firstTower });

        Assert.AreEqual(1, incrementCounter.Invoke(state, new object[] { firstTower, "Kill" }));
        Assert.AreEqual(2, incrementCounter.Invoke(state, new object[] { secondTower, "Kill" }));
    }

    // 从生产程序集取得指定的当前类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
