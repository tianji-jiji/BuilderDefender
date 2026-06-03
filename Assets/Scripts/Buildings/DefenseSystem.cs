using UnityEngine;
using UnityEngine.Serialization;

public class DefenseSystem : MonoBehaviour
{
    private const int MAX_DETECTED_ENEMIES = 10;
    private const float DETECT_INTERVAL = 0.2f;
    private const float MIN_ARROW_GENERATE_RATE = 0.05f;

    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform[] superArrowSpawnPoints;
    [SerializeField] private Transform detectPoint;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private float arrowGenerateRate = 0.3f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Arrow arrowPrefab;
    
    private readonly Collider2D[] _detectResults = new Collider2D[MAX_DETECTED_ENEMIES];
    private float _baseDetectRadius;
    private float _baseArrowGenerateRate;
    private float _timer;
    private int _baseAttackDamage;
    private int _detectedEnemyCount;
    private bool _hasEnemy; 
    private bool _superArrowUnlocked;
    private Enemy _currentTargetEnemy;

    private enum TargetLane
    {
        Any,
        Upper,
        Lower
    }

    // 缓存防御塔的基础战斗属性。
    private void Awake()
    {
        _baseDetectRadius = detectRadius;
        _baseArrowGenerateRate = arrowGenerateRate;
        _baseAttackDamage = attackDamage;
    }
    
    // 启动防御塔的周期性敌人侦测。
    private void Start()
    {
        InvokeRepeating(nameof(DetectEnemy),0f,DETECT_INTERVAL);
    }

    // 生成箭头，并将当前有效敌人交给箭头追踪。
    private void GenerateArrow()
    {
        if (!arrowPrefab)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        bool hasFired = FireFromSpawnPoint(arrowSpawnPoint, TargetLane.Any);

        if (_superArrowUnlocked)
        {
            hasFired |= FireSuperArrowSpawnPoint(0, TargetLane.Upper);
            hasFired |= FireSuperArrowSpawnPoint(1, TargetLane.Lower);
        }

        if (!hasFired)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
        }
    }

    // 根据升星配置提升攻击力、射速并解锁超级弓箭。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (upgradeLevel == null)
        {
            return;
        }

        attackDamage = Mathf.Max(1, Mathf.RoundToInt(_baseAttackDamage * upgradeLevel.AttackDamageMultiplier));
        arrowGenerateRate = Mathf.Max(MIN_ARROW_GENERATE_RATE, _baseArrowGenerateRate * upgradeLevel.AttackIntervalMultiplier);
        detectRadius = Mathf.Max(0.01f, _baseDetectRadius * upgradeLevel.DetectRadiusMultiplier);
        _superArrowUnlocked = upgradeLevel.UnlockSuperArrow;
    }

    // 根据攻击间隔持续向当前目标发射箭头。
    private void Update()
    {
        if(!_hasEnemy) return;
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            GenerateArrow();
            _timer = arrowGenerateRate;
        }
    }

    // 侦测范围内最近的有效敌人并设置为当前攻击目标。
    private void DetectEnemy()
    {
        _detectedEnemyCount = Physics2D.OverlapCircleNonAlloc(detectPoint.position, detectRadius, _detectResults, detectLayer);
        
        // 没有敌人
        if (_detectedEnemyCount == 0)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        // 设置最近的敌人
        _currentTargetEnemy = FindClosestTarget(TargetLane.Any);
        _hasEnemy = _currentTargetEnemy;
    }

    // 使用指定发射点向目标区域内的最近敌人射箭。
    private bool FireFromSpawnPoint(Transform spawnPoint, TargetLane targetLane)
    {
        if (!spawnPoint)
        {
            return false;
        }

        Enemy target = FindPreferredTarget(targetLane);
        if (!IsTargetValid(target))
        {
            return false;
        }

        Arrow arrow = PoolManager.Instance
            ? PoolManager.Instance.Spawn(arrowPrefab, spawnPoint.position, Quaternion.identity)
            : Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity);

        if (!arrow)
        {
            return false;
        }

        arrow.SetDamage(attackDamage);
        arrow.SetTarget(target);
        return true;
    }

    // 使用三星解锁的额外发射点进行分区射击。
    private bool FireSuperArrowSpawnPoint(int index, TargetLane targetLane)
    {
        if (superArrowSpawnPoints == null || index < 0 || index >= superArrowSpawnPoints.Length)
        {
            return false;
        }

        return FireFromSpawnPoint(superArrowSpawnPoints[index], targetLane);
    }

    // 优先查找指定区域目标，区域内没有敌人时回退到全局索敌。
    private Enemy FindPreferredTarget(TargetLane targetLane)
    {
        Enemy target = FindClosestTarget(targetLane);
        if (IsTargetValid(target) || targetLane == TargetLane.Any)
        {
            return target;
        }

        return FindClosestTarget(TargetLane.Any);
    }

    // 查找指定区域内距离防御塔最近的有效敌人。
    private Enemy FindClosestTarget(TargetLane targetLane)
    {
        Enemy closestTarget = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < _detectedEnemyCount; i++)
        {
            Collider2D result = _detectResults[i];
            if (!result || !result.TryGetComponent(out Enemy enemy) || !IsTargetValid(enemy) || !IsEnemyInTargetLane(enemy, targetLane))
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude(enemy.transform.position - transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = enemy;
            }
        }

        return closestTarget;
    }

    // 判断敌人是否属于指定的上下半区目标范围。
    private bool IsEnemyInTargetLane(Enemy enemy, TargetLane targetLane)
    {
        switch (targetLane)
        {
            case TargetLane.Upper:
                return enemy.transform.position.y >= transform.position.y;

            case TargetLane.Lower:
                return enemy.transform.position.y < transform.position.y;

            default:
                return true;
        }
    }

    // 禁用防御塔时停止周期侦测。
    private void OnDisable()
    {
        CancelInvoke();
        _hasEnemy = false;
        _currentTargetEnemy = null;
    }

    // 判断敌人是否仍然可被防御塔攻击。
    private bool IsTargetValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }
}
