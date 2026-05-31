using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    private const float DETECTION_INTERVAL = 0.2f;

    [SerializeField] private EnemySo enemySo;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private HealthSystem healthSystem;

    public static event Action OnEnemyDead;

    private Rigidbody2D _rb2;
    private Transform _currentTarget;
    private Transform _defaultTarget;
    private float _timer;
    private float _randomMoveSpeed;
    private GameObject _enemyDiedParticles;

    // 初始化敌人需要缓存的组件和默认攻击目标。
    private void Awake()
    {
        _enemyDiedParticles = Resources.Load<GameObject>("Particles/EnemyDieParticles");
        _rb2 = GetComponent<Rigidbody2D>();

        GameObject home = GameObject.FindGameObjectWithTag("Home");
        _defaultTarget = home.transform.Find("DefaultTargetTransform");
        _currentTarget = _defaultTarget;
    }

    // 初始化敌人的运行时配置。
    public void Init(EnemySo so)
    {
        enemySo = so;
        _randomMoveSpeed = enemySo.moveSpeed * UnityEngine.Random.Range(0.9f, 1.1f);
        healthSystem.Init(enemySo.maxHealth);
    }

    // 订阅血量系统的死亡事件。
    private void Start()
    {
        healthSystem.OnDied += Death;
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
        if (!_currentTarget)
        {
            return;
        }

        Vector3 moveDir = (_currentTarget.position - transform.position).normalized;
        _rb2.velocity = moveDir * _randomMoveSpeed;
    }

    // 查找检测范围内的建筑目标，没有建筑时攻击默认基地目标。
    private void FindTarget()
    {
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
        if (other.gameObject.layer != LayerMask.NameToLayer("Building"))
        {
            return;
        }

        other.gameObject.GetComponent<HealthSystem>().TakeDamage(enemySo.atk);
        Debug.Log($"对{other.gameObject.name}造成{enemySo.atk}点伤害");
        Death();
    }

    // 处理敌人死亡时的销毁、粒子和全局事件。
    private void Death()
    {
        Destroy(gameObject);
        Instantiate(_enemyDiedParticles, transform.position, Quaternion.identity);
        OnEnemyDead?.Invoke();
    }

    // 取消订阅血量系统的死亡事件。
    private void OnDisable()
    {
        healthSystem.OnDied -= Death;
    }
}
