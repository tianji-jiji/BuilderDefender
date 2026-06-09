using System.Collections.Generic;
using UnityEngine;

public class DirectionIndicatorManager : MonoBehaviour
{
    [SerializeField] private RectTransform indicatorRoot;
    [SerializeField] private RectTransform enemyIndicatorPrefab;
    [SerializeField] private RectTransform homeIndicatorPrefab;
    [SerializeField] private Transform homeTransform;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private bool showWhenTargetOnScreen;
    [SerializeField] private float edgePadding = 80f;
    [SerializeField] private float rotationOffset = -90f;

    private readonly List<EnemyBatchIndicator> _enemyBatchIndicators = new();
    private readonly List<EnemyBatchIndicator> _removedBatchIndicators = new();
    private RectTransform _homeIndicator;
    private bool _isSubscribed;
    private bool _hasStarted;

    private class EnemyBatchIndicator
    {
        public readonly List<Enemy> enemies;
        public readonly RectTransform indicator;

        // 保存一批敌人的引用和这批敌人共用的箭头。
        public EnemyBatchIndicator(IReadOnlyList<Enemy> enemies, RectTransform indicator)
        {
            this.enemies = new List<Enemy>(enemies);
            this.indicator = indicator;
        }
    }

    // 初始化摄像机引用。
    private void Awake()
    {
        if (!worldCamera)
        {
            worldCamera = Camera.main;
        }
    }

    // 对象重新启用时恢复订阅。
    private void OnEnable()
    {
        if (_hasStarted)
        {
            SubscribeWaveManager();
        }
    }

    // 等待场景对象初始化完成后订阅波次事件并创建基地箭头。
    private void Start()
    {
        _hasStarted = true;
        SubscribeWaveManager();
        CreateHomeIndicator();
    }

    // 取消订阅波次事件。
    private void OnDisable()
    {
        UnsubscribeWaveManager();
    }

    // 每帧刷新敌人批次箭头和基地箭头。
    private void LateUpdate()
    {
        if (!indicatorRoot || !worldCamera)
        {
            return;
        }

        UpdateEnemyBatchIndicators();
        UpdateHomeIndicator();
    }

    // 订阅每批敌人生成事件。
    private void SubscribeWaveManager()
    {
        if (_isSubscribed || EnemyWaveManager.Instance == null)
        {
            return;
        }

        EnemyWaveManager.Instance.OnEnemyBatchSpawned += AddEnemyBatchIndicator;
        _isSubscribed = true;
    }

    // 取消订阅每批敌人生成事件。
    private void UnsubscribeWaveManager()
    {
        if (!_isSubscribed || EnemyWaveManager.Instance == null)
        {
            return;
        }

        EnemyWaveManager.Instance.OnEnemyBatchSpawned -= AddEnemyBatchIndicator;
        _isSubscribed = false;
    }

    // 为一批新生成的敌人创建一个共享箭头。
    private void AddEnemyBatchIndicator(IReadOnlyList<Enemy> enemies)
    {
        if (enemies == null || enemies.Count <= 0 || !indicatorRoot || !enemyIndicatorPrefab)
        {
            return;
        }

        RectTransform indicator = Instantiate(enemyIndicatorPrefab, indicatorRoot);
        indicator.gameObject.SetActive(true);
        _enemyBatchIndicators.Add(new EnemyBatchIndicator(enemies, indicator));
    }

    // 创建指向基地的箭头。
    private void CreateHomeIndicator()
    {
        if (!homeTransform || !indicatorRoot || !homeIndicatorPrefab || _homeIndicator)
        {
            return;
        }

        _homeIndicator = Instantiate(homeIndicatorPrefab, indicatorRoot);
        _homeIndicator.gameObject.SetActive(false);
    }

    // 更新所有敌人批次箭头。
    private void UpdateEnemyBatchIndicators()
    {
        _removedBatchIndicators.Clear();

        foreach (EnemyBatchIndicator batchIndicator in _enemyBatchIndicators)
        {
            UpdateEnemyBatchIndicator(batchIndicator);
        }

        foreach (EnemyBatchIndicator batchIndicator in _removedBatchIndicators)
        {
            RemoveEnemyBatchIndicator(batchIndicator);
        }
    }

