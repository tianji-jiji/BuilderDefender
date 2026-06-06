using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour, IPoolable
{
    private const float DETECTION_INTERVAL = 0.2f;

    [SerializeField] private EnemySo enemySo;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform damageFloatingTextPoint;

    public static event Action OnEnemyDead;

    private Rigidbody2D _rb2;
    private PooledObject _pooledObject;
    private Transform _currentTarget;
    private Transform _defaultTarget;
    private float _timer;
    private float _randomMoveSpeed;
    private GameObject _enemyDiedParticles;
    private bool _isSubscribed;

    public bool IsAlive { get; private set; }
    public int Armor => enemySo ? Mathf.Max(0, enemySo.armor) : 0;
    public Vector3 DamageFloatingTextPosition => damageFloatingTextPoint ? damageFloatingTextPoint.position : transform.position;

    // 初始化敌人需要缓存的组件和默认攻击目标。
    private void Awake()
    {
        _enemyDiedParticles = Resources.Load<GameObject>("Particles/EnemyDieParticles");
        TryGetComponent(out _rb2);
        TryGetComponent(out _pooledObject);

        GameObject home = GameObject.FindGameObjectWithTag("Home");
        if (home)
        {
            Transform defaultTarget = home.transform.Find("DefaultTargetTransform");
            _defaultTarget = defaultTarget ? defaultTarget : home.transform;
        }

        _currentTarget = _defaultTarget;
    }

    // 初始化敌人的运行时配置。
    public void Init(EnemySo so)
    {
        enemySo = so;
        IsAlive = true;
        _timer = 0f;
        _currentTarget = _defaultTarget;
        _randomMoveSpeed = enemySo.moveSpeed * UnityEngine.Random.Range(0.9f, 1.1f);
        healthSystem.Init(enemySo.maxHealth);

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

    // 启用敌人时订阅血量系统死亡事件。
    private void OnEnable()
    {
        SubscribeHealthSystem();
    }

    // 固定帧处理敌人移动。
    private void FixedUpdate()
    {
        HandleMovement();
    }

    // 按固定间隔重新寻找攻击目标。
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DETECTION_INTERVAL)
        {
            FindTarget();
            _timer = 0f;
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

        Collider2D c2D = Physics2D.OverlapCircle(
            transform.position,
            enemySo.detectRadius,
            detectLayer
        );

        _currentTarget = c2D ? c2D.transform : _defaultTarget;
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
            int actualDamage = targetHealthSystem.TakeDamage(enemySo.atk);
            DamageFloatingTextService.ShowBuildingDamage(other.transform.position, actualDamage);
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

    // 取消订阅血量系统的死亡事件。
    private void OnDisable()
    {
        UnsubscribeHealthSystem();
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
