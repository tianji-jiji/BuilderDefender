using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Building/BuildingSo")]
public class BuildingSo : ScriptableObject
{
    // 建筑名
    public string assetName;
    // 建筑类型
    public enum BuildingType
    {
        Harvester,
        Defense,
        Home,
    }
    public BuildingType buildingType;
    
    public int maxHealth;
    // 建筑预制体
    public GameObject prefab;
    // 建造花费时间
    public float constructionTime = 2f;
    // 拆除返还系数
    public float demolishRefundMultiplier;
    // 建造耗费的资源
    public List<ResourceCost> resourceCost;
    // 采集一个材料所需时间
    public float harvestSpeed ;
    // 资源采集范围
    public float harvestRadius;
    // 资源采集最大数量 
    public int maxHarvestAmount;
    // 采集什么类型的资源
    public ResourceSo resourceSo;
    // 建筑体积
    public Vector2 size;
}
