using UnityEngine;

/// <summary>
/// 箭矢投射物本体，负责飞行、追踪、命中判断和对象池生命周期。
/// </summary>
public class Arrow : MonoBehaviour, IPoolable
{
    private const int MAX_EFFECT_HIT_COUNT = 32;

    [SerializeField] private float flySpeed;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private ArrowVisual arrowVisual;
    [SerializeField] private ArrowEffectSo explosiveArrowEffect;

    private readonly Collider2D[] _effectHitResults = new Collider2D[MAX_EFFECT_HIT_COUNT];
    
    private Rigidbody2D _rb2;
    private Collider2D _collider;
    private PooledObject _pooledObject;
    
    private DefenseSystem _sourceDefenseSystem;
    private Enemy _targetEnemy;
    private Transform _targetTransform;
    private Vector2 _moveDirection;
    
    private float _lifeTimer;
    private float _armorIgnorePercent;
    private float _explosionRadius;
    private float _explosionDamageMultiplier;
    private int _damage;
    private bool _isExplosiveArrow;
    private bool _isReturning;

    // 初始化箭矢需要缓存的组件引用。
    private void Awake()
    {
        TryGetComponent(out _rb2);
        TryGetComponent(out _collider);
        TryGetComponent(out _pooledObject);

        if (!arrowVisual)
        {
            TryGetComponent(out arrowVisual);
        }
    }

    // 重置箭头从对象池取出后的运行状态。
    public void OnSpawned()
    {
        TryGetComponent(out _pooledObject);
        _collider.enabled = true;
        arrowVisual?.ResetForSpawn();
        _sourceDefenseSystem = null;
        _targetEnemy = null;
        _targetTransform = null;
        _moveDirection = Vector2.zero;
        _lifeTimer = 0f;
        _damage = defaultDamage;
        _armorIgnorePercent = 0f;
        _isExplosiveArrow = false;
        _explosionRadius = 0f;
        _explosionDamageMultiplier = 0f;
        _isReturning = false;
        _rb2.linearVelocity = Vector2.zero;
    }

    // 清理箭头回池前的运行状态。
    public void OnDespawned()
    {
        _sourceDefenseSystem = null;
        _targetEnemy = null;
        _targetTransform = null;
        _moveDirection = Vector2.zero;
        _isReturning = true;
        _collider.enabled = false;
        arrowVisual?.ResetForDespawn();
        _rb2.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        _lifeTimer += Time.deltaTime;

        if (_lifeTimer >= lifetime)
        {
            ReturnArrow();
        }
    }

    private void FixedUpdate()
    {
        FlyToEnemy();
    }

    // 设置箭头追踪目标。
    public void SetTarget(Enemy target)
    {
        if (!ArrowDamageApplier.IsEnemyValid(target))
        {
            ReturnArrow();
            return;
        }

        _targetEnemy = target;
        _targetTransform = target.transform;
        _moveDirection = (_targetTransform.position - transform.position).normalized;
    }

    // 设置本次发射的箭头伤害。
    public void SetDamage(int damage)
    {
        _damage = Mathf.Max(1, damage);
    }

    // 设置本次发射携带的攻击上下文。
    public void SetAttackContext(DefenseSystem sourceDefenseSystem, float armorIgnorePercent, bool isExplosiveArrow, float explosionRadius, float explosionDamageMultiplier)
    {
        _sourceDefenseSystem = sourceDefenseSystem;
        _armorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        _isExplosiveArrow = isExplosiveArrow;
        _explosionRadius = Mathf.Max(0f, explosionRadius);
        _explosionDamageMultiplier = Mathf.Max(0f, explosionDamageMultiplier);
    }

    // 设置本次发射使用的箭头视觉材质和拖尾状态。
    public void SetVisualEffect(Material visualMaterial, bool enableTrail)
    {
        arrowVisual?.ApplyVisualEffect(visualMaterial, enableTrail);
    }

    // 让箭头飞向当前目标。
    private void FlyToEnemy()
    {
        if (!ArrowDamageApplier.IsEnemyValid(_targetEnemy) || !_targetTransform)
        {
            ContinueFlyingForward();
            return;
        }

        _moveDirection = (_targetTransform.position - transform.position).normalized;
        MoveInCurrentDirection();
    }

    // 处理箭头命中敌人后的命中流程和回收。
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out Enemy enemy) || !ArrowDamageApplier.IsEnemyValid(enemy))
        {
            return;
        }

        ArrowHitContext context = new(
            enemy,
            transform.position,
            _damage,
            _armorIgnorePercent,
            _sourceDefenseSystem,
            _explosionRadius,
            _explosionDamageMultiplier,
            _effectHitResults);
        ArrowHitProcessor.ApplyHit(context, _isExplosiveArrow ? explosiveArrowEffect : null);
        ReturnArrow();
    }

    // 目标失效后保持当前方向继续飞行。
    private void ContinueFlyingForward()
    {
        _targetEnemy = null;
        _targetTransform = null;

        if (_moveDirection == Vector2.zero)
        {
            ReturnArrow();
            return;
        }

        MoveInCurrentDirection();
    }

    // 按当前飞行方向更新刚体速度和朝向。
    private void MoveInCurrentDirection()
    {
        if (_rb2)
        {
            _rb2.linearVelocity = _moveDirection * flySpeed;
        }

        transform.right = _moveDirection;
    }

    // 将箭头回收到对象池，减少对象销毁。
    private void ReturnArrow()
    {
        if (_isReturning)
        {
            return;
        }

        _isReturning = true;

        if (_pooledObject)
        {
            _pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}
