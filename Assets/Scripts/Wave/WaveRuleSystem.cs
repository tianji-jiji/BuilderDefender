using UnityEngine;

/// <summary>
/// 根据配置和当前波数生成刷怪计划。
/// </summary>
public class WaveRuleSystem
{
    private const int DEFAULT_WAVE_COUNT = 20;
    private const float DEFAULT_SPAWN_INTERVAL = 1f;

    private WaveRuleSo _waveRuleSo;
    
    // 波次类型
    public enum WaveType
    {
        NormalWave,
        HardWave,
        BossWave
    }
    
    // 敌人类型
    public enum EnemyKind
    {
        NormalEnemy,
        HardEnemy,
        BossEnemy
    }
    
    /// <summary>
    /// 一波敌人的完整生成计划。
    /// </summary>
    public class WavePlan
    {
        public int waveIndex;
        public WaveType waveType;
        public int enemyCount;
        public int minBatchSize;
        public int maxBatchSize;
        public float spawnInterval;
        public float normalWeight;
        public float hardWeight;
        public float bossWeight;
    }

    public void SetWaveRuleConfig(WaveRuleSo config)
    {
        _waveRuleSo = config;
    }

    // 建立一波的刷怪计划。
    public WavePlan BuildPlan(int waveIndex)
    {
        // 当前波次类型
        WaveType type = GetWaveType(waveIndex);
        WavePlan plan = new WavePlan
        {
            waveIndex = waveIndex,
            waveType = type,
            enemyCount = GetEnemyCount(waveIndex),
            minBatchSize = GetMinBatchSize(),
            maxBatchSize = GetMaxBatchSize(),
            spawnInterval = GetSpawnInterval(waveIndex)
        };

        // 应用权重
        ApplyEnemyWeights(plan);
        return plan;
    }

    // 这一只怪是什么类型，根据计划权重随机选择
    public EnemyKind PickEnemyKind(WavePlan plan, float randomValue)
    {
        float totalWeight = plan.normalWeight + plan.hardWeight + plan.bossWeight;
        if (totalWeight <= 0f)
        {
            return EnemyKind.NormalEnemy;
        }

        float roll = randomValue * totalWeight;
        if (roll < plan.normalWeight)
        {
            return EnemyKind.NormalEnemy;
        }

        roll -= plan.normalWeight;
        if (roll < plan.hardWeight)
        {
            return EnemyKind.HardEnemy;
        }

        return EnemyKind.BossEnemy;
    }

    // 根据波数判断当前波次类型。
    private WaveType GetWaveType(int waveIndex)
    {
        int bossInterval = _waveRuleSo ? _waveRuleSo.BossInterval : 10;
        int hardInterval = _waveRuleSo ? _waveRuleSo.HardInterval : 5;

        if (bossInterval > 0 && waveIndex % bossInterval == 0)
        {
            return WaveType.BossWave;
        }

        if (hardInterval > 0 && waveIndex % hardInterval == 0)
        {
            return WaveType.HardWave;
        }

        return WaveType.NormalWave;
    }

    // 计算当前波次敌人总数。
    private int GetEnemyCount(int wave)
    {
        if (!_waveRuleSo)
        {
            return DEFAULT_WAVE_COUNT;
        }

        return Mathf.Max(0, Mathf.RoundToInt(EvaluateCurve(_waveRuleSo.EnemyCountByWave, wave, DEFAULT_WAVE_COUNT)));
    }

    // 获取每批刷怪的最小数量。
    private int GetMinBatchSize()
    {
        return _waveRuleSo ? _waveRuleSo.MinBatchSize : 3;
    }

    // 获取每批刷怪的最大数量。
    private int GetMaxBatchSize()
    {
        return _waveRuleSo ? _waveRuleSo.MaxBatchSize : 20;
    }

    // 获取当前波次每批刷怪之间的间隔。
    private float GetSpawnInterval(int wave)
    {
        if (!_waveRuleSo)
        {
            return DEFAULT_SPAWN_INTERVAL;
        }

        return Mathf.Max(0.05f, EvaluateCurve(_waveRuleSo.SpawnIntervalByWave, wave, DEFAULT_SPAWN_INTERVAL));
    }

    // 按波次类型设置敌人生成权重。
    private void ApplyEnemyWeights(WavePlan plan)
    {
        switch (plan.waveType)
        {
            case WaveType.BossWave:
                SetWeights(plan, GetBossWaveWeights());
                break;

            case WaveType.HardWave:
                SetWeights(plan, GetHardWaveWeights());
                break;

            default:
                SetWeights(plan, GetNormalWaveWeights());
                break;
        }
    }

    // 写入敌人生成权重。
    private void SetWeights(WavePlan plan, WaveEnemyWeights weights)
    {
        plan.normalWeight = weights.NormalWeight;
        plan.hardWeight = weights.HardWeight;
        plan.bossWeight = weights.BossWeight;
    }

    // 获取普通波敌人权重。
    private WaveEnemyWeights GetNormalWaveWeights()
    {
        return _waveRuleSo ? _waveRuleSo.NormalWaveWeights : new WaveEnemyWeights(0.8f, 0.2f, 0f);
    }

    // 获取精英波敌人权重。
    private WaveEnemyWeights GetHardWaveWeights()
    {
        return _waveRuleSo ? _waveRuleSo.HardWaveWeights : new WaveEnemyWeights(0.6f, 0.4f, 0f);
    }

    // 获取 Boss 波敌人权重。
    private WaveEnemyWeights GetBossWaveWeights()
    {
        return _waveRuleSo ? _waveRuleSo.BossWaveWeights : new WaveEnemyWeights(0.3f, 0.3f, 0.4f);
    }

    // 安全读取曲线值，曲线为空时返回默认值。
    private float EvaluateCurve(AnimationCurve curve, float time, float fallback)
    {
        if (curve == null || curve.length == 0)
        {
            return fallback;
        }

        return curve.Evaluate(time);
    }
}
