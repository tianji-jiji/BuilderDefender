using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static readonly Camera MainCamera = Camera.main;

    public static Vector3 GetMousePosition()
    {
        if (!MainCamera) return Vector3.zero;
        Vector3 pos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }
}