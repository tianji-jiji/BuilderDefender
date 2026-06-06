using UnityEngine;

/// <summary>
/// 箭矢组件，负责飞行、命中结算、爆裂伤害和对象池回收。
/// </summary>
public class Arrow : MonoBehaviour, IPoolable
{
    private const int MAX_EXPLOSION_HIT_COUNT = 32;

    [SerializeField] private float flySpeed;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TrailRenderer[] trailRenderers;
    [SerializeField] private Material defaultVisualMaterial;

    private readonly Collider2D[] _explosionHitResults = new Collider2D[MAX_EXPLOSION_HIT_COUNT];
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

    // 初始化箭头需要缓存的组件引用。
    private void Awake()
    {
        TryGetComponent(out _rb2);
        TryGetComponent(out _collider);
        TryGetComponent(out _pooledObject);
        CacheDefaultVisualMaterial();
    }

    // 重置箭头从对象池取出后的运行状态。
    public void OnSpawned()
    {
        TryGetComponent(out _pooledObject);
        SetVisualsActive(true);
        ApplyVisualMaterial(defaultVisualMaterial);
        SetTrailsActive(false);
        ClearTrails();
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

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 清理箭头回池前的运行状态。
    public void OnDespawned()
    {
        _sourceDefenseSystem = null;
        _targetEnemy = null;
        _targetTransform = null;
        _moveDirection = Vector2.zero;
        _isReturning = true;
        SetVisualsActive(false);
        ApplyVisualMaterial(defaultVisualMaterial);
        SetTrailsActive(false);
        ClearTrails();

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 处理箭头生命周期超时。
    private void Update()
    {
        _lifeTimer += Time.deltaTime;

        if (_lifeTimer >= lifetime)
        {
            ReturnArrow();
        }
    }

    // 固定帧更新箭头飞行。
    private void FixedUpdate()
    {
        FlyToEnemy();
    }

    // 设置箭头追踪目标。
    public void SetTarget(Enemy target)
    {
        if (!IsTargetValid(target))
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
        ApplyVisualMaterial(visualMaterial ? visualMaterial : defaultVisualMaterial);
        SetTrailsActive(enableTrail);
        ClearTrails();
    }

    // 让箭头飞向当前目标。
    private void FlyToEnemy()
    {
        if (!IsTargetValid(_targetEnemy) || !_targetTransform)
        {
            ContinueFlyingForward();
            return;
        }

        _moveDirection = (_targetTransform.position - transform.position).normalized;

        if (_rb2)
        {
            _rb2.linearVelocity = _moveDirection * flySpeed;
        }

        transform.right = _moveDirection;
    }

    // 处理箭头命中敌人的伤害和回收。
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out Enemy enemy) || !IsTargetValid(enemy))
        {
            return;
        }

        ApplyDamageToEnemy(enemy, _damage);
        ApplyExplosionDamage(enemy);
        ReturnArrow();
    }

    // 对指定敌人应用伤害，并在击杀时通知来源防御塔。
    private void ApplyDamageToEnemy(Enemy enemy, int rawDamage)
    {
        if (!enemy || !enemy.gameObject.TryGetComponent(out HealthSystem healthSystem))
        {
            return;
        }

        bool wasAlive = enemy.IsAlive;
        int adjustedDamage = CalculateArmorAdjustedDamage(enemy, rawDamage);
        int actualDamage = healthSystem.TakeDamage(adjustedDamage);
        DamageFloatingTextService.ShowEnemyDamage(enemy.DamageFloatingTextPosition, actualDamage);

        if (wasAlive && !enemy.IsAlive && _sourceDefenseSystem)
        {
            _sourceDefenseSystem.NotifyEnemyKilled();
        }
    }

    // 根据敌人护甲和本次护甲穿透计算最终伤害。
    private int CalculateArmorAdjustedDamage(Enemy enemy, int rawDamage)
    {
        float effectiveArmor = enemy.Armor * (1f - _armorIgnorePercent);
        return Mathf.Max(1, Mathf.RoundToInt(rawDamage - effectiveArmor));
    }

    // 处理爆裂箭范围伤害。
    private void ApplyExplosionDamage(Enemy directHitEnemy)
    {
        if (!_isExplosiveArrow || _explosionRadius <= 0f || _explosionDamageMultiplier <= 0f)
        {
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _explosionRadius, _explosionHitResults);
        int explosionDamage = Mathf.Max(1, Mathf.RoundToInt(_damage * _explosionDamageMultiplier));

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitResult = _explosionHitResults[i];
            if (!hitResult || !hitResult.TryGetComponent(out Enemy enemy) || enemy == directHitEnemy || !IsTargetValid(enemy))
            {
                continue;
            }

            ApplyDamageToEnemy(enemy, explosionDamage);
        }
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

    // 判断箭头当前目标是否仍然可以被攻击。
    private bool IsTargetValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }

    // 设置箭头可视和碰撞状态。
    private void SetVisualsActive(bool active)
    {
        if (_collider)
        {
            _collider.enabled = active;
        }

        if (spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.enabled = active;
            }
        }
    }

    // 缓存箭头默认材质。
    private void CacheDefaultVisualMaterial()
    {
        if (defaultVisualMaterial || spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer && spriteRenderer.sharedMaterial)
            {
                defaultVisualMaterial = spriteRenderer.sharedMaterial;
                return;
            }
        }
    }

    // 将指定材质应用到所有箭头渲染器。
    private void ApplyVisualMaterial(Material visualMaterial)
    {
        if (!visualMaterial || spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.sharedMaterial = visualMaterial;
            }
        }
    }

    // 开关箭头拖尾渲染器。
    private void SetTrailsActive(bool active)
    {
        if (trailRenderers == null)
        {
            return;
        }

        foreach (TrailRenderer trailRenderer in trailRenderers)
        {
            if (!trailRenderer)
            {
                continue;
            }

            trailRenderer.emitting = active;
            trailRenderer.enabled = active;
        }
    }

    // 清理箭头可能存在的拖尾残留。
    private void ClearTrails()
    {
        if (trailRenderers == null)
        {
            return;
        }

        foreach (TrailRenderer trailRenderer in trailRenderers)
        {
            if (trailRenderer)
            {
                trailRenderer.Clear();
            }
        }
    }
}
