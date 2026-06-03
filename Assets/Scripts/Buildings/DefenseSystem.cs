using UnityEngine;
using UnityEngine.Serialization;

public class DefenseSystem : MonoBehaviour
{
    private const int MAX_DETECTED_ENEMIES = 10;
    private const float DETECT_INTERVAL = 0.2f;

    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform detectPoint;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private float arrowGenerateRate = 0.3f;
    [SerializeField] private Arrow arrowPrefab;
    
    private readonly Collider2D[] _detectResults = new Collider2D[MAX_DETECTED_ENEMIES];
    private float _timer;
    private bool _hasEnemy; 
    private Enemy _currentTargetEnemy;
    
    // 启动防御塔的周期性敌人侦测。
    private void Start()
    {
        InvokeRepeating(nameof(DetectEnemy),0f,DETECT_INTERVAL);
    }

    // 生成箭头，并将当前有效敌人交给箭头追踪。
    private void GenerateArrow()
    {
        if (!IsTargetValid(_currentTargetEnemy) || !arrowPrefab)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        Arrow arrow = PoolManager.Instance
            ? PoolManager.Instance.Spawn(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity)
            : Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

        if (arrow)
        {
            arrow.SetTarget(_currentTargetEnemy);
        }
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
        var size = Physics2D.OverlapCircleNonAlloc(detectPoint.position, detectRadius, _detectResults, detectLayer);
        
        // 没有敌人
        if (size == 0)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        // 设置最近的敌人
        Enemy closestTarget = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < size; i++)
        {
            Collider2D result = _detectResults[i];
            if (!result || !result.TryGetComponent(out Enemy enemy) || !IsTargetValid(enemy))
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

        _currentTargetEnemy = closestTarget;
        _hasEnemy = _currentTargetEnemy;
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
