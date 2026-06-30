using System;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Tower 战力计算器测试，验证规则状态到战力快照的转换。
/// </summary>
public class TowerPowerCalculatorTests
{
    // 验证额外攻击、双倍伤害和爆裂箭倍率进入战力快照。
    [Test]
    public void CreateSnapshotIncludesRulePowerMultipliers()
    {
        Type stateType = GetRequiredType("TowerRewardState");
        Type calculatorType = GetRequiredType("TowerPowerCalculator");
        object state = Activator.CreateInstance(stateType);
        stateType.GetMethod("AddExtraAttackRule")?.Invoke(state, new object[] { 2, 1 });
        stateType.GetMethod("AddDoubleDamageChance")?.Invoke(state, new object[] { 0.25f });
        stateType.GetMethod("AddThreeStarExplosiveArrow")?.Invoke(state, new object[] { 2f, 0.5f });

        MethodInfo createSnapshotMethod = calculatorType.GetMethod("CreateSnapshot");
        Assert.NotNull(createSnapshotMethod);
        object snapshot = createSnapshotMethod.Invoke(null, new[] { state });
        Type snapshotType = snapshot.GetType();

        Assert.AreEqual(1.5f, GetFloatProperty(snapshotType, snapshot, "ExtraAttackMultiplier"), 0.0001f);
        Assert.AreEqual(1.25f, GetFloatProperty(snapshotType, snapshot, "CriticalMultiplier"), 0.0001f);
        Assert.AreEqual(1.1f, GetFloatProperty(snapshotType, snapshot, "ExplosiveMultiplier"), 0.0001f);
    }

    // 读取战力快照上的浮点属性。
    private static float GetFloatProperty(Type type, object target, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);
        Assert.NotNull(property, $"未找到属性：{type.Name}.{propertyName}");
        return (float)property.GetValue(target);
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
