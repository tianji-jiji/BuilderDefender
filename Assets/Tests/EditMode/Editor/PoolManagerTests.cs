using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PoolManagerTests
{
    private PoolManager _originalInstance;

    // 保存测试前的对象池单例，避免影响其他测试。
    [SetUp]
    public void SetUp()
    {
        _originalInstance = PoolManager.Instance;
        PoolManager.Instance = null;
    }

    // 清理测试创建的对象，并还原对象池单例。
    [TearDown]
    public void TearDown()
    {
        PoolManager.Instance = _originalInstance;
    }

    // 验证粒子对象不会被对象池管理器自动添加自动回池组件。
    [Test]
    public void Spawn_ParticlePrefab_DoesNotAutoAddParticleAutoReturn()
    {
        PoolManager poolManager = CreatePoolManager("PoolManager");
        GameObject particlePrefab = new GameObject("ParticlePrefab");
        particlePrefab.AddComponent<ParticleSystem>();

        GameObject spawnedObject = poolManager.Spawn(particlePrefab, Vector3.zero, Quaternion.identity);

        Assert.NotNull(spawnedObject);
        Assert.IsFalse(spawnedObject.TryGetComponent(out PooledParticleAutoReturn _));

        Object.DestroyImmediate(spawnedObject);
        Object.DestroyImmediate(particlePrefab);
        Object.DestroyImmediate(poolManager.gameObject);
    }

    // 验证重复对象池管理器不会覆盖已存在的单例。
    [Test]
    public void Awake_DuplicatePoolManager_DoesNotReplaceInstance()
    {
        PoolManager firstManager = CreatePoolManager("FirstPoolManager");
        InvokeAwake(firstManager);
        PoolManager secondManager = CreatePoolManager("SecondPoolManager");
        InvokeAwake(secondManager);

        Assert.AreSame(firstManager, PoolManager.Instance);

        DestroyManager(secondManager);
        DestroyManager(firstManager);
    }

    // 验证预热数量不会超过对象池容量上限。
    [Test]
    public void PoolConfig_InitialSizeGreaterThanMaxSize_ClampsInitialSize()
    {
        PoolConfig config = new PoolConfig();
        SetField(config, "initialSize", 5);
        SetField(config, "maxSize", 2);

        Assert.AreEqual(2, config.MaxSize);
        Assert.AreEqual(2, config.InitialSize);
    }

    // 验证粒子自动回池会统计子粒子系统的播放时间。
    [Test]
    public void PooledParticleAutoReturn_GetMaxLifetime_IncludesChildParticleSystem()
    {
        GameObject rootObject = new GameObject("ParticleRoot");
        ParticleSystem rootParticle = rootObject.AddComponent<ParticleSystem>();
        GameObject childObject = new GameObject("ParticleChild");
        childObject.transform.SetParent(rootObject.transform);
        ParticleSystem childParticle = childObject.AddComponent<ParticleSystem>();
        PooledParticleAutoReturn autoReturn = rootObject.AddComponent<PooledParticleAutoReturn>();

        SetParticleLifetime(rootParticle, 0.5f, 0.5f);
        SetParticleLifetime(childParticle, 3f, 1.5f);
        InvokePrivateMethod(autoReturn, "CacheReferences");

        float maxLifetime = (float)InvokePrivateMethod(autoReturn, "GetMaxLifetime");

        Assert.GreaterOrEqual(maxLifetime, 4.5f);

        Object.DestroyImmediate(rootObject);
    }

    // 创建测试用对象池管理器。
    private PoolManager CreatePoolManager(string objectName)
    {
        GameObject managerObject = new GameObject(objectName);
        return managerObject.AddComponent<PoolManager>();
    }

    // 手动调用 Awake，以便 EditMode 测试验证运行时初始化逻辑。
    private void InvokeAwake(PoolManager poolManager)
    {
        MethodInfo awakeMethodInfo = typeof(PoolManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(awakeMethodInfo, "Missing Awake method");
        awakeMethodInfo.Invoke(poolManager, null);
    }

    // 销毁测试用对象池管理器。
    private void DestroyManager(PoolManager poolManager)
    {
        if (poolManager)
        {
            Object.DestroyImmediate(poolManager.gameObject);
        }
    }

    // 设置私有序列化字段以构建测试数据。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    // 设置测试粒子的基础生命周期。
    private void SetParticleLifetime(ParticleSystem particleSystem, float duration, float startLifetime)
    {
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = false;
        main.duration = duration;
        main.startLifetime = startLifetime;
    }

    // 调用测试目标上的私有方法。
    private object InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo methodInfo = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(methodInfo, $"Missing method {methodName}");
        return methodInfo.Invoke(target, null);
    }
}
