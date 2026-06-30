using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Tower 箭矢能力测试，验证箭矢上下文能携带能力结算数据。
/// </summary>
public class TowerArrowAbilityTests
{
    private static int LaunchedEventCount;
    private static int HitResolvedEventCount;
    private static int ReturnedEventCount;

    // 箭矢只公开原子化发射入口，不再保留分步配置接口。
    [Test]
    public void Arrow_UsesAtomicLaunchApiWithoutLegacySetters()
    {
        Type arrowType = GetRequiredType("Arrow");
        Type launchDataType = GetRequiredType("ArrowLaunchData");

        Assert.NotNull(arrowType.GetMethod("Launch", new[] { launchDataType }));
        Assert.IsNull(arrowType.GetMethod("SetDamage"));
        Assert.IsNull(arrowType.GetMethod("SetAttackContext"));
        Assert.IsNull(arrowType.GetMethod("SetTarget"));
    }

    // 箭矢上下文可以构建同时携带状态、两种爆炸、穿透和飞行行为的发射快照。
    [Test]
    public void TowerArrowContext_BuildsCompositeLaunchData()
    {
        Type contextType = GetRequiredType("TowerArrowContext");
        object context = Activator.CreateInstance(contextType, null, null, 10, 0f, true, 3f, 0.5f);
        object poisonSpec = CreateStatusSpec("Poison", 5f, 1f, 0.03f, "Poison");
        object burnSpec = CreateStatusSpec("Burn", 4f, 1f, 0.04f, "Burn");
        Type flightBehaviorType = GetRequiredType("ArrowFlightBehaviorType");
        object homingBehavior = Enum.Parse(flightBehaviorType, "Homing");

        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { poisonSpec });
        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { burnSpec });
        contextType.GetMethod("SetChanceExplosion")!.Invoke(context, new object[] { 2.5f, 20 });
        contextType.GetMethod("SetPierceCount")!.Invoke(context, new object[] { 2 });
        contextType.GetMethod("SetFlightBehavior")!.Invoke(context, new[] { homingBehavior });

        object launchData = contextType.GetMethod("BuildLaunchData")!.Invoke(context, Array.Empty<object>());
        Type launchDataType = launchData!.GetType();
        ICollection statusEffectSpecList = (ICollection)launchDataType.GetProperty("StatusEffectSpecList")!.GetValue(launchData);
        IList areaDamageSpecList = (IList)launchDataType.GetProperty("AreaDamageSpecList")!.GetValue(launchData);

        Assert.AreEqual(2, statusEffectSpecList.Count);
        Assert.AreEqual(2, areaDamageSpecList.Count);
        Assert.AreEqual("FixedRaw", areaDamageSpecList[0].GetType().GetProperty("DamageMode")!.GetValue(areaDamageSpecList[0])!.ToString());
        Assert.AreEqual("BaseDamageMultiplier", areaDamageSpecList[1].GetType().GetProperty("DamageMode")!.GetValue(areaDamageSpecList[1])!.ToString());
        Assert.AreEqual(2, launchDataType.GetProperty("PierceCount")!.GetValue(launchData));
        Assert.AreEqual(homingBehavior, launchDataType.GetProperty("FlightBehaviorType")!.GetValue(launchData));
    }

    // 飞行控制器根据发射类型选择追踪行为，并朝有效目标更新刚体速度。
    [Test]
    public void ArrowFlightController_WithHomingBehavior_MovesTowardTarget()
    {
        GameObject arrowObject = new GameObject("ArrowFlight");
        Rigidbody2D rigidbody2D = arrowObject.AddComponent<Rigidbody2D>();
        Type homingBehaviorType = GetRequiredType("HomingArrowFlightBehavior");
        Type flightControllerType = GetRequiredType("ArrowFlightController");
        Component homingBehavior = arrowObject.AddComponent(homingBehaviorType);
        Component flightController = arrowObject.AddComponent(flightControllerType);
        Component targetEnemy = CreateEnemy("FlightTarget", 20);
        targetEnemy.transform.position = new Vector3(10f, 0f, 0f);

        InvokePrivateMethod(homingBehavior, "Awake");
        InvokePrivateMethod(flightController, "Awake");
        object homingType = Enum.Parse(GetRequiredType("ArrowFlightBehaviorType"), "Homing");
        bool started = (bool)flightControllerType.GetMethod("TryStart")!.Invoke(
            flightController,
            new object[] { homingType, rigidbody2D, targetEnemy, 12f });
        bool canContinue = (bool)flightControllerType.GetMethod("TickFlight")!.Invoke(
            flightController,
            Array.Empty<object>());

        Assert.IsTrue(started);
        Assert.IsTrue(canContinue);
        Assert.AreEqual(12f, rigidbody2D.linearVelocity.x, 0.001f);
        Assert.AreEqual(0f, rigidbody2D.linearVelocity.y, 0.001f);

        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(arrowObject);
    }

    // 追踪目标失效后保持最后有效方向继续飞行。
    [Test]
    public void HomingArrowFlightBehavior_WhenTargetBecomesInvalid_ContinuesForward()
    {
        GameObject arrowObject = new GameObject("ArrowFlight");
        Rigidbody2D rigidbody2D = arrowObject.AddComponent<Rigidbody2D>();
        Type homingBehaviorType = GetRequiredType("HomingArrowFlightBehavior");
        Type flightControllerType = GetRequiredType("ArrowFlightController");
        Component homingBehavior = arrowObject.AddComponent(homingBehaviorType);
        Component flightController = arrowObject.AddComponent(flightControllerType);
        Component targetEnemy = CreateEnemy("FlightTarget", 20);
        targetEnemy.transform.position = new Vector3(10f, 0f, 0f);

        InvokePrivateMethod(homingBehavior, "Awake");
        InvokePrivateMethod(flightController, "Awake");
        object homingType = Enum.Parse(GetRequiredType("ArrowFlightBehaviorType"), "Homing");
        flightControllerType.GetMethod("TryStart")!.Invoke(
            flightController,
            new object[] { homingType, rigidbody2D, targetEnemy, 12f });
        flightControllerType.GetMethod("TickFlight")!.Invoke(flightController, Array.Empty<object>());
        targetEnemy.gameObject.SetActive(false);
        bool canContinue = (bool)flightControllerType.GetMethod("TickFlight")!.Invoke(
            flightController,
            Array.Empty<object>());

        Assert.IsTrue(canContinue);
        Assert.AreEqual(12f, rigidbody2D.linearVelocity.x, 0.001f);
        Assert.AreEqual(0f, rigidbody2D.linearVelocity.y, 0.001f);

        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(arrowObject);
    }

    // 飞行控制器缺少请求的行为组件时明确报错并拒绝启动。
    [Test]
    public void ArrowFlightController_WithoutRequestedBehavior_LogsErrorAndReturnsFalse()
    {
        GameObject arrowObject = new GameObject("ArrowFlight");
        Rigidbody2D rigidbody2D = arrowObject.AddComponent<Rigidbody2D>();
        Type flightControllerType = GetRequiredType("ArrowFlightController");
        Component flightController = arrowObject.AddComponent(flightControllerType);
        Component targetEnemy = CreateEnemy("FlightTarget", 20);
        InvokePrivateMethod(flightController, "Awake");
        object homingType = Enum.Parse(GetRequiredType("ArrowFlightBehaviorType"), "Homing");

        LogAssert.Expect(LogType.Error, "箭矢缺少飞行行为组件：Homing");
        bool started = (bool)flightControllerType.GetMethod("TryStart")!.Invoke(
            flightController,
            new object[] { homingType, rigidbody2D, targetEnemy, 12f });

        Assert.IsFalse(started);

        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(arrowObject);
    }

    // 启用中的敌人通过碰撞体缓存解析，不依赖命中热路径组件查找。
    [Test]
    public void EnemyColliderRegistry_WithActiveEnemy_ResolvesCachedEnemy()
    {
        Component enemy = CreateEnemy("CachedEnemy", 20);
        Type enemyType = GetRequiredType("Enemy");
        MethodInfo tryGetFromCollider = enemyType.GetMethod("TryGetFromCollider", BindingFlags.Static | BindingFlags.Public);
        object[] arguments = { enemy.GetComponent<Collider2D>(), null };

        bool found = (bool)tryGetFromCollider!.Invoke(null, arguments);

        Assert.IsTrue(found);
        Assert.AreSame(enemy, arguments[1]);

        UnityEngine.Object.DestroyImmediate(enemy.gameObject);
    }

    // 组合命中能力按既有语义同时施加状态、固定爆炸和倍率爆炸。
    [Test]
    public void Arrow_WithCompositeHitAbilities_AppliesExpectedDamageAndStatuses()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManagerObject.GetComponent(GetRequiredType("PoolManager")), sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component directEnemy = CreateEnemy("DirectEnemy", 100, 100);
        Component nearbyEnemy = CreateEnemy("NearbyEnemy", 100, 100);
        nearbyEnemy.transform.position = new Vector3(1f, 0f, 0f);
        Physics2D.SyncTransforms();

        InvokePrivateMethod(arrow, "Awake");
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        object launchData = CreateCompositeLaunchData(directEnemy);
        arrow.GetType().GetMethod("Launch")!.Invoke(arrow, new[] { launchData });
        arrow.GetType().GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(arrow, new object[] { directEnemy.GetComponent<Collider2D>() });

        Component statusEffectController = directEnemy.GetComponent(GetRequiredType("EnemyStatusEffectController"));
        Assert.AreEqual(75, GetEnemyCurrentHealth(directEnemy));
        Assert.AreEqual(78, GetEnemyCurrentHealth(nearbyEnemy));
        Assert.AreEqual(2, statusEffectController.GetType().GetProperty("ActiveStatusCount")!.GetValue(statusEffectController));

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(directEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(nearbyEnemy.gameObject);
    }

    // 箭矢发射、命中和回池时分别广播一次已完成事件。
    [Test]
    public void Arrow_LifecycleEvents_AreRaisedOnce()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManagerObject.GetComponent(GetRequiredType("PoolManager")), sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component targetEnemy = CreateEnemy("EventTarget", 20);
        Type arrowType = arrow.GetType();
        ResetArrowEventCounts();

        RegisterArrowEvent(arrow, arrowType.GetEvent("OnLaunched"), nameof(CaptureLaunchedEvent));
        RegisterArrowEvent(arrow, arrowType.GetEvent("OnHitResolved"), nameof(CaptureHitResolvedEvent));
        RegisterArrowEvent(arrow, arrowType.GetEvent("OnReturned"), nameof(CaptureReturnedEvent));
        InvokePrivateMethod(arrow, "Awake");
        arrowType.GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LaunchArrow(arrow, targetEnemy, 5, 0);
        arrowType.GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(arrow, new object[] { targetEnemy.GetComponent<Collider2D>() });

        Assert.AreEqual(1, LaunchedEventCount);
        Assert.AreEqual(1, HitResolvedEventCount);
        Assert.AreEqual(1, ReturnedEventCount);

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
    }

    // 缺少核心协调组件时发射失败并安全进入回池状态。
    [Test]
    public void Arrow_WithoutFlightController_LogsErrorAndReturnsSafely()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManagerObject.GetComponent(GetRequiredType("PoolManager")), sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component flightController = arrowObject.GetComponent(GetRequiredType("ArrowFlightController"));
        Component targetEnemy = CreateEnemy("MissingControllerTarget", 20);
        UnityEngine.Object.DestroyImmediate(flightController);

        InvokePrivateMethod(arrow, "Awake");
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LogAssert.Expect(LogType.Error, "箭矢发射失败：缺少飞行控制器或命中能力管线。");
        LaunchArrow(arrow, targetEnemy, 5, 0);

        bool isReturning = (bool)arrow.GetType()
            .GetField("_isReturning", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(arrow);
        Assert.IsTrue(isReturning);

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
    }

    // 发射目标无效时记录明确错误并安全进入回池状态。
    [Test]
    public void Arrow_WithInvalidTarget_LogsErrorAndReturnsSafely()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManagerObject.GetComponent(GetRequiredType("PoolManager")), sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component targetEnemy = CreateEnemy("InvalidTarget", 20);
        targetEnemy.gameObject.SetActive(false);

        InvokePrivateMethod(arrow, "Awake");
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LogAssert.Expect(LogType.Error, "箭矢发射失败：目标无效。");
        LaunchArrow(arrow, targetEnemy, 5, 0);

        bool isReturning = (bool)arrow.GetType()
            .GetField("_isReturning", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(arrow);
        Assert.IsTrue(isReturning);

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(targetEnemy.gameObject);
    }

    // 同一敌人的重复碰撞不会重复结算伤害或消耗穿透次数。
    [Test]
    public void Arrow_WhenSameEnemyTriggersTwice_ResolvesHitOnlyOnce()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        Component poolManager = poolManagerObject.GetComponent(GetRequiredType("PoolManager"));
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManager, sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component firstEnemy = CreateEnemy("FirstEnemy", 20);
        Component secondEnemy = CreateEnemy("SecondEnemy", 20);
        MethodInfo onTriggerEnter = arrow.GetType().GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);

        InvokePrivateMethod(arrow, "Awake");
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LaunchArrow(arrow, firstEnemy, 5, 1);
        onTriggerEnter!.Invoke(arrow, new object[] { firstEnemy.GetComponent<Collider2D>() });
        onTriggerEnter.Invoke(arrow, new object[] { firstEnemy.GetComponent<Collider2D>() });
        onTriggerEnter.Invoke(arrow, new object[] { secondEnemy.GetComponent<Collider2D>() });

        Assert.AreEqual(15, GetEnemyCurrentHealth(firstEnemy));
        Assert.AreEqual(15, GetEnemyCurrentHealth(secondEnemy));

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(firstEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(secondEnemy.gameObject);
    }

    // 池对象再次发射时清空上一轮命中记录和剩余穿透次数。
    [Test]
    public void Arrow_AfterPoolReuse_ClearsHitHistoryAndPierceState()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        Component poolManager = poolManagerObject.GetComponent(GetRequiredType("PoolManager"));
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManager, sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        Component firstEnemy = CreateEnemy("FirstEnemy", 20);
        Component secondEnemy = CreateEnemy("SecondEnemy", 20);
        MethodInfo onTriggerEnter = arrow.GetType().GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);

        InvokePrivateMethod(arrow, "Awake");
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LaunchArrow(arrow, firstEnemy, 5, 1);
        onTriggerEnter!.Invoke(arrow, new object[] { firstEnemy.GetComponent<Collider2D>() });
        onTriggerEnter.Invoke(arrow, new object[] { secondEnemy.GetComponent<Collider2D>() });

        GameObject reusedArrowObject = SpawnPooledObject(poolManager, sourcePrefab);
        Component reusedArrow = reusedArrowObject.GetComponent(GetRequiredType("Arrow"));
        LaunchArrow(reusedArrow, firstEnemy, 5, 0);
        MethodInfo reusedOnTriggerEnter = reusedArrow.GetType().GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        reusedOnTriggerEnter!.Invoke(reusedArrow, new object[] { firstEnemy.GetComponent<Collider2D>() });
        reusedOnTriggerEnter.Invoke(reusedArrow, new object[] { secondEnemy.GetComponent<Collider2D>() });

        Assert.AreSame(arrowObject, reusedArrowObject);
        Assert.AreEqual(10, GetEnemyCurrentHealth(firstEnemy));
        Assert.AreEqual(15, GetEnemyCurrentHealth(secondEnemy));

        UnityEngine.Object.DestroyImmediate(reusedArrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        UnityEngine.Object.DestroyImmediate(firstEnemy.gameObject);
        UnityEngine.Object.DestroyImmediate(secondEnemy.gameObject);
    }

    // 伤害执行方法不应暴露无人使用的返回值。
    [Test]
    public void ArrowHitDamageApplier_DamageMethodsReturnVoid()
    {
        Type damageApplierType = GetRequiredType("ArrowHitDamageApplier");
        string[] methodNameArray =
        {
            "ApplyDamage",
            "ApplyRawDamage",
            "ApplyMaxHealthPercentDamage"
        };

        foreach (string methodName in methodNameArray)
        {
            MethodInfo method = damageApplierType.GetMethod(methodName);
            Assert.NotNull(method, $"未找到伤害执行方法：{methodName}");
            Assert.AreEqual(typeof(void), method.ReturnType, $"{methodName} 不应返回未被消费的伤害值");
        }
    }

    // 穿透数量为 2 时，本支箭最多处理 3 个不同敌人，第 4 个敌人不会受到伤害。
    [Test]
    public void Arrow_WithTwoPierces_DoesNotDamageFourthEnemy()
    {
        GameObject poolManagerObject = CreatePoolManagerObject();
        GameObject sourcePrefab = new GameObject("ArrowSourcePrefab");
        GameObject arrowObject = CreateArrowObject(poolManagerObject.GetComponent(GetRequiredType("PoolManager")), sourcePrefab);
        Component arrow = arrowObject.GetComponent(GetRequiredType("Arrow"));
        MethodInfo onTriggerEnter = arrow.GetType().GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        Component[] enemyArray = CreateEnemyArray(4, 20);

        arrow.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(arrow, Array.Empty<object>());
        arrow.GetType().GetMethod("OnSpawned")!.Invoke(arrow, Array.Empty<object>());
        LaunchArrow(arrow, enemyArray[0], 5, 2);

        foreach (Component enemy in enemyArray)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            onTriggerEnter!.Invoke(arrow, new object[] { enemyCollider });
        }

        Assert.AreEqual(15, GetEnemyCurrentHealth(enemyArray[0]));
        Assert.AreEqual(15, GetEnemyCurrentHealth(enemyArray[1]));
        Assert.AreEqual(15, GetEnemyCurrentHealth(enemyArray[2]));
        Assert.AreEqual(20, GetEnemyCurrentHealth(enemyArray[3]));

        UnityEngine.Object.DestroyImmediate(arrowObject);
        UnityEngine.Object.DestroyImmediate(sourcePrefab);
        UnityEngine.Object.DestroyImmediate(poolManagerObject);
        foreach (Component enemy in enemyArray)
        {
            UnityEngine.Object.DestroyImmediate(enemy.gameObject);
        }
    }

    // 创建状态配置实例。
    private static object CreateStatusSpec(string effectTypeName, float duration, float tickInterval, float tickDamagePercent, string floatingTextStyleName)
    {
        Type effectType = GetRequiredType("EnemyStatusEffectType");
        Type floatingTextStyle = GetRequiredType("DamageFloatingTextStyle");
        Type specType = GetRequiredType("EnemyStatusEffectSpec");
        return Activator.CreateInstance(
            specType,
            Enum.Parse(effectType, effectTypeName),
            duration,
            tickInterval,
            tickDamagePercent,
            null,
            Enum.Parse(floatingTextStyle, floatingTextStyleName));
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        return Type.GetType($"{typeName}, Assembly-CSharp", true);
    }

    // 创建用于命中流程测试的箭矢对象。
    private static GameObject CreateArrowObject(Component poolManager, GameObject sourcePrefab)
    {
        GameObject arrowObject = new GameObject("Arrow");
        arrowObject.AddComponent<Rigidbody2D>();
        BoxCollider2D collider = arrowObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        Component homingBehavior = arrowObject.AddComponent(GetRequiredType("HomingArrowFlightBehavior"));
        Component flightController = arrowObject.AddComponent(GetRequiredType("ArrowFlightController"));
        arrowObject.AddComponent(GetRequiredType("ArrowStatusHitEffect"));
        arrowObject.AddComponent(GetRequiredType("ArrowAreaDamageHitEffect"));
        arrowObject.AddComponent(GetRequiredType("ArrowPierceContinuation"));
        Component hitAbilityPipeline = arrowObject.AddComponent(GetRequiredType("ArrowHitAbilityPipeline"));
        InvokePrivateMethod(homingBehavior, "Awake");
        InvokePrivateMethod(flightController, "Awake");
        InvokePrivateMethod(hitAbilityPipeline, "Awake");
        arrowObject.AddComponent(GetRequiredType("Arrow"));
        Component pooledObject = arrowObject.AddComponent(GetRequiredType("PooledObject"));
        pooledObject.GetType().GetMethod("Initialize")!.Invoke(pooledObject, new object[] { poolManager, sourcePrefab });
        return arrowObject;
    }

    // 创建测试用对象池管理器。
    private static GameObject CreatePoolManagerObject()
    {
        GameObject poolManagerObject = new GameObject("PoolManager");
        Component poolManager = poolManagerObject.AddComponent(GetRequiredType("PoolManager"));
        poolManager.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(poolManager, Array.Empty<object>());
        return poolManagerObject;
    }

    // 从测试对象池再次取出指定来源预制体对应的对象。
    private static GameObject SpawnPooledObject(Component poolManager, GameObject sourcePrefab)
    {
        MethodInfo spawnMethod = poolManager.GetType().GetMethod(
            "Spawn",
            new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion), typeof(Transform) });
        return (GameObject)spawnMethod!.Invoke(
            poolManager,
            new object[] { sourcePrefab, Vector3.zero, Quaternion.identity, null });
    }

    // 创建一组可被箭矢命中的敌人对象。
    private static Component[] CreateEnemyArray(int count, int health)
    {
        Component[] enemyArray = new Component[count];
        for (int i = 0; i < count; i++)
        {
            enemyArray[i] = CreateEnemy($"Enemy_{i}", health);
        }

        return enemyArray;
    }

    // 创建单个可被命中的敌人对象。
    private static Component CreateEnemy(string name, int health, int armor = 0)
    {
        GameObject enemyObject = new GameObject(name);
        enemyObject.SetActive(false);
        enemyObject.AddComponent<Rigidbody2D>();
        enemyObject.AddComponent<BoxCollider2D>();
        Component healthSystem = enemyObject.AddComponent(GetRequiredType("HealthSystem"));
        Component statusEffectController = enemyObject.AddComponent(GetRequiredType("EnemyStatusEffectController"));
        Component enemy = enemyObject.AddComponent(GetRequiredType("Enemy"));
        SetPrivateField(enemy, "healthSystem", healthSystem);
        SetPrivateField(enemy, "statusEffectController", statusEffectController);
        InvokePrivateMethod(enemy, "Awake");
        enemyObject.SetActive(true);
        InvokePrivateMethod(enemy, "OnEnable");

        object runtimeStats = Activator.CreateInstance(
            GetRequiredType("EnemyRuntimeStats"),
            health,
            armor,
            1,
            1f,
            1f);
        enemy.GetType().GetMethod("Init")!.Invoke(enemy, new[] { null, runtimeStats });
        return enemy;
    }

    // 构建同时包含两种状态和两种爆炸的测试发射快照。
    private static object CreateCompositeLaunchData(Component targetEnemy)
    {
        Type contextType = GetRequiredType("TowerArrowContext");
        object context = Activator.CreateInstance(contextType, null, targetEnemy, 10, 0f, true, 3f, 0.5f);
        object poisonSpec = CreateStatusSpec("Poison", 5f, 1f, 0.03f, "Poison");
        object burnSpec = CreateStatusSpec("Burn", 4f, 1f, 0.04f, "Burn");
        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { poisonSpec });
        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { burnSpec });
        contextType.GetMethod("SetChanceExplosion")!.Invoke(context, new object[] { 3f, 20 });
        return contextType.GetMethod("BuildLaunchData")!.Invoke(context, Array.Empty<object>());
    }

    // 使用完整发射快照启动测试箭矢。
    private static void LaunchArrow(Component arrow, Component targetEnemy, int damage, int pierceCount)
    {
        Type launchDataType = GetRequiredType("ArrowLaunchData");
        object launchData = Activator.CreateInstance(
            launchDataType,
            null,
            targetEnemy,
            damage,
            0f,
            null,
            null,
            pierceCount,
            Enum.Parse(GetRequiredType("ArrowFlightBehaviorType"), "Homing"));
        arrow.GetType().GetMethod("Launch", new[] { launchDataType })!.Invoke(arrow, new[] { launchData });
    }

    // 获取敌人当前生命值。
    private static int GetEnemyCurrentHealth(Component enemy)
    {
        Component healthSystem = enemy.GetComponent(GetRequiredType("HealthSystem"));
        return (int)healthSystem.GetType().GetProperty("CurrentHealth")!.GetValue(healthSystem);
    }

    // 设置私有序列化字段，模拟 Inspector 引用。
    private static void SetPrivateField(Component component, string fieldName, object value)
    {
        component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(component, value);
    }

    // 调用组件上的无参数私有方法。
    private static void InvokePrivateMethod(Component component, string methodName)
    {
        component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(component, Array.Empty<object>());
    }

    // 为箭矢事件注册一个忽略参数的计数委托。
    private static void RegisterArrowEvent(Component arrow, EventInfo eventInfo, string captureMethodName)
    {
        Assert.NotNull(eventInfo, $"未找到箭矢事件：{captureMethodName}");
        MethodInfo invokeMethod = eventInfo!.EventHandlerType!.GetMethod("Invoke");
        ParameterInfo[] parameterInfoArray = invokeMethod!.GetParameters();
        Type[] parameterTypeArray = new Type[parameterInfoArray.Length];
        for (int i = 0; i < parameterInfoArray.Length; i++)
        {
            parameterTypeArray[i] = parameterInfoArray[i].ParameterType;
        }

        DynamicMethod dynamicMethod = new(
            $"Capture_{captureMethodName}",
            typeof(void),
            parameterTypeArray,
            typeof(TowerArrowAbilityTests).Module,
            true);
        ILGenerator generator = dynamicMethod.GetILGenerator();
        generator.Emit(
            OpCodes.Call,
            typeof(TowerArrowAbilityTests).GetMethod(captureMethodName, BindingFlags.Static | BindingFlags.NonPublic));
        generator.Emit(OpCodes.Ret);
        eventInfo.AddEventHandler(arrow, dynamicMethod.CreateDelegate(eventInfo.EventHandlerType));
    }

    // 重置箭矢生命周期事件计数。
    private static void ResetArrowEventCounts()
    {
        LaunchedEventCount = 0;
        HitResolvedEventCount = 0;
        ReturnedEventCount = 0;
    }

    // 记录一次箭矢发射事件。
    private static void CaptureLaunchedEvent()
    {
        LaunchedEventCount++;
    }

    // 记录一次箭矢命中完成事件。
    private static void CaptureHitResolvedEvent()
    {
        HitResolvedEventCount++;
    }

    // 记录一次箭矢回池事件。
    private static void CaptureReturnedEvent()
    {
        ReturnedEventCount++;
    }
}
