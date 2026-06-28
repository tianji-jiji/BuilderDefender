using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[Serializable]
public class PoolConfig
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 10;
    [SerializeField] private int maxSize = 30;
    [SerializeField] private Transform parent;

    public GameObject Prefab => prefab;
    public int MaxSize => Mathf.Max(1, maxSize);
    public int InitialSize => Mathf.Clamp(initialSize, 0, MaxSize);
    public Transform Parent => parent;
}

public class PoolManager : MonoBehaviour
{
    private const int DEFAULT_MAX_SIZE = 100;

    public static PoolManager Instance;

    [SerializeField] private List<PoolConfig> poolConfigs = new();
    [SerializeField] private Transform defaultPoolRoot;

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, PoolConfig> _configs = new();

    // 初始化对象池单例并预热配置中的对象池。
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogWarning($"场景中存在重复的 {nameof(PoolManager)}，已移除重复对象：{name}");
            DestroyDuplicateManager();
            return;
        }

        Instance = this;
        EnsureDefaultPoolRoot();
        InitializeConfiguredPools();
    }

    // 清理对象池单例引用。
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 从对象池生成指定预制体实例。
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!prefab)
        {
            return null;
        }

        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        GameObject instance = pool.Get();
        Transform instanceTransform = instance.transform;
        Transform spawnParent = GetSpawnParent(prefab, parent);

        instanceTransform.SetParent(spawnParent, false);
        instanceTransform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        if (instance.TryGetComponent(out PooledObject pooledObject))
        {
            pooledObject.MarkSpawned(this, prefab);
        }

        return instance;
    }

    // 从对象池生成指定组件类型的预制体实例。
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        if (!prefab)
        {
            return null;
        }

        GameObject instance = Spawn(prefab.gameObject, position, rotation, parent);
        return instance ? instance.GetComponent<T>() : null;
    }

    // 将对象实例回收到对应对象池。
    public void Release(GameObject instance)
    {
        if (!instance)
        {
            return;
        }

        if (!instance.TryGetComponent(out PooledObject pooledObject) || !pooledObject.SourcePrefab)
        {
            Destroy(instance);
            return;
        }

        if (pooledObject.IsInPool)
        {
            return;
        }

        GameObject sourcePrefab = pooledObject.SourcePrefab;
        ObjectPool<GameObject> pool = GetOrCreatePool(sourcePrefab);
        pooledObject.MarkDespawned();
        pool.Release(instance);
    }

    // 确保默认池根节点存在。
    private void EnsureDefaultPoolRoot()
    {
        if (defaultPoolRoot)
        {
            return;
        }

        GameObject root = new GameObject("PooledObjects");
        root.transform.SetParent(transform);
        defaultPoolRoot = root.transform;
    }

    // 初始化配置列表并执行预热。
    private void InitializeConfiguredPools()
    {
        foreach (PoolConfig config in poolConfigs)
        {
            if (config == null || !config.Prefab)
            {
                continue;
            }

            _configs[config.Prefab] = config;
            Prewarm(config.Prefab, config.InitialSize);
        }
    }

    // 预热指定数量的池对象。
    private void Prewarm(GameObject prefab, int count)
    {
        if (count <= 0)
        {
            return;
        }

        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        List<GameObject> instances = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            instances.Add(pool.Get());
        }

        foreach (GameObject instance in instances)
        {
            pool.Release(instance);
        }
    }

    // 获取指定预制体的对象池，不存在时按配置创建。
    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (_pools.TryGetValue(prefab, out ObjectPool<GameObject> existingPool))
        {
            return existingPool;
        }

        PoolConfig config = _configs.GetValueOrDefault(prefab);
        int maxSize = config?.MaxSize ?? DEFAULT_MAX_SIZE;
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            () => CreateInstance(prefab),
            null,
            ReleaseInstance,
            DestroyInstance,
            true,
            Mathf.Max(1, config?.InitialSize ?? 1),
            maxSize
        );

        _pools[prefab] = pool;
        return pool;
    }

    // 创建一个新的池对象实例。
    private GameObject CreateInstance(GameObject prefab)
    {
        Transform parent = GetPoolParent(prefab);
        GameObject instance = Instantiate(prefab, parent);
        instance.name = $"{prefab.name}_Pooled";

        if (!instance.TryGetComponent(out PooledObject pooledObject))
        {
            pooledObject = instance.AddComponent<PooledObject>();
        }

        pooledObject.Initialize(this, prefab);
        instance.SetActive(false);
        return instance;
    }

    // 回收对象时重置父节点和激活状态。
    private void ReleaseInstance(GameObject instance)
    {
        if (!instance)
        {
            return;
        }

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        Transform parent = pooledObject && pooledObject.SourcePrefab
            ? GetPoolParent(pooledObject.SourcePrefab)
            : defaultPoolRoot;

        instance.transform.SetParent(parent, false);
        instance.SetActive(false);
    }

    // 销毁超过容量上限的池对象。
    private void DestroyInstance(GameObject instance)
    {
        if (instance)
        {
            Destroy(instance);
        }
    }

    // 获取指定预制体回收时使用的父节点。
    private Transform GetPoolParent(GameObject prefab)
    {
        PoolConfig config = _configs.GetValueOrDefault(prefab);
        return config != null && config.Parent ? config.Parent : defaultPoolRoot;
    }

    // 获取对象取出时使用的父节点，未指定时保持在对象池根节点下。
    private Transform GetSpawnParent(GameObject prefab, Transform requestedParent)
    {
        return requestedParent ? requestedParent : GetPoolParent(prefab);
    }

    // 销毁重复的对象池管理器。
    private void DestroyDuplicateManager()
    {
        if (Application.isPlaying)
        {
            Destroy(gameObject);
            return;
        }

        DestroyImmediate(gameObject);
    }
}
