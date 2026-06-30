using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

/// <summary>
/// 奖励卡选择控制器 PlayMode 测试，验证暂停时间的恢复行为。
/// </summary>
public class RewardCardSelectionControllerTests
{
    // 验证选择界面暂停后可以恢复进入界面前的时间倍率。
    [UnityTest]
    public IEnumerator PauseAndResumeRestoresPreviousTimeScale()
    {
        float originalTimeScale = Time.timeScale;
        GameObject gameObject = new("RewardSelection");
        gameObject.SetActive(false);

        try
        {
            Type panelType = GetRequiredType("RewardCardSelectionPanel");
            Type controllerType = GetRequiredType("RewardCardSelectionController");
            gameObject.AddComponent(panelType);
            Component controller = gameObject.AddComponent(controllerType);

            Time.timeScale = 0.75f;
            Invoke(controllerType, controller, "PauseGameTime");
            Assert.AreEqual(0f, Time.timeScale);

            Invoke(controllerType, controller, "ResumeGameTime");
            Assert.AreEqual(0.75f, Time.timeScale);
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            Object.Destroy(gameObject);
        }

        yield return null;
    }

    // 调用指定组件的私有实例方法。
    private static void Invoke(Type type, object target, string methodName)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method, $"未找到方法：{type.Name}.{methodName}");
        method.Invoke(target, null);
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
