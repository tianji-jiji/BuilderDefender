using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Tower 奖励场景适配器测试，验证注册表命令转换。
/// </summary>
public class TowerRewardWorldTests
{
    // 验证指定 Tower 可以通过对应升级组件免费提升一级。
    [Test]
    public void UpgradeTowerOneLevelUsesMatchingController()
    {
        using TestGameObjectScope scope = new();
        ScriptableObject config = null;

        try
        {
            Type towerType = GetRequiredType("TowerCombatSystem");
            Type controllerType = GetRequiredType("TowerUpgradeController");
            Type registryType = GetRequiredType("BuildingRuntimeRegistry");
            Type worldType = GetRequiredType("TowerRewardWorld");
            GameObject towerObject = scope.Create("Tower");
            Component tower = towerObject.AddComponent(towerType);
            Component controller = towerObject.AddComponent(controllerType);
            config = CreateConfig();
            SetField(controllerType, controller, "upgradeConfig", config);
            SetField(controllerType, controller, "towerCombatSystem", tower);
            object registry = Activator.CreateInstance(registryType);
            registryType.GetMethod("RegisterUpgradeController")?.Invoke(registry, new object[] { controller });
            object world = Activator.CreateInstance(worldType, new[] { registry, null });

            bool upgraded = (bool)worldType.GetMethod("UpgradeTowerOneLevel")
                ?.Invoke(world, new object[] { tower });

            Assert.IsTrue(upgraded);
            Assert.AreEqual(2, GetProperty<int>(controllerType, controller, "CurrentStarLevel"));
        }
        finally
        {
            if (config)
            {
                Object.DestroyImmediate(config);
            }
        }
    }

    // 验证没有基地生命组件时返回完整生命比例。
    [Test]
    public void HomeHealthNormalizedWithoutHomeReturnsOne()
    {
        Type registryType = GetRequiredType("BuildingRuntimeRegistry");
        Type worldType = GetRequiredType("TowerRewardWorld");
        object registry = Activator.CreateInstance(registryType);
        object world = Activator.CreateInstance(worldType, new[] { registry, null });

        Assert.AreEqual(1f, GetProperty<float>(worldType, world, "HomeHealthNormalized"));
    }

    // 创建包含二星配置的测试升级资产。
    private static ScriptableObject CreateConfig()
    {
        Type configType = GetRequiredType("BuildingUpgradeConfigSo");
        Type levelType = GetRequiredType("BuildingUpgradeLevel");
        object level = Activator.CreateInstance(levelType);
        SetField(levelType, level, "starLevel", 2);
        ScriptableObject config = ScriptableObject.CreateInstance(configType);
        FieldInfo levelListField = GetRequiredField(configType, "upgradeLevels");
        IList levelList = Activator.CreateInstance(levelListField.FieldType) as IList;
        Assert.NotNull(levelList);
        levelList.Add(level);
        levelListField.SetValue(config, levelList);
        return config;
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
        Assert.NotNull(field);
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
