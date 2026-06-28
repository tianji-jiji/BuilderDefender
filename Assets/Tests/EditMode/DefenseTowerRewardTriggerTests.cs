using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 防御塔运行时奖励触发器测试，验证能力分发和单塔状态隔离。
/// </summary>
public class DefenseTowerRewardTriggerTests
{
    // 运行时奖励计数应按来源防御塔分别累计。
    [Test]
    public void RuntimeState_TracksCounterPerTower()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type stateType = GetRequiredType("DefenseTowerRewardRuntimeState");
        GameObject firstTowerObject = new("FirstTower");
        GameObject secondTowerObject = new("SecondTower");
        firstTowerObject.SetActive(false);
        secondTowerObject.SetActive(false);

        try
        {
            Component firstTower = firstTowerObject.AddComponent(towerType);
            Component secondTower = secondTowerObject.AddComponent(towerType);
            object runtimeState = Activator.CreateInstance(stateType, new object[] { null });
            MethodInfo incrementCounter = stateType.GetMethod(
                "IncrementCounter",
                new[] { towerType, typeof(string) });

            Assert.NotNull(incrementCounter);
            Assert.AreEqual(1, incrementCounter.Invoke(runtimeState, new object[] { firstTower, "Attack" }));
            Assert.AreEqual(2, incrementCounter.Invoke(runtimeState, new object[] { firstTower, "Attack" }));
            Assert.AreEqual(1, incrementCounter.Invoke(runtimeState, new object[] { secondTower, "Attack" }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(firstTowerObject);
            UnityEngine.Object.DestroyImmediate(secondTowerObject);
        }
    }

    // 清理一座防御塔的计数时不应影响其他防御塔。
    [Test]
    public void RuntimeState_ClearCounters_DoesNotAffectOtherTower()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type stateType = GetRequiredType("DefenseTowerRewardRuntimeState");
        GameObject firstTowerObject = new("FirstTower");
        GameObject secondTowerObject = new("SecondTower");
        firstTowerObject.SetActive(false);
        secondTowerObject.SetActive(false);

        try
        {
            Component firstTower = firstTowerObject.AddComponent(towerType);
            Component secondTower = secondTowerObject.AddComponent(towerType);
            object runtimeState = Activator.CreateInstance(stateType, new object[] { null });
            MethodInfo incrementCounter = stateType.GetMethod("IncrementCounter");
            MethodInfo clearCounters = stateType.GetMethod("ClearCounters");

            Assert.NotNull(clearCounters, "运行时状态必须提供单塔计数清理入口");
            incrementCounter.Invoke(runtimeState, new object[] { firstTower, "Attack" });
            incrementCounter.Invoke(runtimeState, new object[] { secondTower, "Attack" });
            clearCounters.Invoke(runtimeState, new object[] { firstTower });

            Assert.AreEqual(1, incrementCounter.Invoke(runtimeState, new object[] { firstTower, "Attack" }));
            Assert.AreEqual(2, incrementCounter.Invoke(runtimeState, new object[] { secondTower, "Attack" }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(firstTowerObject);
            UnityEngine.Object.DestroyImmediate(secondTowerObject);
        }
    }

    // 攻击完成上下文应携带来源塔并累计合法的额外攻击请求。
    [Test]
    public void AttackCompletedContext_CarriesSourceAndAccumulatesExtraAttacks()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type healthType = GetRequiredType("HealthSystem");
        Type contextType = GetRequiredType("DefenseTowerAttackCompletedContext");
        GameObject towerObject = new("Tower");
        towerObject.SetActive(false);

        try
        {
            Component healthSystem = towerObject.AddComponent(healthType);
            Component tower = towerObject.AddComponent(towerType);
            object context = Activator.CreateInstance(contextType, tower, healthSystem);

            contextType.GetMethod("RequestExtraAttack").Invoke(context, new object[] { 2 });
            contextType.GetMethod("RequestExtraAttack").Invoke(context, new object[] { -1 });
            contextType.GetMethod("RequestExtraAttack").Invoke(context, new object[] { 3 });

            Assert.AreSame(tower, contextType.GetProperty("SourceDefenseTowerCombatSystem").GetValue(context));
            Assert.AreSame(healthSystem, contextType.GetProperty("SourceHealthSystem").GetValue(context));
            Assert.AreEqual(5, contextType.GetProperty("ExtraAttackCount").GetValue(context));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(towerObject);
        }
    }

    // 每个运行时奖励应用器只应实现自身需要的触发能力。
    [Test]
    public void RuntimeAppliers_ImplementOnlySupportedCapabilities()
    {
        AssertImplementsOnly(
            "DefenseTowerExtraArrowApplierSo",
            "IDefenseTowerAttackCompletedRewardTrigger");
        AssertImplementsOnly(
            "DefenseTowerAttackHealthCostApplierSo",
            "IDefenseTowerAttackCompletedRewardTrigger");
        AssertImplementsOnly(
            "DefenseTowerBurnArrowApplierSo",
            "IDefenseTowerArrowModifier");
        AssertImplementsOnly(
            "DefenseTowerChanceExplosionApplierSo",
            "IDefenseTowerArrowModifier");
        AssertImplementsOnly(
            "DefenseTowerPiercingArrowApplierSo",
            "IDefenseTowerArrowModifier");
        AssertImplementsOnly(
            "DefenseTowerPoisonArrowApplierSo",
            "IDefenseTowerArrowModifier");
        AssertImplementsOnly(
            "DefenseTowerKillGrowthApplierSo",
            "IDefenseTowerEnemyKilledRewardTrigger");
        AssertImplementsOnly(
            "DefenseTowerWaveEndHealApplierSo",
            "IDefenseTowerWaveCompletedRewardTrigger");
    }

    // 分发器只应暴露当前确有卡牌依赖的运行时节点。
    [Test]
    public void Dispatcher_ExposesOnlySupportedLifecycleMethods()
    {
        Type dispatcherType = GetRequiredType("DefenseTowerRewardTriggerDispatcher");

        Assert.NotNull(dispatcherType.GetMethod("RegisterEffect"));
        Assert.NotNull(dispatcherType.GetMethod("OnAttackCompleted"));
        Assert.NotNull(dispatcherType.GetMethod("ModifyArrow"));
        Assert.NotNull(dispatcherType.GetMethod("OnEnemyKilled"));
        Assert.NotNull(dispatcherType.GetMethod("OnWaveCompleted"));
        Assert.NotNull(dispatcherType.GetMethod("ClearSource"));

        Assert.IsNull(dispatcherType.GetMethod("ModifyStats"));
        Assert.IsNull(dispatcherType.GetMethod("OnBeforeAttack"));
        Assert.IsNull(dispatcherType.GetMethod("OnAfterAttack"));
        Assert.IsNull(dispatcherType.GetMethod("OnEnemyHit"));
    }

    // 额外攻击奖励应分别累计每座防御塔的主动攻击次数。
    [Test]
    public void ExtraArrowTrigger_CountsEachTowerIndependently()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type healthType = GetRequiredType("HealthSystem");
        Type contextType = GetRequiredType("DefenseTowerAttackCompletedContext");
        object dispatcher = Activator.CreateInstance(GetRequiredType("DefenseTowerRewardTriggerDispatcher"));
        ScriptableObject applier = ScriptableObject.CreateInstance(GetRequiredType("DefenseTowerExtraArrowApplierSo"));
        object config = CreateRewardConfig(
            ("TriggerAttackCount", 2f),
            ("ExtraAttackCount", 1f));
        GameObject firstTowerObject = new("FirstTower");
        GameObject secondTowerObject = new("SecondTower");
        firstTowerObject.SetActive(false);
        secondTowerObject.SetActive(false);

        try
        {
            Component firstHealth = firstTowerObject.AddComponent(healthType);
            Component secondHealth = secondTowerObject.AddComponent(healthType);
            Component firstTower = firstTowerObject.AddComponent(towerType);
            Component secondTower = secondTowerObject.AddComponent(towerType);
            MethodInfo registerEffect = dispatcher.GetType().GetMethod("RegisterEffect");
            MethodInfo onAttackCompleted = dispatcher.GetType().GetMethod("OnAttackCompleted");

            registerEffect.Invoke(dispatcher, new[] { applier, config });

            object firstAttack = Activator.CreateInstance(contextType, firstTower, firstHealth);
            object secondAttack = Activator.CreateInstance(contextType, secondTower, secondHealth);
            onAttackCompleted.Invoke(dispatcher, new[] { firstAttack });
            onAttackCompleted.Invoke(dispatcher, new[] { secondAttack });

            Assert.AreEqual(0, GetPropertyValue<int>(firstAttack, "ExtraAttackCount"));
            Assert.AreEqual(0, GetPropertyValue<int>(secondAttack, "ExtraAttackCount"));

            object firstThresholdAttack = Activator.CreateInstance(contextType, firstTower, firstHealth);
            onAttackCompleted.Invoke(dispatcher, new[] { firstThresholdAttack });

            Assert.AreEqual(1, GetPropertyValue<int>(firstThresholdAttack, "ExtraAttackCount"));
            Assert.AreEqual(0, GetPropertyValue<int>(secondAttack, "ExtraAttackCount"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(applier);
            UnityEngine.Object.DestroyImmediate(firstTowerObject);
            UnityEngine.Object.DestroyImmediate(secondTowerObject);
        }
    }

    // 攻击扣血奖励应分别累计每座防御塔的主动攻击次数。
    [Test]
    public void AttackHealthCostTrigger_CountsEachTowerIndependently()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type healthType = GetRequiredType("HealthSystem");
        Type contextType = GetRequiredType("DefenseTowerAttackCompletedContext");
        object dispatcher = Activator.CreateInstance(GetRequiredType("DefenseTowerRewardTriggerDispatcher"));
        ScriptableObject applier = ScriptableObject.CreateInstance(GetRequiredType("DefenseTowerAttackHealthCostApplierSo"));
        object config = CreateRewardConfig(
            ("TriggerAttackCount", 2f),
            ("AttackHealthCost", 2f));
        GameObject firstTowerObject = new("FirstTower");
        GameObject secondTowerObject = new("SecondTower");
        firstTowerObject.SetActive(false);
        secondTowerObject.SetActive(false);

        try
        {
            Component firstHealth = firstTowerObject.AddComponent(healthType);
            Component secondHealth = secondTowerObject.AddComponent(healthType);
            healthType.GetMethod("Init").Invoke(firstHealth, new object[] { 20 });
            healthType.GetMethod("Init").Invoke(secondHealth, new object[] { 20 });
            Component firstTower = firstTowerObject.AddComponent(towerType);
            Component secondTower = secondTowerObject.AddComponent(towerType);
            MethodInfo onAttackCompleted = dispatcher.GetType().GetMethod("OnAttackCompleted");
            dispatcher.GetType().GetMethod("RegisterEffect").Invoke(dispatcher, new[] { applier, config });

            onAttackCompleted.Invoke(dispatcher, new[] { Activator.CreateInstance(contextType, firstTower, firstHealth) });
            onAttackCompleted.Invoke(dispatcher, new[] { Activator.CreateInstance(contextType, secondTower, secondHealth) });
            Assert.AreEqual(20, GetPropertyValue<int>(firstHealth, "CurrentHealth"));
            Assert.AreEqual(20, GetPropertyValue<int>(secondHealth, "CurrentHealth"));

            onAttackCompleted.Invoke(dispatcher, new[] { Activator.CreateInstance(contextType, firstTower, firstHealth) });
            Assert.AreEqual(18, GetPropertyValue<int>(firstHealth, "CurrentHealth"));
            Assert.AreEqual(20, GetPropertyValue<int>(secondHealth, "CurrentHealth"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(applier);
            UnityEngine.Object.DestroyImmediate(firstTowerObject);
            UnityEngine.Object.DestroyImmediate(secondTowerObject);
        }
    }

    // 击杀成长奖励应分别累计每座防御塔造成的击杀。
    [Test]
    public void KillGrowthTrigger_CountsEachTowerIndependently()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type stateType = GetRequiredType("DefenseTowerRewardRuntimeState");
        Type contextType = GetRequiredType("DefenseTowerEnemyKilledContext");
        ScriptableObject applier = ScriptableObject.CreateInstance(GetRequiredType("DefenseTowerKillGrowthApplierSo"));
        object runtimeState = Activator.CreateInstance(
            stateType,
            new[] { CreateRewardConfig(("KillCountToUpgrade", 3f)) });
        GameObject firstTowerObject = new("FirstTower");
        GameObject secondTowerObject = new("SecondTower");
        firstTowerObject.SetActive(false);
        secondTowerObject.SetActive(false);

        try
        {
            Component firstTower = firstTowerObject.AddComponent(towerType);
            Component secondTower = secondTowerObject.AddComponent(towerType);
            MethodInfo onEnemyKilled = applier.GetType().GetMethod("OnEnemyKilled");
            MethodInfo incrementCounter = stateType.GetMethod("IncrementCounter");

            onEnemyKilled.Invoke(applier, new[]
            {
                runtimeState,
                Activator.CreateInstance(contextType, new object[] { firstTower })
            });
            onEnemyKilled.Invoke(applier, new[]
            {
                runtimeState,
                Activator.CreateInstance(contextType, new object[] { secondTower })
            });

            Assert.AreEqual(2, incrementCounter.Invoke(runtimeState, new object[] { firstTower, "KillAutoUpgrade" }));
            Assert.AreEqual(2, incrementCounter.Invoke(runtimeState, new object[] { secondTower, "KillAutoUpgrade" }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(applier);
            UnityEngine.Object.DestroyImmediate(firstTowerObject);
            UnityEngine.Object.DestroyImmediate(secondTowerObject);
        }
    }

    // 分发器清理来源塔后，该塔的奖励计数应重新开始累计。
    [Test]
    public void Dispatcher_ClearSource_ResetsTowerCounters()
    {
        Type towerType = GetRequiredType("DefenseTowerCombatSystem");
        Type contextType = GetRequiredType("DefenseTowerAttackCompletedContext");
        object dispatcher = Activator.CreateInstance(GetRequiredType("DefenseTowerRewardTriggerDispatcher"));
        ScriptableObject applier = ScriptableObject.CreateInstance(GetRequiredType("DefenseTowerExtraArrowApplierSo"));
        object config = CreateRewardConfig(
            ("TriggerAttackCount", 2f),
            ("ExtraAttackCount", 1f));
        GameObject towerObject = new("Tower");
        towerObject.SetActive(false);

        try
        {
            Component tower = towerObject.AddComponent(towerType);
            MethodInfo onAttackCompleted = dispatcher.GetType().GetMethod("OnAttackCompleted");
            dispatcher.GetType().GetMethod("RegisterEffect").Invoke(dispatcher, new[] { applier, config });

            object firstAttack = Activator.CreateInstance(contextType, tower, null);
            onAttackCompleted.Invoke(dispatcher, new[] { firstAttack });
            dispatcher.GetType().GetMethod("ClearSource").Invoke(dispatcher, new object[] { tower });

            object attackAfterClear = Activator.CreateInstance(contextType, tower, null);
            onAttackCompleted.Invoke(dispatcher, new[] { attackAfterClear });
            Assert.AreEqual(0, GetPropertyValue<int>(attackAfterClear, "ExtraAttackCount"));

            object thresholdAttack = Activator.CreateInstance(contextType, tower, null);
            onAttackCompleted.Invoke(dispatcher, new[] { thresholdAttack });
            Assert.AreEqual(1, GetPropertyValue<int>(thresholdAttack, "ExtraAttackCount"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(applier);
            UnityEngine.Object.DestroyImmediate(towerObject);
        }
    }

    // 波次完成事件应让每个已注册的波末奖励执行一次。
    [Test]
    public void Dispatcher_OnWaveCompleted_InvokesWaveHealOnce()
    {
        Type healthType = GetRequiredType("HealthSystem");
        Type buildingType = GetRequiredType("Building");
        Type registryType = GetRequiredType("DefenseTowerRegistry");
        object dispatcher = Activator.CreateInstance(GetRequiredType("DefenseTowerRewardTriggerDispatcher"));
        ScriptableObject applier = ScriptableObject.CreateInstance(GetRequiredType("DefenseTowerWaveEndHealApplierSo"));
        object config = CreateRewardConfig(("WaveEndHealPercent", 0.1f));
        GameObject buildingObject = new("DefenseBuilding");
        Component building = null;

        try
        {
            Component healthSystem = buildingObject.AddComponent(healthType);
            building = buildingObject.AddComponent(buildingType);
            buildingType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(building, Array.Empty<object>());
            healthType.GetMethod("Init").Invoke(healthSystem, new object[] { 100 });
            object cachedHealthSystem = buildingType
                .GetField("_healthSystem", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(building);
            Assert.NotNull(cachedHealthSystem, "测试建筑尚未执行 Awake，未缓存生命组件");
            registryType.GetMethod("RegisterDefenseBuilding").Invoke(null, new[] { building });
            healthType.GetMethod("LoseHealth").Invoke(healthSystem, new object[] { 50 });
            dispatcher.GetType().GetMethod("RegisterEffect").Invoke(dispatcher, new[] { applier, config });
            ICollection waveTriggerList = (ICollection)dispatcher.GetType()
                .GetField("_waveCompletedTriggerList", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(dispatcher);
            Assert.AreEqual(1, waveTriggerList.Count, "波末奖励未正确注册到分发器");

            dispatcher.GetType().GetMethod("OnWaveCompleted").Invoke(dispatcher, Array.Empty<object>());

            Assert.AreEqual(60, GetPropertyValue<int>(healthSystem, "CurrentHealth"));
        }
        finally
        {
            if (building)
            {
                registryType.GetMethod("UnregisterDefenseBuilding").Invoke(null, new[] { building });
            }

            UnityEngine.Object.DestroyImmediate(applier);
            UnityEngine.Object.DestroyImmediate(buildingObject);
        }
    }

    // 已移除的宽接口、空上下文和旧运行时状态类型不应继续保留。
    [Test]
    public void RemovedTriggerTypes_AreNoLongerAvailable()
    {
        string[] removedTypeNameArray =
        {
            "IDefenseTowerRewardTrigger",
            "DefenseTowerRuntimeRewardApplierSo",
            "DefenseTowerRewardTriggerInstance",
            "DefenseTowerStatsContext",
            "DefenseTowerAttackContext",
            "DefenseTowerEnemyHitContext",
            "DefenseTowerEnemyKillContext",
            "DefenseTowerWaveContext"
        };

        foreach (string removedTypeName in removedTypeNameArray)
        {
            Assert.IsNull(
                Type.GetType($"{removedTypeName}, Assembly-CSharp"),
                $"废弃类型仍然存在：{removedTypeName}");
        }

        Assert.NotNull(GetRequiredType("DefenseTowerArrowContext"));
        Assert.NotNull(GetRequiredType("DefenseTowerAttackCompletedContext"));
        Assert.NotNull(GetRequiredType("DefenseTowerEnemyKilledContext"));
    }

    // 验证应用器仅实现指定的防御塔运行时奖励能力。
    private static void AssertImplementsOnly(string applierTypeName, string expectedCapabilityTypeName)
    {
        Type runtimeRewardType = GetRequiredType("IDefenseTowerRuntimeReward");
        Type expectedCapabilityType = GetRequiredType(expectedCapabilityTypeName);
        Type applierType = GetRequiredType(applierTypeName);

        Assert.IsTrue(runtimeRewardType.IsAssignableFrom(applierType));
        Assert.IsTrue(expectedCapabilityType.IsAssignableFrom(applierType));

        string[] capabilityTypeNameArray =
        {
            "IDefenseTowerAttackCompletedRewardTrigger",
            "IDefenseTowerArrowModifier",
            "IDefenseTowerEnemyKilledRewardTrigger",
            "IDefenseTowerWaveCompletedRewardTrigger"
        };

        foreach (string capabilityTypeName in capabilityTypeNameArray)
        {
            if (capabilityTypeName == expectedCapabilityTypeName)
            {
                continue;
            }

            Type capabilityType = GetRequiredType(capabilityTypeName);
            Assert.IsFalse(
                capabilityType.IsAssignableFrom(applierType),
                $"{applierTypeName} 不应实现 {capabilityTypeName}");
        }
    }

    // 创建包含指定数值参数的奖励效果配置。
    private static object CreateRewardConfig(params (string parameterId, float value)[] parameterArray)
    {
        Type parameterType = GetRequiredType("RewardEffectParameterConfig");
        Type parameterListType = typeof(System.Collections.Generic.List<>).MakeGenericType(parameterType);
        IList parameterConfigList = (IList)Activator.CreateInstance(parameterListType);

        foreach ((string parameterId, float value) in parameterArray)
        {
            object parameterConfig = Activator.CreateInstance(parameterType);
            SetPrivateField(parameterConfig, "parameterId", parameterId);
            SetPrivateField(parameterConfig, "value", value);
            parameterConfigList.Add(parameterConfig);
        }

        object config = Activator.CreateInstance(GetRequiredType("RewardCardEffectConfig"));
        SetPrivateField(config, "parameterConfigList", parameterConfigList);
        return config;
    }

    // 获取对象的指定公开属性值。
    private static T GetPropertyValue<T>(object target, string propertyName)
    {
        return (T)target.GetType().GetProperty(propertyName).GetValue(target);
    }

    // 设置测试对象的私有字段。
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
