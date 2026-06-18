using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour, IPoolable
{
    private const float DETECTION_INTERVAL = 0.2f;
    private const int TARGET_DETECTION_BUFFER_SIZE = 16;

    [SerializeField] private EnemySo enemySo;
    [SerializeField] private LayerMask detectLayer;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform damageFloatingTextPoint;

    public static event Action OnEnemyDead;

    private Rigidbody2D _rb2;
    private PooledObject _pooledObject;
    private Transform _currentTarget;
    private Transform _defaultTarget;
    private readonly Collider2D[] _targetColliderArray = new Collider2D[TARGET_DETECTION_BUFFER_SIZE];
    private ContactFilter2D _targetContactFilter;
    private float _timer;
    private float _randomMoveSpeed;
    private GameObject _enemyDiedParticles;
    private bool _isSubscribed;
    private EnemyRuntimeStats _runtimeStats;

    public bool IsAlive { get; private set; }
    public int Armor => _runtimeStats.Armor;
    public Vector3 DamageFloatingTextPosition => damageFloatingTextPoint ? damageFloatingTextPoint.position : transform.position;

    private void Awake()
    {
        _enemyDiedParticles = Resources.Load<GameObject>("Particles/EnemyDieParticles");
        
        TryGetComponent(out _rb2);
        TryGetComponent(out _pooledObject);
        
        _targetContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false,
            layerMask = detectLayer
        };

        GameObject home = GameObject.FindGameObjectWithTag("Home");
        if (home)
        {
            Transform defaultTarget = home.transform.Find("DefaultTargetTransform");
            _defaultTarget = defaultTarget ? defaultTarget : home.transform;
        }

        _currentTarget = _defaultTarget;
    }
    
    private void OnEnable()
    {
        SubscribeHealthSystem();
    }
    
    private void OnDisable()
    {
        UnsubscribeHealthSystem();
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DETECTION_INTERVAL)
        {
            FindTarget();
            _timer = 0f;
        }
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    // 使用成长后的运行时属性初始化敌人。
    public void Init(EnemySo so, EnemyRuntimeStats runtimeStats)
    {
        enemySo = so;
        _runtimeStats = runtimeStats;
        
        RefreshTargetContactFilter();
        IsAlive = true;
        _timer = 0f;
        _currentTarget = _defaultTarget;
        _randomMoveSpeed = _runtimeStats.MoveSpeed * UnityEngine.Random.Range(0.9f, 1.1f);

        if (healthSystem)
        {
            healthSystem.Init(_runtimeStats.MaxHealth);
        }

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 重置敌人从对象池取出后的基础状态。
    public void OnSpawned()
    {
        TryGetComponent(out _pooledObject);
        IsAlive = false;
        _timer = 0f;
        _currentTarget = _defaultTarget;

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 清理敌人回池前的运行状态。
    public void OnDespawned()
    {
        IsAlive = false;
        _currentTarget = null;

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 根据当前目标移动敌人。
    private void HandleMovement()
    {
        if (!IsAlive || !_currentTarget)
        {
            return;
        }

        Vector3 moveDir = (_currentTarget.position - transform.position).normalized;
        _rb2.linearVelocity = moveDir * _randomMoveSpeed;
    }

    // 查找检测范围内的建筑目标，没有建筑时攻击默认基地目标。
    private void FindTarget()
    {
        if (!IsAlive || !enemySo)
        {
            return;
        }

        if (IsTargetValid())
        {
            return;
        }

        _currentTarget = FindNearestTarget() ?? _defaultTarget;
    }

    // 判断当前非基地目标是否仍然有效，避免索敌在多个碰撞体之间抖动。
    private bool IsTargetValid()
    {
        if (!_currentTarget || _currentTarget == _defaultTarget)
        {
            return false;
        }

        if (!_currentTarget.gameObject.activeInHierarchy)
        {
            return false;
        }

        float detectRadius = Mathf.Max(0f, _runtimeStats.DetectRadius);
        return (_currentTarget.position - transform.position).sqrMagnitude <= detectRadius * detectRadius;
    }

    // 从检测范围内选择距离最近的目标。
    private Transform FindNearestTarget()
    {
        RefreshTargetContactFilter();
        int hitCount = Physics2D.OverlapCircle(transform.position, _runtimeStats.DetectRadius, _targetContactFilter, _targetColliderArray);
        Transform nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D targetCollider = _targetColliderArray[i];
            _targetColliderArray[i] = null;

            if (!targetCollider || !targetCollider.gameObject.activeInHierarchy)
            {
                continue;
            }

            Transform target = targetCollider.transform;
            float distanceSqr = (target.position - transform.position).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestTarget = target;
            nearestDistanceSqr = distanceSqr;
        }

        return nearestTarget;
    }

    // 同步索敌层级过滤器，支持运行时或 Inspector 修改检测层。
    private void RefreshTargetContactFilter()
    {
        _targetContactFilter.useLayerMask = true;
        _targetContactFilter.useTriggers = false;
        _targetContactFilter.layerMask = detectLayer;
    }

    // 碰到建筑时造成伤害并销毁自身。
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsAlive)
        {
            return;
        }

        if (other.gameObject.layer != LayerMask.NameToLayer("Building"))
        {
            return;
        }

        if (other.gameObject.TryGetComponent(out HealthSystem targetHealthSystem))
        {
            int actualDamage = targetHealthSystem.TakeDamage(_runtimeStats.AttackDamage);
            DamageFloatingTextEvent.ShowDamage(other.transform.position, actualDamage);
        }

        Death();
    }

    // 处理敌人死亡时的回池、粒子和全局事件。
    private void Death()
    {
        if (!IsAlive)
        {
            return;
        }

        IsAlive = false;
        SpawnDeathParticles();
        OnEnemyDead?.Invoke();
        ReturnEnemy();
    }
   
    // 订阅血量系统死亡事件。
    private void SubscribeHealthSystem()
    {
        if (_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnDied += Death;
        _isSubscribed = true;
    }

    // 取消订阅血量系统死亡事件。
    private void UnsubscribeHealthSystem()
    {
        if (!_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnDied -= Death;
        _isSubscribed = false;
    }

    // 生成敌人死亡粒子。
    private void SpawnDeathParticles()
    {
        if (!_enemyDiedParticles)
        {
            return;
        }

        if (PoolManager.Instance)
        {
            PoolManager.Instance.Spawn(_enemyDiedParticles, transform.position, Quaternion.identity);
            return;
        }

        Instantiate(_enemyDiedParticles, transform.position, Quaternion.identity);
    }

    // 将敌人回收到对象池，缺少对象池时销毁对象。
    private void ReturnEnemy()
    {
        if (_pooledObject)
        {
            _pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}
