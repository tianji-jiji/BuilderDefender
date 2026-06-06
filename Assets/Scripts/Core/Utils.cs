using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用工具类，负责提供跨系统复用的基础辅助方法。
/// </summary>
public static class Utils
{
    private static Camera MainCamera;

    // 获取当前鼠标位置对应的世界坐标。
    public static Vector3 GetMousePosition()
    {
        Camera mainCamera = GetMainCamera();
        if (!mainCamera)
        {
            return Vector3.zero;
        }

        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        return position;
    }

    // 获取当前场景有效的主相机，避免场景重载后继续使用上一局已销毁的相机引用。
    private static Camera GetMainCamera()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main;
        }

        return MainCamera;
    }
}
