using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 测试 GameObject 生命周期范围，确保测试结束时销毁创建的对象。
/// </summary>
public sealed class TestGameObjectScope : IDisposable
{
    private readonly List<GameObject> _gameObjectList = new();

    // 创建默认禁用的测试对象，避免组件生命周期自动访问场景单例。
    public GameObject Create(string objectName)
    {
        GameObject gameObject = new(objectName);
        gameObject.SetActive(false);
        _gameObjectList.Add(gameObject);
        return gameObject;
    }

    // 销毁当前范围创建的全部测试对象。
    public void Dispose()
    {
        foreach (GameObject gameObject in _gameObjectList)
        {
            if (gameObject)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
