using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BuildingRefactorTests
{
    private PoolManager _originalPoolManager;

    // 保存测试前的对象池单例，避免粒子生成逻辑影响其他测试。
    [SetUp]
    public void SetUp()
    {
        _originalPoolManager = PoolManager.Instance;
        PoolManager.Instance = null;
    }

    // 清理测试对象并还原对象池单例。
    [TearDown]
    public void TearDown()
    {
        PoolManager.Instance = _originalPoolManager;

        foreach (GameObject gameObject in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.name.StartsWith("BuildingRefactorTest_"))
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }

    // 验证建造完成粒子改为 Inspector 序列化引用。
    [Test]
    public void BuildingConstructor_UsesSerializedPlacedParticlePrefab()
    {
        FieldInfo placedParticleField = GetPrivateField(typeof(BuildingConstructor), "placedParticlePrefab");
        FieldInfo oldParticleField = typeof(BuildingConstructor).GetField("particlePrefab", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(placedParticleField, "BuildingConstructor should expose placedParticlePrefab as a serialized field.");
        Assert.AreEqual(typeof(GameObject), placedParticleField.FieldType);
        Assert.IsNull(oldParticleField, "BuildingConstructor should not keep the old runtime-loaded particlePrefab field.");
    }

    // 验证建筑销毁粒子改为 Inspector 序列化引用。
    [Test]
    public void Building_UsesSerializedDestroyedParticlePrefab()
    {
        FieldInfo destroyedParticleField = GetPrivateField(typeof(Building), "destroyedParticlePrefab");
        FieldInfo oldParticleField = typeof(Building).GetField("_buildingDestroyedParticles", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(destroyedParticleField, "Building should expose destroyedParticlePrefab as a serialized field.");
        Assert.AreEqual(typeof(GameObject), destroyedParticleField.FieldType);
        Assert.IsNull(oldParticleField, "Building should not keep the old runtime-loaded _buildingDestroyedParticles field.");
    }

    // 验证重复启用不会重复订阅死亡事件。
    [Test]
    public void Building_OnEnableTwice_DeathEventInvokedOnce()
    {
        BuildingSo buildingSo = ScriptableObject.CreateInstance<BuildingSo>();
        buildingSo.maxHealth = 5;

        GameObject buildingObject = new GameObject("BuildingRefactorTest_Building");
        HealthSystem healthSystem = buildingObject.AddComponent<HealthSystem>();
        Building building = buildingObject.AddComponent<Building>();
        SetPrivateField(building, "buildingSo", buildingSo);
        SetPrivateField(building, "destroyedParticlePrefab", null);
        InvokePrivateMethod(building, "Awake");

        int destroyedEventCount = 0;
        Action<BuildingSo, Vector3> handler = (destroyedBuildingSo, _) =>
        {
            if (destroyedBuildingSo == buildingSo)
            {
                destroyedEventCount++;
            }
        };

        Building.OnBuildingDestroyed += handler;
        try
        {
            InvokePrivateMethod(building, "OnEnable");
            InvokePrivateMethod(building, "OnEnable");

            LogAssert.Expect(LogType.Error, "Destroy may not be called from edit mode! Use DestroyImmediate instead.\nDestroying an object in edit mode destroys it permanently.");
            healthSystem.TakeDamage(10);

            Assert.AreEqual(1, destroyedEventCount);
        }
        finally
        {
            Building.OnBuildingDestroyed -= handler;
            ScriptableObject.DestroyImmediate(buildingSo);
        }
    }

    // 验证建造器完成后会生成正式建筑并调用完成回调。
    [Test]
    public void BuildingConstructor_CompleteConstruction_SpawnsBuildingAndInvokesCallback()
    {
        BuildingSo buildingSo = ScriptableObject.CreateInstance<BuildingSo>();
        buildingSo.assetName = "BuildingRefactorTest_FormalBuilding";
        buildingSo.constructionTime = 0.01f;
        buildingSo.prefab = new GameObject("BuildingRefactorTest_FormalBuilding");

        GameObject constructorObject = new GameObject("BuildingRefactorTest_Constructor");
        BuildingConstructor buildingConstructor = constructorObject.AddComponent<BuildingConstructor>();
        SetPrivateField(buildingConstructor, "buildingSpriteList", new List<SpriteRenderer>());
        SetPrivateField(buildingConstructor, "placedParticlePrefab", null);

        bool completed = false;
        buildingConstructor.Init(buildingSo, () => completed = true);

        LogAssert.Expect(LogType.Error, "Destroy may not be called from edit mode! Use DestroyImmediate instead.\nDestroying an object in edit mode destroys it permanently.");
        InvokePrivateMethod(buildingConstructor, "CompleteConstruction");

        Assert.IsTrue(completed);
        Assert.NotNull(GameObject.Find("BuildingRefactorTest_FormalBuilding(Clone)"));

        ScriptableObject.DestroyImmediate(buildingSo);
    }

    // 获取指定类型的私有字段。
    private FieldInfo GetPrivateField(Type type, string fieldName)
    {
        return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
    }

    // 设置私有字段，模拟 Inspector 序列化数据。
    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = GetPrivateField(target.GetType(), fieldName);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    // 调用私有生命周期方法。
    private void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo methodInfo = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(methodInfo, $"Missing method {methodName}");
        methodInfo.Invoke(target, null);
    }
}
