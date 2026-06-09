using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 单种波次中普通、精英和 Boss 敌人的生成权重配置。
/// </summary>
[System.Serializable]
public class WaveEnemyWeights
{
    [SerializeField] private float normalWeight;
    [SerializeField] private float hardWeight;
    [SerializeField] private float bossWeight;

    public float NormalWeight => Mathf.Max(0f, normalWeight);
    public float HardWeight => Mathf.Max(0f, hardWeight);
    public float BossWeight => Mathf.Max(0f, bossWeight);

    // 创建一组敌人权重配置。
    public WaveEnemyWeights(float normalWeight, float hardWeight, float bossWeight)
    {
        this.normalWeight = normalWeight;
        this.hardWeight = hardWeight;
        this.bossWeight = bossWeight;
    }
}

/// <summary>
/// 全局波次规则配置，负责提供波次类型、刷怪批量、敌人权重和肉鸽成长曲线。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Wave Rule Config")]
public class WaveRuleSo : ScriptableObject
{
    private const float MIN_PLAYER_POWER_RESPONSE = 0f;
    private const float MAX_PLAYER_POWER_RESPONSE = 1f;

    [Header("Wave Type")]
    [SerializeField] private int bossInterval = 10;
    [SerializeField] private int hardInterval = 5;

    [Header("Batch")]
    [SerializeField] private int minBatchSize = 3;
    [SerializeField] private int maxBatchSize = 20;

    [Header("Enemy Weights")]
    [SerializeField] private WaveEnemyWeights normalWaveWeights = new(0.8f, 0.2f, 0f);
    [SerializeField] private WaveEnemyWeights hardWaveWeights = new(0.6f, 0.4f, 0f);
    [SerializeField] private WaveEnemyWeights bossWaveWeights = new(0.3f, 0.3f, 0.4f);

    [Header("Roguelike Growth")]
    [SerializeField] private AnimationCurve enemyCountByWave = new AnimationCurve(
        new Keyframe(1, 10),
        new Keyframe(10, 18),
        new Keyframe(20, 40),
        new Keyframe(30, 65),
        new Keyframe(40, 80));
    [SerializeField] private AnimationCurve spawnIntervalByWave = new AnimationCurve(
        new Keyframe(1, 1.0f),
        new Keyframe(10, 0.65f),
        new Keyframe(20, 0.45f),
        new Keyframe(30, 0.32f),
        new Keyframe(40, 0.25f));
    [SerializeField] private AnimationCurve healthMultiplierByWave = new AnimationCurve(
        new Keyframe(1, 1f),
        new Keyframe(10, 1.8f),
        new Keyframe(20, 3.6f),
        new Keyframe(30, 6.5f),
        new Keyframe(40, 9f));
    [SerializeField] private AnimationCurve armorBonusByWave = new AnimationCurve(
        new Keyframe(1, 0),
        new Keyframe(10, 3),
        new Keyframe(20, 12),
        new Keyframe(30, 28),
        new Keyframe(40, 40));
    [SerializeField] private AnimationCurve attackMultiplierByWave = new AnimationCurve(
        new Keyframe(1, 1f),
        new Keyframe(10, 1.2f),
        new Keyframe(20, 1.6f),
        new Keyframe(30, 2.2f),
        new Keyframe(40, 2.8f));
    [SerializeField] private AnimationCurve moveSpeedMultiplierByWave = new AnimationCurve(
        new Keyframe(1, 1f),
        new Keyframe(10, 1.05f),
        new Keyframe(20, 1.15f),
        new Keyframe(30, 1.25f),
        new Keyframe(40, 1.25f));
    
    [SerializeField] private float playerPowerResponseStrength = 0.35f;

    public int BossInterval => Mathf.Max(0, bossInterval);
    public int HardInterval => Mathf.Max(0, hardInterval);
    public int MinBatchSize => Mathf.Max(1, minBatchSize);
    public int MaxBatchSize => Mathf.Max(MinBatchSize, maxBatchSize);
    public WaveEnemyWeights NormalWaveWeights => normalWaveWeights;
    public WaveEnemyWeights HardWaveWeights => hardWaveWeights;
    public WaveEnemyWeights BossWaveWeights => bossWaveWeights;
    public AnimationCurve EnemyCountByWave => enemyCountByWave;
    public AnimationCurve SpawnIntervalByWave => spawnIntervalByWave;
    public AnimationCurve HealthMultiplierByWave => healthMultiplierByWave;
    public AnimationCurve ArmorBonusByWave => armorBonusByWave;
    public AnimationCurve AttackMultiplierByWave => attackMultiplierByWave;
    public AnimationCurve MoveSpeedMultiplierByWave => moveSpeedMultiplierByWave;
    public float PlayerPowerResponseStrength => Mathf.Clamp(playerPowerResponseStrength, MIN_PLAYER_POWER_RESPONSE, MAX_PLAYER_POWER_RESPONSE);

}
