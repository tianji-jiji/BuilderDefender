using UnityEngine;

/// <summary>
/// 箭矢追踪飞行行为，负责追踪有效目标并在目标失效后沿原方向继续飞行。
/// </summary>
public class HomingArrowFlightBehavior : ArrowFlightBehavior
{
    private Rigidbody2D _rigidbody2D;
    private Enemy _targetEnemy;
    private Transform _targetTransform;
    private Vector2 _moveDirection;
    private float _flySpeed;

    public override ArrowFlightBehaviorType BehaviorType => ArrowFlightBehaviorType.Homing;

    private void Awake()
    {
        ResetState();
    }

    // 使用目标和速度初始化追踪飞行。
    public override bool InitializeFlight(Rigidbody2D rigidbody2D, Enemy targetEnemy, float flySpeed)
    {
        if (!rigidbody2D || !ArrowHitDamageApplier.IsEnemyValid(targetEnemy))
        {
            return false;
        }

        _rigidbody2D = rigidbody2D;
        _targetEnemy = targetEnemy;
        _targetTransform = targetEnemy.transform;
        _flySpeed = Mathf.Max(0f, flySpeed);
        _moveDirection = (_targetTransform.position - transform.position).normalized;
        return true;
    }

    // 追踪有效目标，目标失效后保持最后方向飞行。
    public override bool TickFlight()
    {
        if (ArrowHitDamageApplier.IsEnemyValid(_targetEnemy) && _targetTransform)
        {
            _moveDirection = (_targetTransform.position - transform.position).normalized;
            MoveInCurrentDirection();
            return true;
        }

        ClearTarget();
        if (_moveDirection == Vector2.zero)
        {
            return false;
        }

        MoveInCurrentDirection();
        return true;
    }

    // 命中后清除当前目标并保留最后移动方向。
    public override void ContinueAfterHit()
    {
        ClearTarget();
    }

    // 清理追踪目标、方向和刚体速度。
    public override void ResetState()
    {
        if (_rigidbody2D)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        _rigidbody2D = null;
        _targetEnemy = null;
        _targetTransform = null;
        _moveDirection = Vector2.zero;
        _flySpeed = 0f;
    }

    // 清除当前追踪目标。
    private void ClearTarget()
    {
        _targetEnemy = null;
        _targetTransform = null;
    }

    // 按当前方向设置刚体速度和箭矢朝向。
    private void MoveInCurrentDirection()
    {
        _rigidbody2D.linearVelocity = _moveDirection * _flySpeed;
        transform.right = _moveDirection;
    }
}
