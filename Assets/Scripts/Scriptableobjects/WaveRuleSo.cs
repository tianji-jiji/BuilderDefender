using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 全局波次规则配置。
/// </summary>
[CreateAssetMenu(menuName = "Wave/Wave Rule Config")]
public class WaveRuleSo : ScriptableObject
{
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
}
