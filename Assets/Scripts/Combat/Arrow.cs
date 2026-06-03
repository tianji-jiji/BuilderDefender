using UnityEngine;

public class Arrow : MonoBehaviour, IPoolable
{
    [SerializeField] private float flySpeed;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TrailRenderer[] trailRenderers;
    [SerializeField] private Material defaultVisualMaterial;

    private Rigidbody2D _rb2;
    private Collider2D _collider;
    private PooledObject _pooledObject;

    private Enemy _targetEnemy;
    private Transform _targetTransform;

    // 飞行方向
    private Vector2 _moveDirection;
    private float _lifeTimer;
    private int _damage;
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
        _targetEnemy = null;
        _targetTransform = null;
        _moveDirection = Vector2.zero;
        _lifeTimer = 0f;
        _damage = defaultDamage;
        _isReturning = false;

        if (_rb2)
        {
            _rb2.linearVelocity = Vector2.zero;
        }
    }

    // 清理箭头回池前的运行状态。
    public void OnDespawned()
    {
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
            ReturnArrow();
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
        // 是敌人
        if (other.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            if (!IsTargetValid(enemy))
            {
                ReturnArrow();
                return;
            }

            // 有血条
            if (enemy.gameObject.TryGetComponent<HealthSystem>(out var healthSystem))
            {
                healthSystem.TakeDamage(_damage);
                ReturnArrow();
            }
        }
    }

    // 将箭头回收到对象池，缺少对象池时销毁对象。
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

    // 缓存箭头默认材质，一星箭头使用该材质保持无额外 shader 效果。
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

    // 将指定材质应用到所有箭头 SpriteRenderer。
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

    // 开关箭头拖尾渲染器，避免一星和回池状态残留拖尾。
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
