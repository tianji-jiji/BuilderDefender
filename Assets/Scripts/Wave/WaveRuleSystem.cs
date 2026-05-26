using UnityEngine;

/// <summary>
/// 根据配置和当前波数生成刷怪计划。
/// </summary>
public class WaveRuleSystem
{
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

    /// <summary>
    /// 设置波次规则配置资产。
    /// </summary>
    public void SetConfig(WaveRuleSo config)
    {
        _config = config;
    }

    /// <summary>
    /// 根据当前波数和难度系统生成刷怪计划。
    /// </summary>
    public WavePlan BuildPlan(int wave, DifficultySystem difficulty)
    {
        difficulty.WaveIndex = wave;

        var type = GetWaveType(wave);
        int enemyCount = GetEnemyCount(wave, difficulty);

        var plan = new WavePlan
        {
            WaveIndex = wave,
            Type = type,
            EnemyCount = enemyCount,
            MinBatchSize = GetMinBatchSize(),
            MaxBatchSize = GetMaxBatchSize(),
            SpawnInterval = GetSpawnInterval(wave, difficulty)
        };

        ApplyEnemyWeights(plan);
        return plan;
    }

    /// <summary>
    /// 根据计划权重随机选择本次要生成的敌人类型。
    /// </summary>
    public EnemyKind PickEnemyKind(WavePlan plan, float randomValue)
    {
        float totalWeight = plan.NormalWeight + plan.HardWeight + plan.BossWeight;
        if (totalWeight <= 0f)
            return EnemyKind.Normal;

        float roll = randomValue * totalWeight;

        if (roll < plan.NormalWeight)
            return EnemyKind.Normal;

        roll -= plan.NormalWeight;
        if (roll < plan.HardWeight)
            return EnemyKind.Hard;

        return EnemyKind.Boss;
    }

    /// <summary>
    /// 根据波数判断当前波次类型。
    /// </summary>
    private WaveType GetWaveType(int wave)
    {
        int bossInterval = _config ? _config.bossInterval : 10;
        int hardInterval = _config ? _config.hardInterval : 5;

        if (bossInterval > 0 && wave % bossInterval == 0)
            return WaveType.Boss;

        if (hardInterval > 0 && wave % hardInterval == 0)
            return WaveType.Hard;

        return WaveType.Normal;
    }

    /// <summary>
    /// 计算当前波次的敌人总数。
    /// </summary>
    private int GetEnemyCount(int wave, DifficultySystem difficulty)
    {
        if (!_config)
        {
            int defaultCount = 10 + wave * 3 + Mathf.FloorToInt(Time.timeSinceLevelLoad * 0.5f);
            return Mathf.RoundToInt(defaultCount * difficulty.SpawnCountMultiplier);
        }

        int baseCount = _config.baseEnemyCount
                        + wave * _config.enemyCountPerWave
                        + Mathf.FloorToInt(Time.timeSinceLevelLoad * _config.elapsedTimeCountMultiplier);
        float countMultiplier = EvaluateCurve(_config.spawnCountMultiplier, wave, 1f);

        return Mathf.Max(0, Mathf.RoundToInt(baseCount * countMultiplier));
    }

    /// <summary>
    /// 获取每批刷怪的最小数量。
    /// </summary>
    private int GetMinBatchSize()
    {
        if (!_config)
            return 3;

        return Mathf.Max(1, _config.minBatchSize);
    }

    /// <summary>
    /// 获取每批刷怪的最大数量。
    /// </summary>
    private int GetMaxBatchSize()
    {
        if (!_config)
            return 20;

        return Mathf.Max(GetMinBatchSize(), _config.maxBatchSize);
    }

    /// <summary>
    /// 获取当前波次每批刷怪之间的间隔。
    /// </summary>
    private float GetSpawnInterval(int wave, DifficultySystem difficulty)
    {
        if (!_config)
            return difficulty.SpawnInterval;

        return Mathf.Max(0f, EvaluateCurve(_config.spawnInterval, wave, difficulty.SpawnInterval));
    }

    /// <summary>
    /// 按波次类型设置敌人生成权重。
    /// </summary>
    private void ApplyEnemyWeights(WavePlan plan)
    {
        switch (plan.Type)
        {
            case WaveType.Boss:
                SetWeights(plan, GetBossWaveNormalWeight(), GetBossWaveHardWeight(), GetBossWaveBossWeight());
                break;

            case WaveType.Hard:
                SetWeights(plan, GetHardWaveNormalWeight(), GetHardWaveHardWeight(), GetHardWaveBossWeight());
                break;

            default:
                SetWeights(plan, GetNormalWaveNormalWeight(), GetNormalWaveHardWeight(), GetNormalWaveBossWeight());
                break;
        }
    }

    /// <summary>
    /// 写入敌人生成权重。
    /// </summary>
    private void SetWeights(WavePlan plan, float normalWeight, float hardWeight, float bossWeight)
    {
        plan.NormalWeight = Mathf.Max(0f, normalWeight);
        plan.HardWeight = Mathf.Max(0f, hardWeight);
        plan.BossWeight = Mathf.Max(0f, bossWeight);
    }

    private float GetNormalWaveNormalWeight()
    {
        return _config ? _config.normalWaveNormalWeight : 0.8f;
    }

    private float GetNormalWaveHardWeight()
    {
        return _config ? _config.normalWaveHardWeight : 0.2f;
    }

    private float GetNormalWaveBossWeight()
    {
        return _config ? _config.normalWaveBossWeight : 0f;
    }

    private float GetHardWaveNormalWeight()
    {
        return _config ? _config.hardWaveNormalWeight : 0.6f;
    }

    private float GetHardWaveHardWeight()
    {
        return _config ? _config.hardWaveHardWeight : 0.4f;
    }

    private float GetHardWaveBossWeight()
    {
        return _config ? _config.hardWaveBossWeight : 0f;
    }

    private float GetBossWaveNormalWeight()
    {
        return _config ? _config.bossWaveNormalWeight : 0.6f;
    }

    private float GetBossWaveHardWeight()
    {
        return _config ? _config.bossWaveHardWeight : 0.25f;
    }

    private float GetBossWaveBossWeight()
    {
        return _config ? _config.bossWaveBossWeight : 0.15f;
    }

    /// <summary>
    /// 安全读取曲线值，曲线为空时返回默认值。
    /// </summary>
    private float EvaluateCurve(AnimationCurve curve, float time, float fallback)
    {
        if (curve == null || curve.length == 0)
            return fallback;

        return curve.Evaluate(time);
    }
}
