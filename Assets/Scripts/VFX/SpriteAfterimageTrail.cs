using UnityEngine;

public class SpriteAfterimageTrail : MonoBehaviour, IPoolable
{
    [SerializeField] private SpriteRenderer sourceRenderer;
    [SerializeField] private EnemyAfterimage afterimagePrefab;
    [SerializeField] private float spawnInterval = 0.08f;
    [SerializeField] private float minMoveDistance = 0.03f;
    [SerializeField] private Color afterimageColor = Color.white;

    private Enemy _enemy;
    private Vector3 _lastSpawnPosition;
    private float _spawnTimer;

    // 缓存敌人引用，并记录残影生成的初始位置。
    private void Awake()
    {
        TryGetComponent(out _enemy);
        _lastSpawnPosition = transform.position;
    }

    // 重置敌人从对象池取出后的拖尾节奏。
    public void OnSpawned()
    {
        _spawnTimer = 0f;
        _lastSpawnPosition = transform.position;
    }

    // 清理敌人回池前的拖尾计时状态。
    public void OnDespawned()
    {
        _spawnTimer = 0f;
        _lastSpawnPosition = transform.position;
    }

    // 按间隔和移动距离生成残影。
    private void Update()
    {
        if (!CanSpawnAfterimage())
        {
            return;
        }

        _spawnTimer += Time.deltaTime;
        float sqrDistance = (transform.position - _lastSpawnPosition).sqrMagnitude;
        float minSqrDistance = minMoveDistance * minMoveDistance;

        if (_spawnTimer < spawnInterval || sqrDistance < minSqrDistance)
        {
            return;
        }

        SpawnAfterimage();
        _spawnTimer = 0f;
        _lastSpawnPosition = transform.position;
    }

    // 判断当前敌人是否允许生成残影。
    private bool CanSpawnAfterimage()
    {
        return sourceRenderer
               && sourceRenderer.enabled
               && sourceRenderer.sprite
               && afterimagePrefab
               && (!_enemy || _enemy.IsAlive);
    }

    // 通过对象池生成残影，不存在对象池时使用普通实例化作为兜底。
    private void SpawnAfterimage()
    {
        EnemyAfterimage afterimage = null;

        if (PoolManager.Instance)
        {
            afterimage = PoolManager.Instance.Spawn(
                afterimagePrefab,
                sourceRenderer.transform.position,
                sourceRenderer.transform.rotation
            );
        }
        else
        {
            afterimage = Instantiate(
                afterimagePrefab,
                sourceRenderer.transform.position,
                sourceRenderer.transform.rotation
            );
        }

        if (afterimage)
        {
            afterimage.Setup(sourceRenderer, afterimageColor);
        }
    }
}
