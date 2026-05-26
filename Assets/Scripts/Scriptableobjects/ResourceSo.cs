using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptablebojects/ResourceSo")]
public class ResourceSo : ScriptableObject
{
    public string resourceName;
    public Sprite sprite;
    // 一次采集能够提供的资源数量，默认为1
    public int amountOnceHarvest = 1;
}
