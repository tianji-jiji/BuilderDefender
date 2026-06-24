using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 战斗状态效果测试，验证持续伤害状态的刷新和清理规则。
/// </summary>
public class CombatStatusEffectTests
{
    private static string CapturedFloatingTextValue;
    private static bool CapturedForceColor;
    private static Gradient CapturedGradient;

    // 重复应用同类状态时刷新剩余时间，不叠加层数。
    [Test]
    public void ApplyStatus_RefreshesExistingStatusInsteadOfStacking()
    {
        GameObject enemyObject = new GameObject("Enemy");
        Type controllerType = GetRequiredType("EnemyStatusEffectController");
        Component controller = enemyObject.AddComponent(controllerType);
        object poisonType = Enum.Parse(GetRequiredType("EnemyStatusEffectType"), "Poison");
        object poisonStyle = Enum.Parse(GetRequiredType("DamageFloatingTextStyle"), "Poison");
        object firstSpec = CreateStatusSpec(poisonType, 5f, 1f, 3, poisonStyle);
        object secondSpec = CreateStatusSpec(poisonType, 8f, 1f, 3, poisonStyle);

        controllerType.GetMethod("ApplyStatus")!.Invoke(controller, new[] { firstSpec });
        controllerType.GetMethod("ApplyStatus")!.Invoke(controller, new[] { secondSpec });

        Assert.AreEqual(1, controllerType.GetProperty("ActiveStatusCount")!.GetValue(controller));
        object[] tryGetDurationArgs = { poisonType, null };
        bool hasDuration = (bool)controllerType.GetMethod("TryGetRemainingDuration")!.Invoke(controller, tryGetDurationArgs);
        Assert.IsTrue(hasDuration);
        float remainingDuration = (float)tryGetDurationArgs[1];
        Assert.AreEqual(8f, remainingDuration, 0.001f);

        UnityEngine.Object.DestroyImmediate(enemyObject);
    }

    // 中毒伤害飘字事件应同时携带 Feel 强制颜色和 TMP 富文本颜色。
    [Test]
    public void ShowDamage_WithPoisonStyle_BroadcastsColoredFloatingText()
    {
        Type floatingTextStyleType = GetRequiredType("DamageFloatingTextStyle");
        object poisonStyle = Enum.Parse(floatingTextStyleType, "Poison");
        Type damageFloatingTextEventType = GetRequiredType("DamageFloatingTextEvent");
        Delegate handler = CreateFloatingTextCaptureDelegate();

        RegisterFloatingTextHandler(handler);
        damageFloatingTextEventType.GetMethod("ShowDamage", new[] { typeof(Vector3), typeof(int), floatingTextStyleType })!
            .Invoke(null, new[] { Vector3.zero, 3, poisonStyle });
        UnregisterFloatingTextHandler(handler);

        Color expectedColor = (Color)damageFloatingTextEventType.GetMethod("GetStyleColor")!.Invoke(null, new[] { poisonStyle });
        Color actualColor = CapturedGradient.Evaluate(0f);

        Assert.IsTrue(CapturedForceColor);
        Assert.IsTrue(CapturedFloatingTextValue.StartsWith("<color=#", StringComparison.Ordinal));
        Assert.IsTrue(CapturedFloatingTextValue.EndsWith(">3</color>", StringComparison.Ordinal));
        Assert.AreEqual(expectedColor.r, actualColor.r, 0.001f);
        Assert.AreEqual(expectedColor.g, actualColor.g, 0.001f);
        Assert.AreEqual(expectedColor.b, actualColor.b, 0.001f);
    }

