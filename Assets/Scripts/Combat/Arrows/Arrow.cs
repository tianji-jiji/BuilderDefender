using System;
using UnityEngine;

/// <summary>
/// 箭矢投射物协调器，负责发射、生命周期、碰撞入口和对象池回收。
/// </summary>
public class Arrow : MonoBehaviour, IPoolable
{
    [SerializeField] private float flySpeed;
    [SerializeField] private float lifetime = 5f;
    
    private Rigidbody2D _rb2;
    private Collider2D _collider;
    private PooledObject _pooledObject;
    private ArrowFlightController _flightController;
    private ArrowHitAbilityPipeline _hitAbilityPipeline;

    private float _lifeTimer;
    private bool _isLaunched;
    private bool _isReturning;

    // 箭矢完成一次有效发射后触发。
    public event Action<ArrowLaunchData> OnLaunched;

    // 箭矢完成一次有效命中结算后触发。
    public event Action<ArrowHitContext, ArrowHitResolution> OnHitResolved;

    // 箭矢开始执行回池或销毁流程时触发。
    public event Action OnReturned;

    // 初始化箭矢需要缓存的组件引用。
    private void Awake()
    {
        TryGetComponent(out _rb2);
        TryGetComponent(out _collider);
        TryGetComponent(out _pooledObject);
        TryGetComponent(out _flightController);
        TryGetComponent(out _hitAbilityPipeline);
    }
    
    private void Update()
    {
        if (!_isLaunched || _isReturning)
        {
            return;
        }

        _lifeTimer += Time.deltaTime;

        if (_lifeTimer >= lifetime)
        {
            ReturnArrow();
        }
    }

    private void FixedUpdate()
    {
        if (!_isLaunched || _isReturning)
        {
            return;
        }

        if (!_flightController || !_flightController.TickFlight())
        {
            ReturnArrow();
        }
    }
    
    public void OnSpawned()
    {
        if (_collider)
        {
            _collider.enabled = true;
        }

        _lifeTimer = 0f;
        _flightController?.ResetState();
        _hitAbilityPipeline?.ResetState();
        _isLaunched = false;
        _isReturning = false;
        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    public void OnDespawned()
    {
        _flightController?.ResetState();
        _hitAbilityPipeline?.ResetState();
        _isLaunched = false;
        _isReturning = true;
        if (_collider)
        {
            _collider.enabled = false;
        }

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 使用完整发射快照启动箭矢。
    public void Launch(ArrowLaunchData launchData)
    {
        if (!_rb2 || !_collider)
        {
            Debug.LogError("箭矢发射失败：缺少 Rigidbody2D 或 Collider2D。", this);
            ReturnArrow();
            return;
        }

        if (!_flightController || !_hitAbilityPipeline)
        {
            Debug.LogError("箭矢发射失败：缺少飞行控制器或命中能力管线。", this);
            ReturnArrow();
            return;
        }

        if (!ArrowHitDamageApplier.IsEnemyValid(launchData.TargetEnemy))
        {
            Debug.LogError("箭矢发射失败：目标无效。", this);
            ReturnArrow();
            return;
        }

        if (!_hitAbilityPipeline || !_flightController
            || !_flightController.TryStart(
                launchData.FlightBehaviorType,
                _rb2,
                launchData.TargetEnemy,
                flySpeed))
        {
            ReturnArrow();
            return;
        }

        _hitAbilityPipeline.Configure(launchData);
        _isLaunched = true;
        OnLaunched?.Invoke(launchData);
    }

    // 处理箭头命中敌人后的命中流程和回收。
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isLaunched || _isReturning)
        {
            return;
        }

        if (!Enemy.TryGetFromCollider(other, out Enemy enemy))
        {
            return;
        }

        if (!_hitAbilityPipeline.TryResolveHit(
                enemy,
                transform.position,
                out ArrowHitContext hitContext,
                out ArrowHitResolution hitResolution))
        {
            return;
        }

        OnHitResolved?.Invoke(hitContext, hitResolution);
        if (!hitResolution.ShouldContinueFlight)
        {
            ReturnArrow();
            return;
        }

        _flightController.ContinueAfterHit();
    }

    // 将箭头回收到对象池，减少对象销毁。
    private void ReturnArrow()
    {
        if (_isReturning)
        {
            return;
        }

        _isReturning = true;
        _isLaunched = false;
        OnReturned?.Invoke();

        if (_pooledObject)
        {
            _pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }

}
