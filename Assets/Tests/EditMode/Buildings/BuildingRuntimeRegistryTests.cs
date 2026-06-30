using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 建筑运行时注册表测试，验证 Tower 查询与默认场景状态。
/// </summary>
public class BuildingRuntimeRegistryTests
{
    // 验证附近查询排除来源 Tower 并识别半径内另一座 Tower。
    [Test]
    public void HasNearbyTowerExcludesSourceAndFindsNeighbor()
    {
        using TestGameObjectScope scope = new();
        Type towerType = GetRequiredType("TowerCombatSystem");
        Type registryType = GetRequiredType("BuildingRuntimeRegistry");
        Component sourceTower = scope.Create("Source").AddComponent(towerType);
        Component nearbyTower = scope.Create("Nearby").AddComponent(towerType);
        sourceTower.transform.position = Vector3.zero;
        nearbyTower.transform.position = Vector3.right * 2f;
        object registry = Activator.CreateInstance(registryType);
        MethodInfo registerTowerMethod = registryType.GetMethod("RegisterTower");
        MethodInfo unregisterTowerMethod = registryType.GetMethod("UnregisterTower");
        MethodInfo hasNearbyTowerMethod = registryType.GetMethod("HasNearbyTower");

        registerTowerMethod?.Invoke(registry, new object[] { sourceTower });
        registerTowerMethod?.Invoke(registry, new object[] { sourceTower });
        registerTowerMethod?.Invoke(registry, new object[] { nearbyTower });
        Assert.IsTrue((bool)hasNearbyTowerMethod?.Invoke(registry, new object[] { sourceTower, 3f }));

        unregisterTowerMethod?.Invoke(registry, new object[] { nearbyTower });
        Assert.IsFalse((bool)hasNearbyTowerMethod?.Invoke(registry, new object[] { sourceTower, 3f }));
    }

    // 验证升级组件可以按 Tower 查询。
    [Test]
    public void GetUpgradeControllerReturnsMatchingController()
    {
        using TestGameObjectScope scope = new();
        Type towerType = GetRequiredType("TowerCombatSystem");
        Type controllerType = GetRequiredType("TowerUpgradeController");
        Type registryType = GetRequiredType("BuildingRuntimeRegistry");
        GameObject towerObject = scope.Create("Tower");
        Component tower = towerObject.AddComponent(towerType);
        Component controller = towerObject.AddComponent(controllerType);
        SetField(controllerType, controller, "towerCombatSystem", tower);
        object registry = Activator.CreateInstance(registryType);

        registryType.GetMethod("RegisterUpgradeController")?.Invoke(registry, new object[] { controller });
        object result = registryType.GetMethod("GetUpgradeController")?.Invoke(registry, new object[] { tower });

        Assert.AreSame(controller, result);
    }

    // 验证没有基地生命组件时返回完整生命比例。
    [Test]
    public void HomeHealthNormalizedWithoutHomeReturnsOne()
    {
        Type registryType = GetRequiredType("BuildingRuntimeRegistry");
        object registry = Activator.CreateInstance(registryType);
        PropertyInfo homeHealthProperty = registryType.GetProperty("HomeHealthNormalized");

        Assert.NotNull(homeHealthProperty);
        Assert.AreEqual(1f, (float)homeHealthProperty.GetValue(registry));
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field.SetValue(target, value);
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }
}
