using UnityEngine;

/// <summary>
/// 根据波次、敌人类型和玩家战力计算敌人运行时成长属性
/// </summary>
public class EnemyGrowthSystem
{
    private const float DEFAULT_PLAYER_POWER_RESPONSE = 0.35f;
    private const float BOSS_PLAYER_POWER_RESPONSE_BONUS = 0.1f;
    private const float DEFAULT_MULTIPLIER = 1f;
    private const float BOSS_HEALTH_RESPONSE_MULTIPLIER = 1.1f;

    private WaveRuleSo _waveRuleSo;

    // 设置敌人成长读取的波次配置
    public void SetWaveRuleConfig(WaveRuleSo config)
    {
        _waveRuleSo = config;
    }

    // 计算指定敌人在当前波次出生时应使用的运行时属性。
    public EnemyRuntimeStats BuildStats(EnemySo enemySo, int waveIndex, WaveRuleSystem.EnemyKind enemyKind, DefensePowerSnapshot playerPowerSnapshot)
    {
        // 从 EnemySo 里拿基础属性
        EnemyRuntimeStats baseStats = EnemyRuntimeStats.FromEnemySo(enemySo);
        
        // 玩家强度对敌人的影响
        float playerResponseStrength = GetPlayerResponseStrength(enemyKind);
        float playerPowerMultiplier = Mathf.Lerp(DEFAULT_MULTIPLIER, playerPowerSnapshot.PowerMultiplier, playerResponseStrength);

        // 各种属性倍率
        float healthMultiplier = EvaluateCurve(GetHealthMultiplierCurve(), waveIndex, DEFAULT_MULTIPLIER) * playerPowerMultiplier;
        if (enemyKind == WaveRuleSystem.EnemyKind.BossEnemy)
        {
            healthMultiplier *= BOSS_HEALTH_RESPONSE_MULTIPLIER;
        }
        float attackMultiplier = EvaluateCurve(GetAttackMultiplierCurve(), waveIndex, DEFAULT_MULTIPLIER);
        float moveSpeedMultiplier = EvaluateCurve(GetMoveSpeedMultiplierCurve(), waveIndex, DEFAULT_MULTIPLIER);
        int armorBonus = Mathf.RoundToInt(EvaluateCurve(GetArmorBonusCurve(), waveIndex, 0f));

        // 返回一份运行时属性
        return new EnemyRuntimeStats(
            Mathf.RoundToInt(baseStats.MaxHealth * healthMultiplier),
            baseStats.Armor + armorBonus,
            Mathf.RoundToInt(baseStats.AttackDamage * attackMultiplier),
            baseStats.MoveSpeed * moveSpeedMultiplier,
            baseStats.DetectRadius);
    }

    // 获取当前敌人类型对玩家战力的响应强度。
    private float GetPlayerResponseStrength(WaveRuleSystem.EnemyKind enemyKind)
    {
        float responseStrength = _waveRuleSo ? _waveRuleSo.PlayerPowerResponseStrength : DEFAULT_PLAYER_POWER_RESPONSE;
        if (enemyKind == WaveRuleSystem.EnemyKind.BossEnemy)
        {
            responseStrength += BOSS_PLAYER_POWER_RESPONSE_BONUS;
        }

        return Mathf.Clamp01(responseStrength);
    }

    // 获取生命成长曲线。
    private AnimationCurve GetHealthMultiplierCurve()
    {
        return _waveRuleSo ? _waveRuleSo.HealthMultiplierByWave : null;
    }

    // 获取护甲成长曲线。
    private AnimationCurve GetArmorBonusCurve()
    {
        return _waveRuleSo ? _waveRuleSo.ArmorBonusByWave : null;
    }

    // 获取攻击成长曲线。
    private AnimationCurve GetAttackMultiplierCurve()
    {
        return _waveRuleSo ? _waveRuleSo.AttackMultiplierByWave : null;
    }

    // 获取移速成长曲线。
    private AnimationCurve GetMoveSpeedMultiplierCurve()
    {
        return _waveRuleSo ? _waveRuleSo.MoveSpeedMultiplierByWave : null;
    }

    // 安全读取曲线值，曲线缺失时使用默认值。
    private float EvaluateCurve(AnimationCurve curve, float time, float fallback)
    {
        if (curve == null || curve.length == 0)
        {
            return fallback;
        }

        return curve.Evaluate(time);
    }
}
