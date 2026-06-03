using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private PoolManager _poolManager;
    private IPoolable[] _poolables;

    public GameObject SourcePrefab { get; private set; }
    public bool IsInPool { get; private set; }

    // 初始化对象池实例与它对应的原始预制体。
    public void Initialize(PoolManager poolManager, GameObject sourcePrefab)
    {
        _poolManager = poolManager;
        SourcePrefab = sourcePrefab;
        CachePoolables();
    }

    // 将当前对象归还给对象池。
    public void ReturnToPool()
    {
        if (IsInPool)
        {
            return;
        }

        if (_poolManager)
        {
            _poolManager.Release(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    // 标记对象已经从对象池取出，并通知可池化组件。
    public void MarkSpawned(PoolManager poolManager, GameObject sourcePrefab)
    {
        _poolManager = poolManager;
        SourcePrefab = sourcePrefab;
        IsInPool = false;

        foreach (IPoolable poolable in _poolables)
        {
            poolable.OnSpawned();
        }
    }

    // 标记对象即将进入对象池，并通知可池化组件。
    public void MarkDespawned()
    {
        if (IsInPool)
        {
            return;
        }

        foreach (IPoolable poolable in _poolables)
        {
            poolable.OnDespawned();
        }

        IsInPool = true;
    }

    // 缓存当前对象上的池化生命周期组件。
    private void CachePoolables()
    {
        _poolables = GetComponents<IPoolable>();
    }
}