    // 状态跳伤击杀敌人并触发回池清理时，本轮状态更新不应访问已清空的字典。
    [Test]
    public void TickStatuses_WhenDamageKillsEnemyAndClearsStatuses_DoesNotThrow()
    {
        Component enemy = CreateEnemy("Enemy", 1);
        Component controller = enemy.GetComponent(GetRequiredType("EnemyStatusEffectController"));
        Component healthSystem = enemy.GetComponent(GetRequiredType("HealthSystem"));
        Type effectType = GetRequiredType("EnemyStatusEffectType");
        Type floatingTextStyleType = GetRequiredType("DamageFloatingTextStyle");
        object poisonSpec = CreateStatusSpec(Enum.Parse(effectType, "Poison"), 5f, 1f, 1, Enum.Parse(floatingTextStyleType, "Poison"));
        object burnSpec = CreateStatusSpec(Enum.Parse(effectType, "Burn"), 5f, 1f, 1, Enum.Parse(floatingTextStyleType, "Burn"));

        AddHealthDiedHandler(healthSystem, controller);
        controller.GetType().GetMethod("ApplyStatus")!.Invoke(controller, new[] { poisonSpec });
        controller.GetType().GetMethod("ApplyStatus")!.Invoke(controller, new[] { burnSpec });

        Assert.DoesNotThrow(() => controller.GetType()
            .GetMethod("TickStatuses", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(controller, new object[] { 1f }));

        UnityEngine.Object.DestroyImmediate(enemy.gameObject);
    }

    // 重复刷新同类状态时保留下一次跳伤计时，避免连续命中导致跳伤被一直延后。
    [Test]
    public void ApplyStatus_WhenRefreshingExistingStatus_PreservesNextTickTimer()
    {
        Component enemy = CreateEnemy("Enemy", 20);
        Component controller = enemy.GetComponent(GetRequiredType("EnemyStatusEffectController"));
        Type effectType = GetRequiredType("EnemyStatusEffectType");
        Type floatingTextStyleType = GetRequiredType("DamageFloatingTextStyle");
        object firstSpec = CreateStatusSpec(Enum.Parse(effectType, "Burn"), 5f, 1f, 4, Enum.Parse(floatingTextStyleType, "Burn"));
        object secondSpec = CreateStatusSpec(Enum.Parse(effectType, "Burn"), 5f, 1f, 4, Enum.Parse(floatingTextStyleType, "Burn"));

        controller.GetType().GetMethod("ApplyStatus")!.Invoke(controller, new[] { firstSpec });
        TickStatuses(controller, 0.5f);
        controller.GetType().GetMethod("ApplyStatus")!.Invoke(controller, new[] { secondSpec });
        TickStatuses(controller, 0.5f);

        Assert.AreEqual(16, GetEnemyCurrentHealth(enemy));

        UnityEngine.Object.DestroyImmediate(enemy.gameObject);
    }

    // 状态已经超过持续时间时不应再补一次跳伤，避免过期状态产生无来源飘字。
    [Test]
    public void TickStatuses_WhenDurationEndsBeforeTickInterval_DoesNotDealExpiredDamage()
    {
        Component enemy = CreateEnemy("Enemy", 20);
        Component controller = enemy.GetComponent(GetRequiredType("EnemyStatusEffectController"));
        Type effectType = GetRequiredType("EnemyStatusEffectType");
        Type floatingTextStyleType = GetRequiredType("DamageFloatingTextStyle");
        object burnSpec = CreateStatusSpec(Enum.Parse(effectType, "Burn"), 0.5f, 1f, 4, Enum.Parse(floatingTextStyleType, "Burn"));

        controller.GetType().GetMethod("ApplyStatus")!.Invoke(controller, new[] { burnSpec });
        TickStatuses(controller, 1f);

        Assert.AreEqual(20, GetEnemyCurrentHealth(enemy));

        UnityEngine.Object.DestroyImmediate(enemy.gameObject);
    }

    // 创建状态配置实例。
    private static object CreateStatusSpec(object effectType, float duration, float tickInterval, int tickDamage, object floatingTextStyle)
    {
        Type specType = GetRequiredType("EnemyStatusEffectSpec");
        return Activator.CreateInstance(specType, effectType, duration, tickInterval, tickDamage, null, floatingTextStyle);
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        return Type.GetType($"{typeName}, Assembly-CSharp", true);
    }

    // 创建用于捕获 Feel 浮动文字事件参数的委托。
    private static Delegate CreateFloatingTextCaptureDelegate()
    {
        Type delegateType = GetRequiredRuntimeType("MoreMountains.Feedbacks.MMFloatingTextSpawnEvent+Delegate");
        MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
        ParameterInfo[] parameterInfoArray = invokeMethod.GetParameters();
        Type[] parameterTypeArray = new Type[parameterInfoArray.Length];
        for (int i = 0; i < parameterInfoArray.Length; i++)
        {
            parameterTypeArray[i] = parameterInfoArray[i].ParameterType;
        }

        DynamicMethod dynamicMethod = new(
            "CaptureFloatingTextEvent",
            typeof(void),
            parameterTypeArray,
            typeof(CombatStatusEffectTests).Module,
            true);
        ILGenerator generator = dynamicMethod.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_2);
        generator.Emit(OpCodes.Ldarg_S, 7);
        generator.Emit(OpCodes.Ldarg_S, 8);
        generator.Emit(OpCodes.Call, typeof(CombatStatusEffectTests).GetMethod(nameof(CaptureFloatingTextEvent), BindingFlags.Static | BindingFlags.NonPublic));
        generator.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate(delegateType);
    }

