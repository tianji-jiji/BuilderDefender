using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticleAutoReturn : MonoBehaviour, IPoolable
{
    [SerializeField] private float fallbackLifetime = 2f;

    private readonly List<ParticleSystem> _particleSystemList = new();
    private PooledObject _pooledObject;
    private Coroutine _returnCoroutine;

    // 播放粒子并安排自动回池。
    public void OnSpawned()
    {
        CacheReferences();
        StopReturnCoroutine();

        RestartParticles();

        _returnCoroutine = StartCoroutine(ReturnAfterLifetime());
    }

    // 停止粒子和等待协程，清理回池状态。
    public void OnDespawned()
    {
        CacheReferences();
        StopReturnCoroutine();

        StopParticles();
    }

    // 缓存粒子和池对象引用。
    private void CacheReferences()
    {
        CacheParticleSystems();

        if (!_pooledObject)
        {
            TryGetComponent(out _pooledObject);
        }
    }

    // 缓存根对象、子对象和子发射器引用到的粒子系统。
    private void CacheParticleSystems()
    {
        _particleSystemList.Clear();
        GetComponentsInChildren(true, _particleSystemList);

        for (int i = 0; i < _particleSystemList.Count; i++)
        {
            AddSubEmitterParticleSystems(_particleSystemList[i]);
        }
    }

    // 补充缓存粒子系统的子发射器，避免只统计层级中的粒子。
    private void AddSubEmitterParticleSystems(ParticleSystem particleSystem)
    {
        ParticleSystem.SubEmittersModule subEmitters = particleSystem.subEmitters;
        if (!subEmitters.enabled)
        {
            return;
        }

        for (int i = 0; i < subEmitters.subEmittersCount; i++)
        {
            ParticleSystem subEmitter = subEmitters.GetSubEmitterSystem(i);
            if (!subEmitter || _particleSystemList.Contains(subEmitter))
            {
                continue;
            }

            _particleSystemList.Add(subEmitter);
        }
    }

    // 重新播放所有缓存到的粒子系统。
    private void RestartParticles()
    {
        foreach (ParticleSystem particleSystem in _particleSystemList)
        {
            if (particleSystem)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        foreach (ParticleSystem particleSystem in _particleSystemList)
        {
            if (particleSystem)
            {
                particleSystem.Play(true);
            }
        }
    }

    // 停止所有缓存到的粒子系统。
    private void StopParticles()
    {
        foreach (ParticleSystem particleSystem in _particleSystemList)
        {
            if (particleSystem)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    // 等待粒子播放结束后回收到对象池。
    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(GetMaxLifetime());

        while (HasAliveParticles())
        {
            yield return null;
        }

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

        if (_particleSystemList.Count == 0)
        {
            return maxLifetime;
        }

        foreach (ParticleSystem particleSystem in _particleSystemList)
        {
            maxLifetime = Mathf.Max(maxLifetime, GetParticleLifetime(particleSystem));
        }

        return maxLifetime;
    }

    // 计算单个粒子系统包含拖尾后的最长播放时间。
    private float GetParticleLifetime(ParticleSystem particleSystem)
    {
        if (!particleSystem)
        {
            return fallbackLifetime;
        }

        ParticleSystem.MainModule main = particleSystem.main;
        if (main.loop)
        {
            return fallbackLifetime;
        }

        float lifetime = main.duration + main.startDelay.constantMax + main.startLifetime.constantMax;
        ParticleSystem.TrailModule trails = particleSystem.trails;
        if (trails.enabled)
        {
            lifetime += trails.lifetime.constantMax;
        }

        return lifetime;
    }

    // 判断是否还有非循环粒子正在播放或存活。
    private bool HasAliveParticles()
    {
        foreach (ParticleSystem particleSystem in _particleSystemList)
        {
            if (!particleSystem || particleSystem.main.loop)
            {
                continue;
            }

            if (particleSystem.IsAlive(true))
            {
                return true;
            }
        }

        return false;
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
