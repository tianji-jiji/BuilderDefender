using NUnit.Framework;
using UnityEngine;

public class WaveGrowthSystemTests
{
    // 验证波次计划在关键波数上使用 30 波成长曲线。
    [Test]
    public void BuildPlan_UsesThirtyWaveCountAndIntervalCurve()
    {
        WaveRuleSo config = ScriptableObject.CreateInstance<WaveRuleSo>();
        WaveRuleSystem ruleSystem = new WaveRuleSystem();
        ruleSystem.SetConfig(config);

        WaveRuleSystem.WavePlan waveOnePlan = ruleSystem.BuildPlan(1, new DifficultySystem());
        WaveRuleSystem.WavePlan waveTenPlan = ruleSystem.BuildPlan(10, new DifficultySystem());
        WaveRuleSystem.WavePlan waveTwentyPlan = ruleSystem.BuildPlan(20, new DifficultySystem());
        WaveRuleSystem.WavePlan waveThirtyPlan = ruleSystem.BuildPlan(30, new DifficultySystem());

        Assert.AreEqual(10, waveOnePlan.enemyCount);
        Assert.AreEqual(18, waveTenPlan.enemyCount);
        Assert.AreEqual(40, waveTwentyPlan.enemyCount);
        Assert.AreEqual(65, waveThirtyPlan.enemyCount);
        Assert.Less(waveThirtyPlan.spawnInterval, waveOnePlan.spawnInterval);

        Object.DestroyImmediate(config);
    }

    // 验证敌人生命会响应玩家战力，但不会完全等比追赶玩家。
    [Test]
    public void BuildStats_PlayerPowerSoftlyIncreasesHealth()
    {
        WaveRuleSo config = ScriptableObject.CreateInstance<WaveRuleSo>();
        EnemySo enemySo = ScriptableObject.CreateInstance<EnemySo>();
        enemySo.maxHealth = 100;
        enemySo.armor = 5;
        enemySo.atk = 10;
        enemySo.moveSpeed = 4f;
        enemySo.detectRadius = 6f;

        EnemyGrowthSystem growthSystem = new EnemyGrowthSystem();
        growthSystem.SetConfig(config);

        EnemyRuntimeStats baseStats = growthSystem.BuildStats(enemySo, 1, WaveRuleSystem.EnemyKind.Normal, DefensePowerSnapshot.Default());
        DefensePowerSnapshot strongPlayerSnapshot = new DefensePowerSnapshot(4f, 1f, 0f, 1f, 1f, 1f);
        EnemyRuntimeStats scaledStats = growthSystem.BuildStats(enemySo, 1, WaveRuleSystem.EnemyKind.Normal, strongPlayerSnapshot);

        Assert.Greater(scaledStats.MaxHealth, baseStats.MaxHealth);
        Assert.Less(scaledStats.MaxHealth, enemySo.maxHealth * strongPlayerSnapshot.PowerMultiplier);

        Object.DestroyImmediate(enemySo);
        Object.DestroyImmediate(config);
    }

    // 验证高护甲不会把高基础伤害硬压成 1 点。
    [Test]
    public void CalculateDamage_HighArmorKeepsMeaningfulDamage()
    {
        int damageWithoutPiercing = ArmorDamageCalculator.CalculateDamage(100, 300, 0f);
        int damageWithPiercing = ArmorDamageCalculator.CalculateDamage(100, 300, 0.5f);

        Assert.Greater(damageWithoutPiercing, 1);
        Assert.Greater(damageWithPiercing, damageWithoutPiercing);
    }
}
