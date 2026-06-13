using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 管理敌人波次的状态流转、刷怪协程和敌人存活数量。
/// </summary>
public class EnemyWaveManager : MonoBehaviour
{
    private const float SPAWN_OFFSET_RADIUS = 2f;

    public static EnemyWaveManager Instance;

    public enum WaveState
    {
        Preparing,
        Spawning,
        Fighting,
        WaitingNextWave
    }

    // 核心变量
    public WaveState State { get; private set; }
    
    [HideInInspector] public int waveIndex;
    [HideInInspector] public float stateTimer;
    [HideInInspector] public int aliveEnemyCount;
    
    [SerializeField] public float nextWaveTimer;
    [SerializeField] public float firstWaveTimer = 15f;
    
    [SerializeField] public EnemySpawnSystem spawnSystem;
    [SerializeField] private EnemyDatabase enemyDatabase;
    [SerializeField] private WaveRuleSo waveRuleSo;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform homeTransform;

    public event Action OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action OnAliveEnemyCountChanged;
    public event Action<IReadOnlyList<Enemy>> OnEnemyBatchSpawned;
    
    // 波次规则系统和敌人成长系统
    private readonly WaveRuleSystem _ruleSystem = new();
    private readonly EnemyGrowthSystem _growthSystem = new();
    
    public bool IsFirstWave => waveIndex <= 0;

    // 初始化单例和波次规则系统。
    private void Awake()
    {
        Instance = this;
        _ruleSystem.SetWaveRuleConfig(waveRuleSo);
        _growthSystem.SetWaveRuleConfig(waveRuleSo);
    }

    // 订阅敌人死亡事件来维护存活数量。
    private void OnEnable()
    {
        Enemy.OnEnemyDead += HandleEnemyDead;
    }

    // 取消订阅敌人死亡事件。
    private void OnDisable()
    {
        Enemy.OnEnemyDead -= HandleEnemyDead;
    }
    
    // 根据当前波次状态更新计时和状态流转。
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
        }
    }

    // 开始第一波前的准备阶段。
    public void Begin()
    {
        EnterPreparingState();
    }

    // 进入第一波前的准备状态。
    private void EnterPreparingState()
    {
        State = WaveState.Preparing;
        stateTimer = firstWaveTimer;
    }

    // 敌人死亡时减少存活数量并通知 UI。
    private void HandleEnemyDead()
    {
        aliveEnemyCount--;
        OnAliveEnemyCountChanged?.Invoke();
    }

    // 更新第一波前的准备倒计时。
    private void UpdatePreparing()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartNextWave();
        }
    }

    // 进入等待下一波状态。
    private void EnterWaitingNextWaveState()
    {
        State = WaveState.WaitingNextWave;
        stateTimer = nextWaveTimer;
    }

    // 更新下一波倒计时。
    private void UpdateWaitingNextWave()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartNextWave();
        }
    }

    // 进入战斗状态，等待场上敌人清空。
    private void EnterFightingState()
    {
        State = WaveState.Fighting;
    }

    // 检查当前波敌人是否已经清空。
    private void UpdateFighting()
    {
        if (aliveEnemyCount <= 0)
        {
            OnWaveCompleted?.Invoke(waveIndex);
            EnterWaitingNextWaveState();
        }
    }

    // 启动下一波刷怪流程。
    private void StartNextWave()
    {
        EnterSpawningState();
    }

    // 生成当前波次计划并启动刷怪协程。
    private void EnterSpawningState()
    {
        State = WaveState.Spawning;
        waveIndex++;

        OnWaveStarted?.Invoke();
        WaveRuleSystem.WavePlan plan = _ruleSystem.BuildPlan(waveIndex);
        StartCoroutine(SpawnWave(plan));
    }

    // 按波次计划分批生成敌人。
    private IEnumerator SpawnWave(WaveRuleSystem.WavePlan plan)
    {
        Transform spawnPoint = spawnSystem.GetRandomPoint();
        int spawned = 0;

        while (spawned < plan.enemyCount)
        {
            int batchSize = Random.Range(plan.minBatchSize, plan.maxBatchSize + 1);
            batchSize = Mathf.Min(batchSize, plan.enemyCount - spawned);

            SpawnBatch(plan, spawnPoint, batchSize);
            spawned += batchSize;
            spawnPoint = spawnSystem.GetRandomPoint();

            yield return new WaitForSeconds(plan.spawnInterval);
        }

        EnterFightingState();
    }

    // 在指定出生点生成一批敌人并通知指示器系统。
    private void SpawnBatch(WaveRuleSystem.WavePlan plan, Transform spawnPoint, int batchSize)
    {
        List<Enemy> spawnedEnemies = new(batchSize);

        for (int i = 0; i < batchSize; i++)
        {
            WaveRuleSystem.EnemyKind enemyKind = _ruleSystem.PickEnemyKind(plan, Random.value);
            EnemySo enemy = PickEnemy(enemyKind);
            Enemy spawnedEnemy = SpawnEnemy(enemy, enemyKind, spawnPoint);

            if (spawnedEnemy)
            {
                spawnedEnemies.Add(spawnedEnemy);
            }
        }

        if (spawnedEnemies.Count > 0)
        {
            OnEnemyBatchSpawned?.Invoke(spawnedEnemies);
        }
    }

    // 从对象池生成敌人并初始化敌人数据。
    private Enemy SpawnEnemy(EnemySo data, WaveRuleSystem.EnemyKind enemyKind, Transform point)
    {
        if (!data || !point)
        {
            return null;
        }

        Vector2 offset = Random.insideUnitCircle * SPAWN_OFFSET_RADIUS;
        Vector3 spawnPosition = (Vector2)point.position + offset;
        GameObject enemyObject = PoolManager.Instance
            ? PoolManager.Instance.Spawn(data.prefab, spawnPosition, Quaternion.identity, enemyContainer)
            : Instantiate(data.prefab, spawnPosition, Quaternion.identity, enemyContainer);

        if (!enemyObject || !enemyObject.TryGetComponent(out Enemy enemy))
        {
            return null;
        }

        EnemyRuntimeStats runtimeStats = _growthSystem.BuildStats(data, waveIndex, enemyKind, GetPlayerPowerSnapshot());
        enemy.Init(data, runtimeStats);
        aliveEnemyCount++;
        OnAliveEnemyCountChanged?.Invoke();
        return enemy;
    }

    // 根据敌人类型从敌人池中取得配置资产。
    private EnemySo PickEnemy(WaveRuleSystem.EnemyKind enemyKind)
    {
        switch (enemyKind)
        {
            case WaveRuleSystem.EnemyKind.BossEnemy:
                return enemyDatabase.GetBoss();

            case WaveRuleSystem.EnemyKind.HardEnemy:
                return enemyDatabase.GetHard();

            default:
                return enemyDatabase.GetNormal();
        }
    }

    // 获取当前玩家防御体系战力快照。
    private DefensePowerSnapshot GetPlayerPowerSnapshot()
    {
        return RewardBonusManager.Instance
            ? RewardBonusManager.Instance.GetDefensePowerSnapshot()
            : DefensePowerSnapshot.Default();
    }
}
