using System.Collections;
using UnityEngine;

public class PooledParticleAutoReturn : MonoBehaviour, IPoolable
{
    [SerializeField] private float fallbackLifetime = 2f;

    private ParticleSystem _particleSystem;
    private PooledObject _pooledObject;
    private Coroutine _returnCoroutine;

    // 播放粒子并安排自动回池。
    public void OnSpawned()
    {
        CacheReferences();
        StopReturnCoroutine();

        if (_particleSystem)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Play(true);
        }

        _returnCoroutine = StartCoroutine(ReturnAfterLifetime());
    }

    // 停止粒子和等待协程，清理回池状态。
    public void OnDespawned()
    {
        CacheReferences();
        StopReturnCoroutine();

        if (_particleSystem)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // 缓存粒子和池对象引用。
    private void CacheReferences()
    {
        if (!_particleSystem)
        {
            TryGetComponent(out _particleSystem);
        }

        if (!_pooledObject)
        {
            TryGetComponent(out _pooledObject);
        }
    }

    // 等待粒子播放结束后回收到对象池。
    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(GetMaxLifetime());

        if (_pooledObject)
        {
            _pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 计算当前粒子系统最长播放时间。
    private float GetMaxLifetime()
    {
        float maxLifetime = fallbackLifetime;

        if (!_particleSystem)
        {
            return maxLifetime;
        }

        ParticleSystem.MainModule main = _particleSystem.main;
        if (main.loop)
        {
            return maxLifetime;
        }

        float lifetime = main.duration + main.startLifetime.constantMax;
        maxLifetime = Mathf.Max(maxLifetime, lifetime);

        return maxLifetime;
    }

    // 停止当前自动回池协程。
    private void StopReturnCoroutine()
    {
        if (_returnCoroutine == null)
        {
            return;
        }

        StopCoroutine(_returnCoroutine);
        _returnCoroutine = null;
    }
}
