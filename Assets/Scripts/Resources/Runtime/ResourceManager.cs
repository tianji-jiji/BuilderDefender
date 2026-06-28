using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private int startingResourceValue = 100;
    public static ResourceManager Instance;
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
            _resourcesDic[resourceSo] = startingResourceValue;
        }
    }

   // amountGained：每次采集该资源能提供多少资源数量
    public void AddResource(ResourceSo resourceSo, int amountGained)
    {
        _resourcesDic[resourceSo] += amountGained;
        OnResourceAmountChanged?.Invoke();
    }

    // 获取指定资源当前数量。
    public int GetResourceAmount(ResourceSo resourceSo)
    {
        return _resourcesDic[resourceSo];
    }

    // 判断当前资源是否足够支付指定消耗。
    public bool CanAfford(IEnumerable<ResourceCost> resourceCosts)
    {
        foreach (var resourceCost in resourceCosts)
        {
            if (resourceCost == null || !resourceCost.resourceSo)
            {
                return false;
            }

            if (!_resourcesDic.TryGetValue(resourceCost.resourceSo, out int currentAmount))
            {
                return false;
            }

            if (currentAmount < resourceCost.amount)
            {
                return false;
            }
        }

        return true;
    }

    // 判断当前资源是否足够支付建筑经过奖励修正后的建造消耗。
    public bool CanAfford(BuildingSo buildingSo)
    {
        if (!buildingSo)
        {
            return false;
        }

        foreach (ResourceCost resourceCost in buildingSo.resourceCost)
        {
            if (resourceCost == null || !resourceCost.resourceSo)
            {
                return false;
            }

            if (!_resourcesDic.TryGetValue(resourceCost.resourceSo, out int currentAmount))
            {
                return false;
            }

            int adjustedAmount = GetAdjustedBuildCostAmount(buildingSo, resourceCost);
            if (currentAmount < adjustedAmount)
            {
                return false;
            }
        }

        return true;
    }

    // 扣除一组资源消耗。
    public void Spend(IEnumerable<ResourceCost> resourceCosts)
    {
        foreach (var resourceCost in resourceCosts)
        {
            if (resourceCost == null || !resourceCost.resourceSo)
            {
                continue;
            }

            _resourcesDic[resourceCost.resourceSo] -= resourceCost.amount;
        }

        OnResourceAmountChanged?.Invoke();
    }

    // 扣除建造建筑时需要的资源。
    public void Spend(BuildingSo buildingSo)
    {
        if (!buildingSo)
        {
            return;
        }

        foreach (ResourceCost resourceCost in buildingSo.resourceCost)
        {
            if (resourceCost == null || !resourceCost.resourceSo)
            {
                continue;
            }

            _resourcesDic[resourceCost.resourceSo] -= GetAdjustedBuildCostAmount(buildingSo, resourceCost);
        }

        OnResourceAmountChanged?.Invoke();
    }

    // 获取建筑经过奖励修正后的单项资源消耗。
    private int GetAdjustedBuildCostAmount(BuildingSo buildingSo, ResourceCost resourceCost)
    {
        return RewardRuntimeCoordinator.Instance
            ? RewardRuntimeCoordinator.Instance.DefenseTowerRewards.GetAdjustedBuildCostAmount(buildingSo, resourceCost)
            : resourceCost.amount;
    }
}
