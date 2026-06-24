using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 防御塔箭矢能力测试，验证箭矢上下文能携带能力结算数据。
/// </summary>
public class DefenseTowerArrowAbilityTests
{
    // 箭矢上下文可以同时携带 DOT、爆炸和穿透能力。
    [Test]
    public void DefenseTowerArrowContext_CarriesAllAbilityPayloads()
    {
        Type contextType = GetRequiredType("DefenseTowerArrowContext");
        object context = Activator.CreateInstance(contextType, null, null, 10, 0f, false, 0f, 0f, null, false);
        object poisonSpec = CreateStatusSpec("Poison", 5f, 1f, 3, "Poison");
        object burnSpec = CreateStatusSpec("Burn", 4f, 1f, 4, "Burn");

        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { poisonSpec });
        contextType.GetMethod("AddStatusEffect")!.Invoke(context, new[] { burnSpec });
        contextType.GetMethod("SetChanceExplosion")!.Invoke(context, new object[] { 2.5f, 20 });
        contextType.GetMethod("SetPierceCount")!.Invoke(context, new object[] { 2 });

        ICollection statusEffectSpecList = (ICollection)contextType.GetProperty("StatusEffectSpecList")!.GetValue(context);
        Assert.AreEqual(2, statusEffectSpecList.Count);
        Assert.AreEqual(2.5f, (float)contextType.GetProperty("ChanceExplosionRadius")!.GetValue(context), 0.001f);
        Assert.AreEqual(20, contextType.GetProperty("ChanceExplosionDamage")!.GetValue(context));
        Assert.AreEqual(2, contextType.GetProperty("PierceCount")!.GetValue(context));
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
        arrow.GetType().GetMethod("SetDamage")!.Invoke(arrow, new object[] { 5 });
        SetArrowAttackContext(arrow, 2);

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
    private static object CreateStatusSpec(string effectTypeName, float duration, float tickInterval, int tickDamage, string floatingTextStyleName)
    {
        Type effectType = GetRequiredType("EnemyStatusEffectType");
        Type floatingTextStyle = GetRequiredType("DamageFloatingTextStyle");
        Type specType = GetRequiredType("EnemyStatusEffectSpec");
        return Activator.CreateInstance(
            specType,
            Enum.Parse(effectType, effectTypeName),
            duration,
            tickInterval,
            tickDamage,
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
    private static Component CreateEnemy(string name, int health)
    {
        GameObject enemyObject = new GameObject(name);
        enemyObject.SetActive(false);
        enemyObject.AddComponent<Rigidbody2D>();
        enemyObject.AddComponent<BoxCollider2D>();
        Component healthSystem = enemyObject.AddComponent(GetRequiredType("HealthSystem"));
        Component enemy = enemyObject.AddComponent(GetRequiredType("Enemy"));
        SetPrivateField(enemy, "healthSystem", healthSystem);
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

    // 设置箭矢本次攻击上下文。
    private static void SetArrowAttackContext(Component arrow, int pierceCount)
    {
        arrow.GetType().GetMethod(
            "SetAttackContext",
            new[]
            {
                GetRequiredType("DefenseTowerCombatSystem"),
                typeof(float),
                typeof(bool),
                typeof(float),
                typeof(float),
                typeof(IReadOnlyList<>).MakeGenericType(GetRequiredType("EnemyStatusEffectSpec")),
                typeof(float),
                typeof(int),
                typeof(int)
            })!.Invoke(arrow, new object[] { null, 0f, false, 0f, 0f, null, 0f, 0, pierceCount });
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
