using System;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Tower 奖励状态测试，验证状态聚合与外部输入计算边界。
/// </summary>
public class TowerRewardStateTests
{
    // 验证攻击力奖励按加法累计并转换为倍率。
    [Test]
    public void AddAttackDamageBonusAccumulatesMultiplier()
    {
        Type stateType = GetRequiredType("TowerRewardState");
        object state = Activator.CreateInstance(stateType);
        MethodInfo addBonusMethod = stateType.GetMethod("AddAttackDamageBonus");
        PropertyInfo multiplierProperty = stateType.GetProperty("AttackDamageMultiplier");

        Assert.NotNull(addBonusMethod);
        Assert.NotNull(multiplierProperty);
        addBonusMethod.Invoke(state, new object[] { 0.1f });
        addBonusMethod.Invoke(state, new object[] { 0.2f });

        Assert.AreEqual(1.3f, (float)multiplierProperty.GetValue(state), 0.0001f);
    }

    // 验证最终防线倍率只依赖外部传入的基地生命比例。
    [Test]
    public void GetFinalDefenseMultiplierUsesProvidedHomeHealth()
    {
        Type stateType = GetRequiredType("TowerRewardState");
        Type calculatorType = GetRequiredType("TowerPowerCalculator");
        object state = Activator.CreateInstance(stateType);
        MethodInfo addFinalDefenseMethod = stateType.GetMethod("AddFinalDefense");
        MethodInfo getMultiplierMethod = calculatorType.GetMethod("GetFinalDefenseMultiplier");

        Assert.NotNull(addFinalDefenseMethod);
        Assert.NotNull(getMultiplierMethod);
        addFinalDefenseMethod.Invoke(state, new object[] { 0.5f, 0.3f });

        Assert.AreEqual(1.5f, (float)getMultiplierMethod.Invoke(null, new object[] { state, 0.2f }));
        Assert.AreEqual(1f, (float)getMultiplierMethod.Invoke(null, new object[] { state, 0.8f }));
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
