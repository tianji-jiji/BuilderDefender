using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 通用奖励效果应用服务测试，验证无效配置的诊断行为。
/// </summary>
public class RewardEffectApplicationServiceTests
{
    // 验证缺少效果定义时跳过配置并输出明确警告。
    [Test]
    public void ApplyEffectsMissingDefinitionLogsWarningAndSkips()
    {
        Type configType = GetRequiredType("RewardCardEffectConfig");
        Type contextType = GetRequiredType("RewardApplyContext");
        Type serviceType = GetRequiredType("RewardEffectApplicationService");
        Type configListType = typeof(System.Collections.Generic.List<>).MakeGenericType(configType);
        IList configList = Activator.CreateInstance(configListType) as IList;
        Assert.NotNull(configList);
        configList.Add(Activator.CreateInstance(configType));
        object context = Activator.CreateInstance(contextType, new object[] { null });
        MethodInfo applyEffectsMethod = serviceType.GetMethod(
            "ApplyEffects",
            BindingFlags.Static | BindingFlags.Public);
        Assert.NotNull(applyEffectsMethod);

        LogAssert.Expect(LogType.Warning, "奖励效果缺少 EffectDefinition，已跳过。");

        applyEffectsMethod.Invoke(null, new[] { configList, context });
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
