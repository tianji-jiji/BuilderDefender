using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 管理敌人波次的状态流转、刷怪协程和敌人存活数量。
/// </summary>
public class EnemyWaveManager : MonoBehaviour
{
    public static EnemyWaveManager Instance;

    public enum WaveState
    {
        Preparing, //第一波之前的准备
        Spawning,
        Fighting,
        WaitingNextWave
    }

    public WaveState State { get; private set; }

    [HideInInspector] public float stateTimer;

    //当前正在进行的波次编号
    public int waveIndex;

    [SerializeField] public EnemySpawnSystem spawnSystem;
    [SerializeField] public EnemyPool enemyPool;
    [SerializeField] public float nextWaveTimer;
    [SerializeField] public float firstWaveTimer = 15f;
    [SerializeField] private WaveRuleSo waveRuleSo;
    [SerializeField] private Transform enemyContainer;
    [FormerlySerializedAs("baseTransform")] [SerializeField] private Transform homeTransform;
    public event Action OnWaveStarted;
    public event Action OnAliveEnemyCountChanged;

    private readonly DifficultySystem _difficulty = new();
    private readonly WaveRuleSystem _ruleSystem = new();
    [HideInInspector] public int aliveEnemyCount;
    public bool IsFirstWave => waveIndex <= 0;

    private void Awake()
    {
        Instance = this;
        _ruleSystem.SetConfig(waveRuleSo);
    }

    private void Update()
    {
        switch (State)
        {
            case WaveState.Preparing:
                UpdatePreparing();
                break;

            case WaveState.WaitingNextWave:
                UpdateWaitingNextWave();
                break;

            case WaveState.Fighting:
                UpdateFighting();
                break;

            case WaveState.Spawning:
                break;
        }
    }

    /// <summary>
    /// 第一波之前的准备状态
    /// </summary>
    public void Begin()
    {
        EnterPreparingState();
    }

    private void EnterPreparingState()
    {
        State = WaveState.Preparing;
        stateTimer = firstWaveTimer;
    }


    private void OnEnable()
    {
        Enemy.OnEnemyDead += HandleEnemyDead;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDead -= HandleEnemyDead;
    }

    private void HandleEnemyDead()
    {
        aliveEnemyCount--;
        OnAliveEnemyCountChanged?.Invoke();
    }


    private void UpdatePreparing()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// 进入等待下一波状态。
    /// </summary>
    private void EnterWaitingNextWaveState()
    {
        State = WaveState.WaitingNextWave;
        stateTimer = nextWaveTimer;
    }

    /// <summary>
    /// 倒计时结束后开启下一波。
    /// </summary>
    private void UpdateWaitingNextWave()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// 进入战斗状态，等待场上敌人被清空。
    /// </summary>
    private void EnterFightingState()
    {
        State = WaveState.Fighting;
    }

    /// <summary>
    /// 场上敌人清空后进入下一波等待。
    /// </summary>
    private void UpdateFighting()
    {
        if (aliveEnemyCount <= 0)
        {
            EnterWaitingNextWaveState();
        }
    }

    /// <summary>
    /// 开始下一波刷怪流程。
    /// </summary>
    private void StartNextWave()
    {
        EnterSpawningState();
    }

    /// <summary>
    /// 生成当前波次计划并启动刷怪协程。
    /// </summary>
    private void EnterSpawningState()
    {
        State = WaveState.Spawning;
        waveIndex++;

        OnWaveStarted?.Invoke();
        WaveRuleSystem.WavePlan plan = _ruleSystem.BuildPlan(waveIndex, _difficulty);
        StartCoroutine(SpawnWave(plan));
    }

    /// <summary>
    /// 按波次计划分批生成敌人。
    /// </summary>
    private IEnumerator SpawnWave(WaveRuleSystem.WavePlan plan)
    {
        Transform spawnPoint = spawnSystem.GetRandomPoint();

        int spawned = 0;

        while (spawned < plan.EnemyCount)
        {
            int batchSize = Random.Range(plan.MinBatchSize, plan.MaxBatchSize);
            batchSize = Mathf.Min(batchSize, plan.EnemyCount - spawned);

            SpawnBatch(plan, spawnPoint, batchSize);

            spawned += batchSize;

            spawnPoint = spawnSystem.GetRandomPoint();

            yield return new WaitForSeconds(plan.SpawnInterval);
        }

        EnterFightingState();
    }

    /// <summary>
    /// 在指定出生点生成一批敌人。
    /// </summary>
    private void SpawnBatch(WaveRuleSystem.WavePlan plan, Transform spawnPoint, int batchSize)
    {
        for (int i = 0; i < batchSize; i++)
        {
            EnemySo enemy = PickEnemy(plan);
            SpawnEnemy(enemy, spawnPoint);
        }
    }

    /// <summary>
    /// 实例化敌人并初始化敌人数据。
    /// </summary>
    private void SpawnEnemy( EnemySo data, Transform point)
    {
        aliveEnemyCount++;
        OnAliveEnemyCountChanged?.Invoke();

        Vector2 offset = Random.insideUnitCircle * 2.0f;
            
        GameObject enemy = Instantiate(
            data.prefab,
            (Vector2)point.position + offset,
            Quaternion.identity
        );
        enemy.GetComponent<Enemy>().Init(data);
    }

    /// <summary>
    /// 根据波次权重从敌人池中选择敌人类型。
    /// </summary>
    private EnemySo PickEnemy(WaveRuleSystem.WavePlan plan)
    {
        switch (_ruleSystem.PickEnemyKind(plan, Random.value))
        {
            case WaveRuleSystem.EnemyKind.Boss:
                return enemyPool.GetBoss();

            case WaveRuleSystem.EnemyKind.Hard:
                return enemyPool.GetHard();

            default:
                return enemyPool.GetNormal();
        }
    }
}