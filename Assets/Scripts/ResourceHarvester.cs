using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 资源采集器
/// </summary>
public class ResourceHarvester : MonoBehaviour
{
    // 关联的建筑
    [FormerlySerializedAs("_buildingSo")] [SerializeField]
    private BuildingSo buildingSo;

    [SerializeField] private LayerMask harvestLayerMask;
    [SerializeField] private Transform popupUITransform;
    private readonly Collider2D[] _results = new Collider2D[10];
    private float _timer;

    // 一次采集行为共获得了多少资源
    public event Action<int, Vector3, BuildingSo> OnResourceHarvestedOneTime;
    public static event Action<ResourceHarvester> OnHarvesterCreated;

    private void Awake()
    {
        OnHarvesterCreated?.Invoke(this);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer < buildingSo.harvestSpeed)
            return;

        _timer = 0;

        HarvestAllTargets();
    }

    /// <summary>
    /// 寻找并采集资源
    /// </summary>
    /// <returns></returns>
    private void HarvestAllTargets()
    {
        // 检测采集范围内是否有资源
        var resourceSize = Physics2D.OverlapCircleNonAlloc(transform.position, buildingSo.harvestRadius,
            _results, harvestLayerMask);

        // 侦测到的资源数量
        int harvestedCount = 0;

        for (var i = 0; i < resourceSize; i++)
        {
            var resource = _results[i];
            var node = resource.GetComponent<ResourceNode>();

            if (node.resourceSo != buildingSo.resourceSo) continue;

            Harvest(node, buildingSo.resourceSo.amountOnceHarvest);
            harvestedCount++;

            // 达到最大采集数量限制
            if (harvestedCount >= buildingSo.maxHarvestAmount)
                break;
        }

        // 没有采到任何资源
        if (harvestedCount <= 0)
            return;

        // 飘字UI
        int amount = harvestedCount * buildingSo.resourceSo.amountOnceHarvest;
        OnResourceHarvestedOneTime?.Invoke(amount, popupUITransform.position, buildingSo);
    }

    /// <summary>
    /// 采集单个资源
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amountGained"></param>
    private void Harvest(ResourceNode target, int amountGained)
    {
        ResourceManager.Instance.AddResource(target.resourceSo, amountGained);
    }
}