using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static Camera _mainCamera = Camera.main;
    public static Vector3 GetMousePosition()
    {
        if (!_mainCamera) return Vector3.zero;
        Vector3 pos =_mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }
}