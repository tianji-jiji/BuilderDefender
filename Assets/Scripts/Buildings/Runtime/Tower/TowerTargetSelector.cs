using UnityEngine;

/// <summary>
/// 防御塔索敌组件，负责检测范围内敌人并按目标区域选择最近目标。
/// </summary>
public class TowerTargetSelector : MonoBehaviour
{
    public const float DETECT_INTERVAL = 0.2f;
    private const int MAX_DETECTED_ENEMIES = 10;

    [SerializeField] private Transform detectPoint;
    [SerializeField] private LayerMask detectLayer;

    private readonly Collider2D[] _detectResults = new Collider2D[MAX_DETECTED_ENEMIES];
    private int _detectedEnemyCount;
    private Enemy _currentTargetEnemy;

    // 判断敌人是否仍然可以被防御塔攻击。
    public static bool IsTargetValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }
    
    // 判断敌人是否属于指定的上下半区目标范围。
    private bool IsEnemyInTargetLane(Enemy enemy, TowerTargetLane targetLane)
    {
        switch (targetLane)
        {
            case TowerTargetLane.Upper:
                return enemy.transform.position.y >= transform.position.y;
            case TowerTargetLane.Lower:
                return enemy.transform.position.y < transform.position.y;
            default:
                return true;
        }
    }
    
    // 侦测范围内是否有敌人
    public bool HasTargetInRadius(float radius)
    {
        Transform detectionPoint = detectPoint ? detectPoint : transform;
        float detectRadius = Mathf.Max(0.01f, radius);
        _detectedEnemyCount = Physics2D.OverlapCircleNonAlloc(detectionPoint.position, detectRadius, _detectResults, detectLayer);

        if (_detectedEnemyCount == 0)
        {
            ClearTarget();
            return false;
        }

        _currentTargetEnemy = FindClosestTarget(TowerTargetLane.Any);
        return IsTargetValid(_currentTargetEnemy);
    }

    // 优先查找指定区域目标，区域内没有敌人时回退到全局索敌。
    public Enemy FindPreferredTarget(TowerTargetLane targetLane)
    {
        Enemy target = FindClosestTarget(targetLane);
        if (IsTargetValid(target) || targetLane == TowerTargetLane.Any)
        {
            return target;
        }

        return FindClosestTarget(TowerTargetLane.Any);
    }
    
    // 查找指定区域内距离防御塔最近的有效敌人。
    private Enemy FindClosestTarget(TowerTargetLane targetLane)
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
    
    // 当前目标失效时从缓存中重锁，缓存无效时重新侦测。
    public bool RefreshCurrentTarget(float radius)
    {
        if (IsTargetValid(_currentTargetEnemy))
        {
            return true;
        }

        _currentTargetEnemy = FindClosestTarget(TowerTargetLane.Any);
        
        if (IsTargetValid(_currentTargetEnemy))
        {
            return true;
        }

        return HasTargetInRadius(radius);
    }
    
    // 清空当前目标和检测缓存数量。
    public void ClearTarget()
    {
        _currentTargetEnemy = null;
        _detectedEnemyCount = 0;
    }
}
