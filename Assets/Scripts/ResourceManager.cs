using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    // 所有资源
    private ResourceSoList _resourceSoList;
    
    public event Action OnResourceAmountChanged;
    
    // 存储资源及其数量
    private Dictionary<ResourceSo,int> _resourcesDic;
    
    private void Awake()
    {
        Instance = this;
        
        _resourcesDic = new Dictionary<ResourceSo,int>();
        _resourceSoList = Resources.Load<ResourceSoList>("ResourceSoList");
        InitializeDic();
    }

    /// <summary>
    /// 初始化资源字典
    /// </summary>
    private void InitializeDic()
    {
        foreach (var resourceSo in _resourceSoList.list)
        {
            _resourcesDic[resourceSo] = 100;
        }
    }

   // amount：每次采集该资源能提供多少资源数量
    public void AddResource(ResourceSo resourceSo, int amountGained)
    {
        _resourcesDic[resourceSo] += amountGained;
        OnResourceAmountChanged?.Invoke();
    }

    public int GetResourceAmount(ResourceSo resourceSo)
    {
        return _resourcesDic[resourceSo];
    }
    
    public void Spend(BuildingSo buildingSo)
    {
        foreach (var resourceCost in buildingSo.resourceCost)
        {
            _resourcesDic[resourceCost.resourceSo] -= resourceCost.amount;
        }
    }
}
