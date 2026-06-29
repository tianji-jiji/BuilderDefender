using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭矢飞行控制器，负责缓存、选择并驱动当前发射使用的飞行行为。
/// </summary>
public class ArrowFlightController : MonoBehaviour
{
    private readonly Dictionary<ArrowFlightBehaviorType, ArrowFlightBehavior> _flightBehaviorDic = new();

    private ArrowFlightBehavior _activeFlightBehavior;

    private void Awake()
    {
        CacheFlightBehaviors();
    }

    // 选择并初始化指定类型的飞行行为。
    public bool TryStart(
        ArrowFlightBehaviorType flightBehaviorType,
        Rigidbody2D rigidbody2D,
        Enemy targetEnemy,
        float flySpeed)
    {
        ResetState();
        if (!_flightBehaviorDic.TryGetValue(flightBehaviorType, out ArrowFlightBehavior flightBehavior))
        {
            Debug.LogError($"箭矢缺少飞行行为组件：{flightBehaviorType}", this);
            return false;
        }

        if (!flightBehavior.InitializeFlight(rigidbody2D, targetEnemy, flySpeed))
        {
            return false;
        }

        _activeFlightBehavior = flightBehavior;
        return true;
    }

    // 推进当前飞行行为。
    public bool TickFlight()
    {
        return _activeFlightBehavior && _activeFlightBehavior.TickFlight();
    }

    // 通知当前飞行行为继续处理命中后的飞行。
    public void ContinueAfterHit()
    {
        _activeFlightBehavior?.ContinueAfterHit();
    }

    // 清理全部飞行行为和当前选择。
    public void ResetState()
    {
        foreach (ArrowFlightBehavior flightBehavior in _flightBehaviorDic.Values)
        {
            flightBehavior.ResetState();
        }

        _activeFlightBehavior = null;
    }

    // 缓存同一箭矢对象上的飞行行为组件。
    private void CacheFlightBehaviors()
    {
        _flightBehaviorDic.Clear();
        ArrowFlightBehavior[] flightBehaviorArray = GetComponents<ArrowFlightBehavior>();
        foreach (ArrowFlightBehavior flightBehavior in flightBehaviorArray)
        {
            if (flightBehavior)
            {
                _flightBehaviorDic[flightBehavior.BehaviorType] = flightBehavior;
            }
        }
    }
}