    // 更新单个敌人批次箭头。
    private void UpdateEnemyBatchIndicator(EnemyBatchIndicator batchIndicator)
    {
        RemoveInvalidEnemies(batchIndicator.enemies);

        if (batchIndicator.enemies.Count <= 0)
        {
            _removedBatchIndicators.Add(batchIndicator);
            return;
        }

        if (IsAnyEnemyOnScreen(batchIndicator.enemies) && !showWhenTargetOnScreen)
        {
            batchIndicator.indicator.gameObject.SetActive(false);
            return;
        }

        Vector3 targetPosition = GetEnemyBatchCenter(batchIndicator.enemies);
        UpdateOffScreenIndicator(targetPosition, batchIndicator.indicator);
    }

    // 移除一批敌人中已经失效的敌人引用。
    private void RemoveInvalidEnemies(List<Enemy> enemies)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (!IsEnemyTrackable(enemies[i]))
            {
                enemies.RemoveAt(i);
            }
        }
    }

    // 判断敌人是否仍然适合被箭头追踪。
    private bool IsEnemyTrackable(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }

    // 移除已经没有存活敌人的批次箭头。
    private void RemoveEnemyBatchIndicator(EnemyBatchIndicator batchIndicator)
    {
        _enemyBatchIndicators.Remove(batchIndicator);

        if (batchIndicator.indicator)
        {
            Destroy(batchIndicator.indicator.gameObject);
        }
    }

    // 更新基地箭头，基地在屏幕内时隐藏。
    private void UpdateHomeIndicator()
    {
        if (!_homeIndicator || !homeTransform)
        {
            return;
        }

        if (IsWorldPositionOnScreen(homeTransform.position) && !showWhenTargetOnScreen)
        {
            _homeIndicator.gameObject.SetActive(false);
            return;
        }

        UpdateOffScreenIndicator(homeTransform.position, _homeIndicator);
    }

    // 判断一批敌人中是否已有敌人进入摄像机视野。
    private bool IsAnyEnemyOnScreen(List<Enemy> enemies)
    {
        foreach (Enemy enemy in enemies)
        {
            if (IsWorldPositionOnScreen(enemy.transform.position))
            {
                return true;
            }
        }

        return false;
    }

    // 计算一批存活敌人的中心点。
    private Vector3 GetEnemyBatchCenter(List<Enemy> enemies)
    {
        Vector3 positionSum = Vector3.zero;

        foreach (Enemy enemy in enemies)
        {
            positionSum += enemy.transform.position;
        }

        return positionSum / enemies.Count;
    }

    // 判断世界坐标是否在摄像机视野范围内。
    private bool IsWorldPositionOnScreen(Vector3 worldPosition)
    {
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);

        return screenPosition is { z: > 0f, x: >= 0f } &&
               screenPosition.x <= Screen.width &&
               screenPosition.y >= 0f &&
               screenPosition.y <= Screen.height;
    }

    // 根据目标世界坐标更新屏幕边缘箭头的位置和朝向。
    private void UpdateOffScreenIndicator(Vector3 targetWorldPosition, RectTransform indicator)
    {
        if (!indicator)
        {
            return;
        }

        indicator.gameObject.SetActive(true);

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(targetWorldPosition);
        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 targetScreenPosition = screenPosition;

        if (screenPosition.z <= 0f)
        {
            targetScreenPosition = screenCenter - (targetScreenPosition - screenCenter);
        }

        Vector2 direction = (targetScreenPosition - screenCenter).normalized;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = Vector2.up;
        }

        Vector2 clampedScreenPosition = new(
            Mathf.Clamp(targetScreenPosition.x, edgePadding, Screen.width - edgePadding),
            Mathf.Clamp(targetScreenPosition.y, edgePadding, Screen.height - edgePadding)
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorRoot,
            clampedScreenPosition,
            null,
            out Vector2 localPosition
        );

        indicator.anchoredPosition = localPosition;
        indicator.localEulerAngles = new Vector3(
            0f,
            0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset
        );
    }
}
