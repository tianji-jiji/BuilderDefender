using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopupUI : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private float moveUpDistance = 8f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float fadeDuration = 0.4f; 
    [SerializeField] private float targetScale = 0.04f;

    private CanvasGroup _canvasGroup;
    private PooledObject _pooledObject;
    private Vector3 _startPos;
    private Tween _moveTween;
    private Tween _scaleTween;
    private Tween _fadeTween;
    private Tween _returnTween;
    private bool _wasSpawnedFromPool;

    // 初始化漂浮提示需要的透明度控制组件。
    private void Awake()
    {
        CacheReferences();
    }

    // 兼容非对象池方式生成的漂浮提示。
    private void Start()
    {
        if (TryGetComponent(out PooledObject pooledObject) && pooledObject.SourcePrefab)
        {
            _wasSpawnedFromPool = true;
            return;
        }

        if (_wasSpawnedFromPool)
        {
            return;
        }

        ResetVisualState();
        _startPos = transform.position;
        PlayAnimation();
    }

    // 重置漂浮提示从对象池取出后的状态。
    public void OnSpawned()
    {
        _wasSpawnedFromPool = true;
        CacheReferences();
        ResetVisualState();
        _startPos = transform.position;
        PlayAnimation();
    }

    // 清理漂浮提示回池前的动画状态。
    public void OnDespawned()
    {
        KillTweens();
        ResetVisualState();
        _wasSpawnedFromPool = false;
    }

    // 播放漂浮提示的上升、弹出和淡出动画。
    private void PlayAnimation()
    {
        KillTweens();

        // 缓动上升
        _moveTween = transform.DOMoveY(_startPos.y + moveUpDistance, duration)
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject);

        // 缩放弹出
        transform.localScale = Vector3.zero;
        _scaleTween = transform.DOScale(Vector3.one * targetScale, 0.3f)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
        
        // 淡出
        _fadeTween = _canvasGroup.DOFade(0, fadeDuration)
            .SetDelay(duration - fadeDuration)
            .SetLink(gameObject);

        // 自动销毁
        _returnTween = DOVirtual.DelayedCall(duration, () =>
        {
            ReturnPopup();
        }).SetLink(gameObject);
    }

    // 设置漂浮提示显示的文本。
    public void SetText(string amount)
    {
        if (amountText)
        {
            amountText.text = amount;
        }
    }

    // 缓存漂浮提示依赖的组件。
    private void CacheReferences()
    {
        if (!TryGetComponent(out _canvasGroup))
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        TryGetComponent(out _pooledObject);
    }

    // 重置漂浮提示的视觉状态。
    private void ResetVisualState()
    {
        if (_canvasGroup)
        {
            _canvasGroup.alpha = 1f;
        }

        transform.localScale = Vector3.zero;
    }

    // 停止当前漂浮提示的所有 DOTween 动画。
    private void KillTweens()
    {
        _moveTween?.Kill();
        _scaleTween?.Kill();
        _fadeTween?.Kill();
        _returnTween?.Kill();

        _moveTween = null;
        _scaleTween = null;
        _fadeTween = null;
        _returnTween = null;
    }

    // 将漂浮提示归还对象池，非池化对象则销毁。
    private void ReturnPopup()
    {
        if (_pooledObject)
        {
            _pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}
