using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private const int MIN_HEALTH_PER_SEGMENT = 1;
    private const float DEFAULT_FULL_BAR_WIDTH = 4f;

    private enum HealthBarVisibilityMode
    {
        Default,
        ForceVisible,
        ForceHidden
    }

    private static readonly List<HealthBar> HealthBarList = new();
    private static HealthBarVisibilityMode CurrentVisibilityMode = HealthBarVisibilityMode.Default;

    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private GameObject bar;
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private bool enableTicks = true;
    [SerializeField] private Transform tickRoot;
    [SerializeField] private SpriteRenderer tickPrefab;
    [SerializeField] private int healthPerSegment = 100;
    [SerializeField] private float fullBarWidth = DEFAULT_FULL_BAR_WIDTH;
    [SerializeField] private float tickWidth = 0.035f;
    [SerializeField] private float tickHeight = 0.75f;
    [SerializeField] private Color tickColor = Color.black;

    private readonly List<SpriteRenderer> _tickRendererList = new();
    private bool _isSubscribed;
    private bool _lastEnableTicks;
    private int _lastMaxHealth = -1;

    // 启用血条时订阅生命变化，并立即同步血量和刻度显示。
    private void OnEnable()
    {
        RegisterHealthBar(this);
        SubscribeHealthSystem();
        UpdateBarSize();
    }

    // 禁用血条时取消订阅，避免对象池复用后重复监听。
    private void OnDisable()
    {
        UnsubscribeHealthSystem();
        UnregisterHealthBar(this);
    }

    // 在运行时通过 Inspector 修改刻度开关后立即刷新血条刻度。
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _lastMaxHealth = -1;
        UpdateBarSize();
    }

    // 切换所有血条的强制显示或强制隐藏状态。
    public static void ToggleAllHealthBarsVisibility()
    {
        CurrentVisibilityMode = CurrentVisibilityMode == HealthBarVisibilityMode.ForceVisible
            ? HealthBarVisibilityMode.ForceHidden
            : HealthBarVisibilityMode.ForceVisible;

        RefreshAllHealthBarsVisibility();
    }

    // 注册当前启用中的血条。
    private static void RegisterHealthBar(HealthBar healthBar)
    {
        if (!healthBar || HealthBarList.Contains(healthBar))
        {
            return;
        }

        HealthBarList.Add(healthBar);
    }

    // 取消注册已经禁用或销毁的血条。
    private static void UnregisterHealthBar(HealthBar healthBar)
    {
        HealthBarList.Remove(healthBar);
    }

    // 刷新所有启用中血条的可见性。
    private static void RefreshAllHealthBarsVisibility()
    {
        for (int i = HealthBarList.Count - 1; i >= 0; i--)
        {
            HealthBar healthBar = HealthBarList[i];

            if (!healthBar)
            {
                HealthBarList.RemoveAt(i);
                continue;
            }

            healthBar.UpdateBarVisible();
        }
    }

    // 订阅生命系统的变化事件。
    private void SubscribeHealthSystem()
    {
        if (_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnHealthChanged += UpdateBarSize;
        _isSubscribed = true;
    }

    // 取消订阅生命系统的变化事件。
    private void UnsubscribeHealthSystem()
    {
        if (!_isSubscribed || !healthSystem)
        {
            return;
        }

        healthSystem.OnHealthChanged -= UpdateBarSize;
        _isSubscribed = false;
    }

    // 根据当前生命百分比调整血条宽度，并在最大生命变化时重建刻度。
    private void UpdateBarSize()
    {
        if (!healthSystem || !bar)
        {
            return;
        }

        RebuildTicksIfNeeded();
        bar.transform.localScale = new Vector3(healthSystem.CurrentHealthNormalized, 1f, 1f);
        UpdateBarVisible();
    }

    // 最大生命值变化时按配置的每段血量重新生成刻度线。
    private void RebuildTicksIfNeeded()
    {
        if (!healthSystem)
        {
            return;
        }

        bool shouldRebuildTicks = healthSystem.MaxHealth != _lastMaxHealth || enableTicks != _lastEnableTicks;
        if (!shouldRebuildTicks)
        {
            return;
        }

        _lastMaxHealth = healthSystem.MaxHealth;
        _lastEnableTicks = enableTicks;
        ClearDynamicTicks();

        if (!enableTicks)
        {
            return;
        }

        int segmentCount = Mathf.CeilToInt((float)healthSystem.MaxHealth / Mathf.Max(MIN_HEALTH_PER_SEGMENT, healthPerSegment));
        int tickCount = Mathf.Max(0, segmentCount - 1);

        for (int i = 1; i <= tickCount; i++)
        {
            float normalizedPosition = (float)i / segmentCount;
            CreateTick(normalizedPosition);
        }
    }

    // 创建一条指定归一化位置的血条刻度线。
    private void CreateTick(float normalizedPosition)
    {
        if (!tickRoot)
        {
            return;
        }

        SpriteRenderer tickRenderer = CreateTickRenderer();
        if (!tickRenderer)
        {
            return;
        }

        Transform tickTransform = tickRenderer.transform;
        tickTransform.SetParent(tickRoot, false);
        tickTransform.localPosition = new Vector3(normalizedPosition * fullBarWidth, 0f, 0f);
        tickTransform.localRotation = Quaternion.identity;
        tickTransform.localScale = new Vector3(tickWidth, tickHeight, 1f);
        tickRenderer.color = tickColor;
        tickRenderer.enabled = false;
        _tickRendererList.Add(tickRenderer);
    }

    // 创建刻度渲染器，优先复制配置好的模板，未配置时使用已有血条 Sprite 兜底。
    private SpriteRenderer CreateTickRenderer()
    {
        if (tickPrefab)
        {
            SpriteRenderer tickRenderer = Instantiate(tickPrefab, tickRoot);
            tickRenderer.gameObject.SetActive(true);
            return tickRenderer;
        }

        SpriteRenderer sourceRenderer = GetFallbackTickSourceRenderer();
        if (!sourceRenderer)
        {
            return null;
        }

        GameObject tickObject = new GameObject("HealthTick");
        SpriteRenderer fallbackRenderer = tickObject.AddComponent<SpriteRenderer>();
        fallbackRenderer.sprite = sourceRenderer.sprite;
        fallbackRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
        fallbackRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        fallbackRenderer.sortingOrder = sourceRenderer.sortingOrder + 2;
        return fallbackRenderer;
    }

    // 获取用于兜底创建刻度的 SpriteRenderer。
    private SpriteRenderer GetFallbackTickSourceRenderer()
    {
        if (renderers == null)
        {
            return null;
        }

        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            if (spriteRenderer && spriteRenderer.sprite)
            {
                return spriteRenderer;
            }
        }

        return null;
    }

    // 清理已经动态创建的刻度线。
    private void ClearDynamicTicks()
    {
        foreach (SpriteRenderer tickRenderer in _tickRendererList)
        {
            if (tickRenderer)
            {
                Destroy(tickRenderer.gameObject);
            }
        }

        _tickRendererList.Clear();
    }

    // 满血时隐藏血条渲染器，受伤时重新显示。
    private void UpdateBarVisible()
    {
        if (!healthSystem)
        {
            SetRenderersVisible(false);
            SetTickRenderersVisible(false);
            return;
        }

        bool visible = ShouldShowHealthBar();
        SetRenderersVisible(visible);
        SetTickRenderersVisible(visible);
    }

    // 根据全局显示模式和当前血量判断血条是否应该显示。
    private bool ShouldShowHealthBar()
    {
        return CurrentVisibilityMode switch
        {
            HealthBarVisibilityMode.ForceVisible => true,
            HealthBarVisibilityMode.ForceHidden => false,
            _ => healthSystem.CurrentHealth < healthSystem.MaxHealth
        };
    }

    // 设置血条相关渲染器的显示状态。
    private void SetRenderersVisible(bool visible)
    {
        if (renderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }

    // 设置动态刻度渲染器的显示状态。
    private void SetTickRenderersVisible(bool visible)
    {
        foreach (SpriteRenderer tickRenderer in _tickRendererList)
        {
            if (tickRenderer)
            {
                tickRenderer.enabled = visible;
            }
        }
    }
}