    // 注册 Feel 浮动文字事件捕获器。
    private static void RegisterFloatingTextHandler(Delegate handler)
    {
        ResetFloatingTextCapture();
        GetFloatingTextSpawnEventType().GetMethod("Register")!.Invoke(null, new object[] { handler });
    }

    // 取消注册 Feel 浮动文字事件捕获器。
    private static void UnregisterFloatingTextHandler(Delegate handler)
    {
        GetFloatingTextSpawnEventType().GetMethod("Unregister")!.Invoke(null, new object[] { handler });
    }

    // 保存最近一次 Feel 浮动文字事件参数。
    private static void CaptureFloatingTextEvent(string value, bool forceColor, Gradient animateColorGradient)
    {
        CapturedFloatingTextValue = value;
        CapturedForceColor = forceColor;
        CapturedGradient = animateColorGradient;
    }

    // 清理浮动文字事件捕获结果。
    private static void ResetFloatingTextCapture()
    {
        CapturedFloatingTextValue = string.Empty;
        CapturedForceColor = false;
        CapturedGradient = null;
    }

    // 获取 Feel 浮动文字事件类型。
    private static Type GetFloatingTextSpawnEventType()
    {
        return GetRequiredRuntimeType("MoreMountains.Feedbacks.MMFloatingTextSpawnEvent");
    }

    // 在当前域的所有程序集中查找运行时类型。
    private static Type GetRequiredRuntimeType(string fullName)
    {
        Type type = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, false))
            .FirstOrDefault(foundType => foundType != null);
        Assert.IsNotNull(type, $"Type not found: {fullName}");
        return type;
    }

    // 创建测试用敌人。
    private static Component CreateEnemy(string name, int health)
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
        enemy.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(enemy, Array.Empty<object>());
        enemyObject.SetActive(true);

        object runtimeStats = Activator.CreateInstance(
            GetRequiredType("EnemyRuntimeStats"),
            health,
            0,
            1,
            1f,
            1f);
        enemy.GetType().GetMethod("Init")!.Invoke(enemy, new[] { null, runtimeStats });
        return enemy;
    }

    // 让生命归零时直接清空状态，稳定复现 tick 过程中状态字典被清理的场景。
    private static void AddHealthDiedHandler(Component healthSystem, Component statusEffectController)
    {
        EventInfo onDiedEvent = healthSystem.GetType().GetEvent("OnDied");
        Delegate clearAllHandler = Delegate.CreateDelegate(typeof(Action), statusEffectController, statusEffectController.GetType().GetMethod("ClearAll")!);
        onDiedEvent!.AddEventHandler(healthSystem, clearAllHandler);
    }

    // 推进状态控制器的内部跳伤计时。
    private static void TickStatuses(Component controller, float deltaTime)
    {
        controller.GetType()
            .GetMethod("TickStatuses", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(controller, new object[] { deltaTime });
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
}
