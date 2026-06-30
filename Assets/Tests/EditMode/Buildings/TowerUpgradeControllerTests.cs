using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Tower 升级运行时组件测试，验证免费升级命令和星级状态。
/// </summary>
public class TowerUpgradeControllerTests
{
    // 验证免费升级可以提升一级并升到满级。
    [Test]
    public void FreeUpgradeAdvancesAndReachesMaxStar()
    {
        using TestGameObjectScope scope = new();
        ScriptableObject config = null;

        try
        {
            Type controllerType = GetRequiredType("TowerUpgradeController");
            config = CreateConfig();
            Component controller = scope.Create("TowerUpgrade").AddComponent(controllerType);
            SetField(controllerType, controller, "upgradeConfig", config);

            Assert.IsTrue(InvokeBool(controllerType, controller, "UpgradeOneLevelWithoutCost"));
            Assert.AreEqual(2, GetProperty<int>(controllerType, controller, "CurrentStarLevel"));
            Assert.IsTrue(InvokeBool(controllerType, controller, "UpgradeToMaxStarWithoutCost"));
            Assert.AreEqual(3, GetProperty<int>(controllerType, controller, "CurrentStarLevel"));
            Assert.IsTrue(GetProperty<bool>(controllerType, controller, "IsMaxStar"));
        }
        finally
        {
            if (config)
            {
                Object.DestroyImmediate(config);
            }
        }
    }

    // 创建包含二星和三星配置的测试资产。
    private static ScriptableObject CreateConfig()
    {
        Type configType = GetRequiredType("BuildingUpgradeConfigSo");
        Type levelType = GetRequiredType("BuildingUpgradeLevel");
        object levelTwo = Activator.CreateInstance(levelType);
        object levelThree = Activator.CreateInstance(levelType);
        SetField(levelType, levelTwo, "starLevel", 2);
        SetField(levelType, levelThree, "starLevel", 3);

        ScriptableObject config = ScriptableObject.CreateInstance(configType);
        FieldInfo levelListField = GetRequiredField(configType, "upgradeLevels");
        IList levelList = Activator.CreateInstance(levelListField.FieldType) as IList;
        Assert.NotNull(levelList);
        levelList.Add(levelTwo);
        levelList.Add(levelThree);
        levelListField.SetValue(config, levelList);
        return config;
    }

    // 调用无参数并返回布尔值的方法。
    private static bool InvokeBool(Type type, object target, string methodName)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);
        return (bool)method.Invoke(target, Array.Empty<object>());
    }

    // 读取指定对象的公开属性。
    private static T GetProperty<T>(Type type, object target, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(property);
        return (T)property.GetValue(target);
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        GetRequiredField(type, fieldName).SetValue(target, value);
    }

    // 获取指定类型的私有实例字段。
    private static FieldInfo GetRequiredField(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"未找到字段：{type.Name}.{fieldName}");
        return field;
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
