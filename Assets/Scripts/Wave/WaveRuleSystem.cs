using UnityEngine;

/// <summary>
/// 根据配置和当前波数生成刷怪计划。
/// </summary>
public class WaveRuleSystem
{
    private const int DEFAULT_WAVE_COUNT = 10;
    private const float DEFAULT_SPAWN_INTERVAL = 1f;

    private WaveRuleSo _config;

    /// <summary>
    /// 一波敌人的完整生成计划。
    /// </summary>
    public class WavePlan
    {
        public int WaveIndex;
        public WaveType Type;
        public int EnemyCount;
        public int MinBatchSize;
        public int MaxBatchSize;
        public float SpawnInterval;
        public float NormalWeight;
        public float HardWeight;
        public float BossWeight;
    }

    public enum WaveType
    {
        Normal,
        Hard,
        Boss
    }

    public enum EnemyKind
    {
        Normal,
        Hard,
        Boss
    }

    // 设置波次规则配置资产。
    public void SetConfig(WaveRuleSo config)
    {
        _config = config;
    }

    // 根据当前波数和难度系统生成刷怪计划。
    public WavePlan BuildPlan(int wave, DifficultySystem difficulty)
    {
        difficulty.WaveIndex = wave;

        WaveType type = GetWaveType(wave);
        WavePlan plan = new WavePlan
        {
            WaveIndex = wave,
            Type = type,
            EnemyCount = GetEnemyCount(wave, difficulty),
            MinBatchSize = GetMinBatchSize(),
            MaxBatchSize = GetMaxBatchSize(),
            SpawnInterval = GetSpawnInterval(wave, difficulty)
        };

        ApplyEnemyWeights(plan);
        return plan;
    }

    // 根据计划权重随机选择本次要生成的敌人类型。
    public EnemyKind PickEnemyKind(WavePlan plan, float randomValue)
    {
        float totalWeight = plan.NormalWeight + plan.HardWeight + plan.BossWeight;
        if (totalWeight <= 0f)
        {
            return EnemyKind.Normal;
        }

        float roll = randomValue * totalWeight;
        if (roll < plan.NormalWeight)
        {
            return EnemyKind.Normal;
        }

        roll -= plan.NormalWeight;
        if (roll < plan.HardWeight)
        {
            return EnemyKind.Hard;
        }

        return EnemyKind.Boss;
    }

    // 根据波数判断当前波次类型。
    private WaveType GetWaveType(int wave)
    {
        int bossInterval = _config ? _config.BossInterval : 10;
        int hardInterval = _config ? _config.HardInterval : 5;

        if (bossInterval > 0 && wave % bossInterval == 0)
        {
            return WaveType.Boss;
        }

        if (hardInterval > 0 && wave % hardInterval == 0)
        {
            return WaveType.Hard;
        }

        return WaveType.Normal;
    }

    // 计算当前波次敌人总数。
    private int GetEnemyCount(int wave, DifficultySystem difficulty)
    {
        if (!_config)
        {
            return Mathf.RoundToInt(DEFAULT_WAVE_COUNT * difficulty.SpawnCountMultiplier);
        }

        return Mathf.Max(0, Mathf.RoundToInt(EvaluateCurve(_config.EnemyCountByWave, wave, DEFAULT_WAVE_COUNT)));
    }

    // 获取每批刷怪的最小数量。
    private int GetMinBatchSize()
    {
        return _config ? _config.MinBatchSize : 3;
    }

    // 获取每批刷怪的最大数量。
    private int GetMaxBatchSize()
    {
        return _config ? _config.MaxBatchSize : 20;
    }

    // 获取当前波次每批刷怪之间的间隔。
    private float GetSpawnInterval(int wave, DifficultySystem difficulty)
    {
        if (!_config)
        {
            return difficulty.SpawnInterval;
        }

        return Mathf.Max(0.05f, EvaluateCurve(_config.SpawnIntervalByWave, wave, DEFAULT_SPAWN_INTERVAL));
    }

    // 按波次类型设置敌人生成权重。
    private void ApplyEnemyWeights(WavePlan plan)
    {
        switch (plan.Type)
        {
            case WaveType.Boss:
                SetWeights(plan, GetBossWaveWeights());
                break;

            case WaveType.Hard:
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
        plan.NormalWeight = weights.NormalWeight;
        plan.HardWeight = weights.HardWeight;
        plan.BossWeight = weights.BossWeight;
    }

    // 获取普通波敌人权重。
    private WaveEnemyWeights GetNormalWaveWeights()
    {
        return _config ? _config.NormalWaveWeights : new WaveEnemyWeights(0.8f, 0.2f, 0f);
    }

    // 获取精英波敌人权重。
    private WaveEnemyWeights GetHardWaveWeights()
    {
        return _config ? _config.HardWaveWeights : new WaveEnemyWeights(0.6f, 0.4f, 0f);
    }

    // 获取 Boss 波敌人权重。
    private WaveEnemyWeights GetBossWaveWeights()
    {
        return _config ? _config.BossWaveWeights : new WaveEnemyWeights(0.3f, 0.3f, 0.4f);
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
