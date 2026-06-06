using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 全局波次规则配置。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Wave Rule Config")]
public class WaveRuleSo : ScriptableObject
{
    private const float MIN_PLAYER_POWER_RESPONSE = 0f;
    private const float MAX_PLAYER_POWER_RESPONSE = 1f;

    // 每隔多少波出现一次 Boss 波
    public int bossInterval = 10;
    // 每隔多少波出现一次精英波
    public int hardInterval = 5;

    // 每波基础敌人数量
    public int baseEnemyCount = 10;
    // 每过一波额外增加多少敌人
    public int enemyCountPerWave = 3;
    // 根据本局已经经过的时间额外增加敌人数量
    public float elapsedTimeCountMultiplier = 0.5f;

    // 每一批怪的数量范围
    public int minBatchSize = 3;
    public int maxBatchSize = 20;

    // 普通波敌人权重
    [FormerlySerializedAs("normalWeight")]
    public float normalWaveNormalWeight = 0.8f;
    [FormerlySerializedAs("hardWeightInNormalWave")]
    public float normalWaveHardWeight = 0.2f;
    public float normalWaveBossWeight = 0f;

    // 精英波敌人权重
    public float hardWaveNormalWeight = 0.6f;
    public float hardWaveHardWeight = 0.4f;
    public float hardWaveBossWeight = 0f;

    // Boss 波敌人权重
    public float bossWaveNormalWeight = 0.6f;
    public float bossWaveHardWeight = 0.25f;
    public float bossWaveBossWeight = 0.15f;

    // 敌人数量倍率曲线
    public AnimationCurve spawnCountMultiplier = AnimationCurve.Linear(1, 1f, 30, 3f);
    // 每批刷怪间隔曲线
    public AnimationCurve spawnInterval = AnimationCurve.Linear(1, 0.6f, 30, 0.05f);

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

    public AnimationCurve EnemyCountByWave => enemyCountByWave;
    public AnimationCurve SpawnIntervalByWave => spawnIntervalByWave;
    public AnimationCurve HealthMultiplierByWave => healthMultiplierByWave;
    public AnimationCurve ArmorBonusByWave => armorBonusByWave;
    public AnimationCurve AttackMultiplierByWave => attackMultiplierByWave;
    public AnimationCurve MoveSpeedMultiplierByWave => moveSpeedMultiplierByWave;
    public float PlayerPowerResponseStrength => Mathf.Clamp(playerPowerResponseStrength, MIN_PLAYER_POWER_RESPONSE, MAX_PLAYER_POWER_RESPONSE);
}
