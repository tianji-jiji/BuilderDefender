using NUnit.Framework;

public class DefenseSystemRefactorTests
{
    // 验证防御塔属性计算器会保留基础属性。
    [Test]
    public void DefenseStatCalculator_BuildsStatsFromBaseValues()
    {
        DefenseStatCalculator calculator = new(null, 10, 0.3f, 15f);

        DefenseCombatStats stats = calculator.RefreshStats(null);

        Assert.AreEqual(10, stats.AttackDamage);
        Assert.AreEqual(0.3f, stats.ArrowGenerateRate);
        Assert.AreEqual(15f, stats.DetectRadius);
    }

    // 验证防御塔目标分区枚举可被多个战斗子系统共享。
    [Test]
    public void DefenseTargetLane_ExposesSharedTargetLanes()
    {
        Assert.AreEqual(0, (int)DefenseTargetLane.Any);
        Assert.AreEqual(1, (int)DefenseTargetLane.Upper);
        Assert.AreEqual(2, (int)DefenseTargetLane.Lower);
    }
}
