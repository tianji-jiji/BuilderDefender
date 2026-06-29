using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人持续状态控制器，负责管理中毒、燃烧的刷新、跳伤和状态标记显示。
/// </summary>
public class EnemyStatusEffectController : MonoBehaviour
{
    private const float MARKER_SIZE = 0.18f;
    private const float MARKER_Y_OFFSET = 0.65f;
    private const float MARKER_X_OFFSET = 0.12f;
    private const int MARKER_SORTING_ORDER = 20;

    private static Sprite StatusMarkerSprite;

    private readonly Dictionary<EnemyStatusEffectType, RuntimeStatus> _activeStatusDic = new();

    private Enemy _enemy;
    private SpriteRenderer _poisonMarkerRenderer;
    private SpriteRenderer _burnMarkerRenderer;

    public int ActiveStatusCount => _activeStatusDic.Count;

    private void Awake()
    {
        CacheReferences();
        CreateStatusMarkers();
        RefreshMarkers();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    private void Update()
    {
        TickStatuses(Time.deltaTime);
    }

    // 应用一个持续状态；同类状态已存在时刷新持续时间和参数。
    public void ApplyStatus(EnemyStatusEffectSpec spec)
    {
        if (spec.Duration <= 0f || spec.TickDamagePercent <= 0f)
        {
            return;
        }

        if (_activeStatusDic.TryGetValue(spec.EffectType, out RuntimeStatus status))
        {
            status.Refresh(spec);
            _activeStatusDic[spec.EffectType] = status;
            RefreshMarkers();
            return;
        }

        _activeStatusDic[spec.EffectType] = new RuntimeStatus(spec);
        RefreshMarkers();
    }

    // 查询指定状态剩余持续时间。
    public bool TryGetRemainingDuration(EnemyStatusEffectType effectType, out float remainingDuration)
    {
        if (_activeStatusDic.TryGetValue(effectType, out RuntimeStatus status))
        {
            remainingDuration = status.RemainingDuration;
            return true;
        }

        remainingDuration = 0f;
        return false;
    }

    // 清理所有持续状态和状态标记。
    public void ClearAll()
    {
        _activeStatusDic.Clear();
        RefreshMarkers();
    }

    // 缓存状态控制器依赖。
    private void CacheReferences()
    {
        if (!_enemy)
        {
            TryGetComponent(out _enemy);
        }
    }

    // 更新所有状态的持续时间和跳伤。
    private void TickStatuses(float deltaTime)
    {
        if (_activeStatusDic.Count <= 0)
        {
            return;
        }

        List<EnemyStatusEffectType> expiredEffectTypeList = new();
        List<EnemyStatusEffectType> effectTypeList = new(_activeStatusDic.Keys);

        foreach (EnemyStatusEffectType effectType in effectTypeList)
        {
            if (!_activeStatusDic.TryGetValue(effectType, out RuntimeStatus status))
            {
                continue;
            }

            status.Tick(deltaTime, GetEnemy());

            if (!_activeStatusDic.ContainsKey(effectType))
            {
                continue;
            }

            if (status.IsExpired)
            {
                expiredEffectTypeList.Add(effectType);
                continue;
            }

            _activeStatusDic[effectType] = status;
        }

        RemoveExpiredStatuses(expiredEffectTypeList);
    }

    // 移除已经到期的状态。
    private void RemoveExpiredStatuses(IReadOnlyList<EnemyStatusEffectType> expiredEffectTypeList)
    {
        if (expiredEffectTypeList.Count <= 0)
        {
            return;
        }

        foreach (EnemyStatusEffectType effectType in expiredEffectTypeList)
        {
            _activeStatusDic.Remove(effectType);
        }

        RefreshMarkers();
    }

    // 获取当前敌人组件引用。
    private Enemy GetEnemy()
    {
        if (!_enemy)
        {
            TryGetComponent(out _enemy);
        }

        return _enemy;
    }

    // 创建中毒和燃烧状态标记。
    private void CreateStatusMarkers()
    {
        _poisonMarkerRenderer = CreateMarker("PoisonStatusMarker", DamageFloatingTextEvent.GetStyleColor(DamageFloatingTextStyle.Poison), new Vector3(-MARKER_X_OFFSET, MARKER_Y_OFFSET, 0f));
        _burnMarkerRenderer = CreateMarker("BurnStatusMarker", DamageFloatingTextEvent.GetStyleColor(DamageFloatingTextStyle.Burn), new Vector3(MARKER_X_OFFSET, MARKER_Y_OFFSET, 0f));
    }

    // 创建单个状态标记渲染器。
    private SpriteRenderer CreateMarker(string markerName, Color color, Vector3 localPosition)
    {
        GameObject markerObject = new GameObject(markerName);
        markerObject.transform.SetParent(transform);
        markerObject.transform.localPosition = localPosition;
        markerObject.transform.localRotation = Quaternion.identity;
        markerObject.transform.localScale = Vector3.one * MARKER_SIZE;

        SpriteRenderer markerRenderer = markerObject.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = GetStatusMarkerSprite();
        markerRenderer.color = color;
        markerRenderer.sortingOrder = MARKER_SORTING_ORDER;
        markerObject.SetActive(false);
        return markerRenderer;
    }

    // 刷新状态标记显隐。
    private void RefreshMarkers()
    {
        if (_poisonMarkerRenderer)
        {
            _poisonMarkerRenderer.gameObject.SetActive(_activeStatusDic.ContainsKey(EnemyStatusEffectType.Poison));
        }

        if (_burnMarkerRenderer)
        {
            _burnMarkerRenderer.gameObject.SetActive(_activeStatusDic.ContainsKey(EnemyStatusEffectType.Burn));
        }
    }

    // 获取运行时生成的状态标记精灵。
    private static Sprite GetStatusMarkerSprite()
    {
        if (StatusMarkerSprite)
        {
            return StatusMarkerSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        StatusMarkerSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return StatusMarkerSprite;
    }

    private struct RuntimeStatus
    {
        public float RemainingDuration { get; private set; }
        private float _tickTimer;
        private EnemyStatusEffectSpec _spec;

        public bool IsExpired => RemainingDuration <= 0f;

        public RuntimeStatus(EnemyStatusEffectSpec spec)
        {
            _spec = spec;
            RemainingDuration = spec.Duration;
            _tickTimer = spec.TickInterval;
        }

        // 刷新状态参数和持续时间，同时保留下一次跳伤计时。
        public void Refresh(EnemyStatusEffectSpec spec)
        {
            _spec = spec;
            RemainingDuration = spec.Duration;
            _tickTimer = Mathf.Min(_tickTimer, spec.TickInterval);
        }

        // 推进状态计时并在间隔到达时造成跳伤。
        public void Tick(float deltaTime, Enemy enemy)
        {
            if (deltaTime <= 0f || IsExpired)
            {
                return;
            }

            float timeUntilNextTick = _tickTimer;
            RemainingDuration -= deltaTime;
            _tickTimer -= deltaTime;

            if (_tickTimer > 0f)
            {
                return;
            }

            if (timeUntilNextTick > RemainingDuration + deltaTime)
            {
                return;
            }

            _tickTimer += _spec.TickInterval;
            ArrowHitDamageApplier.ApplyMaxHealthPercentDamage(
                enemy,
                _spec.TickDamagePercent,
                _spec.FloatingTextStyle,
                _spec.SourceTowerCombatSystem);
        }
    }
}
