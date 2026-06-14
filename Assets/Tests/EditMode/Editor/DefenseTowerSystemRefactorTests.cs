using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class DefenseTowerSystemRefactorTests
{
    // 验证防御塔属性计算器会保留基础属性。
    [Test]
    public void DefenseTowerStatCalculator_BuildsStatsFromBaseValues()
    {
        DefenseTowerStatCalculator calculator = new(null, 10, 0.3f, 15f);

        DefenseTowerCombatStats stats = calculator.RefreshStats(null);

        Assert.AreEqual(10, stats.AttackDamage);
        Assert.AreEqual(0.3f, stats.ArrowGenerateRate);
        Assert.AreEqual(15f, stats.DetectRadius);
    }

    // 验证防御塔目标分区枚举可被多个战斗子系统共享。
    [Test]
    public void DefenseTowerTargetLane_ExposesSharedTargetLanes()
    {
        Assert.AreEqual(0, (int)DefenseTowerTargetLane.Any);
        Assert.AreEqual(1, (int)DefenseTowerTargetLane.Upper);
        Assert.AreEqual(2, (int)DefenseTowerTargetLane.Lower);
    }

    // 验证升星不会再默认改变箭矢材质和拖尾，视觉效果由卡牌效果接管。
    [Test]
    public void DefenseTowerArrowLauncher_CreateArrowContext_DoesNotApplyStarVisualEffect()
    {
        DefenseTowerArrowLauncher launcher = new GameObject("DefenseTowerSystemRefactorTest_Launcher").AddComponent<DefenseTowerArrowLauncher>();
        DefenseTowerStatCalculator calculator = new(null, 10, 0.3f, 15f);
        BuildingUpgradeLevel upgradeLevel = new();

        SetPrivateField(upgradeLevel, "starLevel", 3);
        calculator.ApplyUpgradeLevel(upgradeLevel);

        DefenseTowerArrowContext context = InvokeCreateArrowContext(launcher, calculator);

        Assert.IsNull(context.VisualMaterial);
        Assert.IsFalse(context.EnableTrail);

        Object.DestroyImmediate(launcher.gameObject);
    }

    // 设置私有字段，模拟 Inspector 序列化数据。
    private void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    // 调用箭矢上下文创建方法。
    private DefenseTowerArrowContext InvokeCreateArrowContext(DefenseTowerArrowLauncher launcher, DefenseTowerStatCalculator calculator)
    {
        MethodInfo methodInfo = typeof(DefenseTowerArrowLauncher).GetMethod("CreateArrowContext", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(methodInfo, "Missing CreateArrowContext method.");
        return (DefenseTowerArrowContext)methodInfo.Invoke(launcher, new object[] { null, null, calculator });
    }
}
