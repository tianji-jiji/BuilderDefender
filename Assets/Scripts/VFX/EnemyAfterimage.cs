using UnityEngine;

public class EnemyAfterimage : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float lifetime = 0.28f;
    [SerializeField] private float startAlpha = 0.35f;
    [SerializeField] private float endAlpha;

    private PooledObject _pooledObject;
    private float _timer;
    private Color _baseColor = Color.white;
    private bool _isReturning;

    // 缓存残影回池需要的组件引用。
    private void Awake()
    {
        TryGetComponent(out _pooledObject);
    }

    // 初始化残影显示内容并从源 SpriteRenderer 复制渲染状态。
    public void Setup(SpriteRenderer sourceRenderer, Color tintColor)
    {
        if (!sourceRenderer || !spriteRenderer)
        {
            ReturnAfterimage();
            return;
        }

        Transform sourceTransform = sourceRenderer.transform;
        transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
        transform.localScale = sourceTransform.lossyScale;

        spriteRenderer.sprite = sourceRenderer.sprite;
        spriteRenderer.flipX = sourceRenderer.flipX;
        spriteRenderer.flipY = sourceRenderer.flipY;
        spriteRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        spriteRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;
        spriteRenderer.sharedMaterial = sourceRenderer.sharedMaterial;

        Color sourceColor = sourceRenderer.color;
        _baseColor = new Color(
            sourceColor.r * tintColor.r,
            sourceColor.g * tintColor.g,
            sourceColor.b * tintColor.b,
            startAlpha
        );

        spriteRenderer.color = _baseColor;
        spriteRenderer.enabled = true;
        _timer = 0f;
        _isReturning = false;
    }

    // 重置残影从对象池取出后的生命周期状态。
    public void OnSpawned()
    {
        _timer = 0f;
        _isReturning = false;

        if (spriteRenderer)
        {
            spriteRenderer.enabled = false;
        }
    }

    // 清理残影回池前的渲染状态，避免复用时残留上一帧外观。
    public void OnDespawned()
    {
        _timer = 0f;
        _isReturning = true;

        if (spriteRenderer)
        {
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = null;
            spriteRenderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, startAlpha);
        }
    }

    // 按生命周期淡出残影并在结束后回池。
    private void Update()
    {
        if (_isReturning || !spriteRenderer || !spriteRenderer.enabled)
        {
            return;
        }

        _timer += Time.deltaTime;
        float progress = lifetime > 0f ? Mathf.Clamp01(_timer / lifetime) : 1f;
        Color color = _baseColor;
        color.a = Mathf.Lerp(startAlpha, endAlpha, progress);
        spriteRenderer.color = color;

        if (progress >= 1f)
        {
            ReturnAfterimage();
        }
    }

    // 将残影归还对象池，不存在对象池时销毁对象。
    private void ReturnAfterimage()
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
}
