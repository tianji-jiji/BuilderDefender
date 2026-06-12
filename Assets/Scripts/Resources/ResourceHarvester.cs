using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 资源采集器
/// </summary>
public class ResourceHarvester : MonoBehaviour
{
    private const int RESULTS_MAX_COUNT = 10;

    // 关联的建筑
    [FormerlySerializedAs("_buildingSo")] [SerializeField]
    private BuildingSo buildingSo;

    [SerializeField] private LayerMask harvestLayerMask;
    [SerializeField] private Transform popupUITransform;
    private readonly Collider2D[] _results = new Collider2D[RESULTS_MAX_COUNT];
    private ContactFilter2D _harvestContactFilter;
    private readonly Dictionary<Collider2D, ResourceNode> _resourceNodeByColliderDic = new();
    private readonly List<ParticleSystem> _harvestParticleSystemList = new();
    private Animator _harvesterAnimator;
    private float _timer;
    private bool _isHarvestAnimationPlaying = true;

    // 一次采集行为共获得了多少资源
    public event Action<int, Vector3, BuildingSo> OnResourceHarvestedOneTime;

    // 新资源采集器被创建时触发
    public static event Action<ResourceHarvester> OnHarvesterCreated;

    // 初始化采集器状态并通知外部系统。
    private void Awake()
    {
        _harvestContactFilter = new ContactFilter2D();
        _harvestContactFilter.SetLayerMask(harvestLayerMask);
        _harvestContactFilter.useTriggers = true;

        CacheHarvestVisualComponents();
        SetHarvestAnimation(false);
        OnHarvesterCreated?.Invoke(this);
    }

    // 按采集范围内的目标资源状态驱动动画和定时采集。
    private void Update()
    {
        bool hasHarvestTarget = HasHarvestTarget();
        SetHarvestAnimation(hasHarvestTarget);

        if (!hasHarvestTarget)
        {
            _timer = 0;
            return;
        }

        _timer += Time.deltaTime;

        if (_timer < buildingSo.harvestSpeed)
            return;

        _timer = 0;

        HarvestAllTargets();
    }

    // 检查采集范围内是否存在当前建筑对应的资源。
    private bool HasHarvestTarget()
    {
        int resourceSize = Physics2D.OverlapCircle(transform.position, buildingSo.harvestRadius, _harvestContactFilter,
            _results);

        for (var i = 0; i < resourceSize; i++)
        {
            if (TryGetMatchingResourceNode(_results[i], out _))
                return true;
        }

        return false;
    }

    // 寻找并采集范围内所有匹配资源。
    private void HarvestAllTargets()
    {
        // 检测采集范围内是否有资源
        int resourceSize = Physics2D.OverlapCircle(transform.position, buildingSo.harvestRadius,_harvestContactFilter,
            _results);

        // 侦测到的资源数量
        int harvestedCount = 0;

        for (var i = 0; i < resourceSize; i++)
        {
            if (!TryGetMatchingResourceNode(_results[i], out ResourceNode node))
                continue;

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

    // 判断碰撞体是否挂载了当前采集器需要的资源节点。
    private bool TryGetMatchingResourceNode(Collider2D detectedCollider, out ResourceNode node)
    {
        node = null;

        if (!detectedCollider)
            return false;

        // 字典中没有
        if (!_resourceNodeByColliderDic.TryGetValue(detectedCollider, out node))
        {
            detectedCollider.TryGetComponent(out node);
            _resourceNodeByColliderDic.Add(detectedCollider, node);
        }

        return node && node.resourceSo == buildingSo.resourceSo;
    }

    // 按是否存在对应资源播放或暂停采集视觉表现。
    private void SetHarvestAnimation(bool shouldPlay)
    {
        if (_isHarvestAnimationPlaying == shouldPlay)
            return;

        _isHarvestAnimationPlaying = shouldPlay;
        SetAnimatorPlaying(shouldPlay);
        SetParticlePlaying(shouldPlay);
    }

    // 控制采集器动画状态。
    private void SetAnimatorPlaying(bool shouldPlay)
    {
        if (!_harvesterAnimator)
            return;

        _harvesterAnimator.speed = shouldPlay ? 1f : 0f;

        if (shouldPlay)
            return;

        _harvesterAnimator.Rebind();
        _harvesterAnimator.Update(0f);
    }

    // 控制采集器粒子特效状态。
    private void SetParticlePlaying(bool shouldPlay)
    {
        foreach (ParticleSystem harvestParticleSystem in _harvestParticleSystemList)
        {
            if (!harvestParticleSystem)
                continue;

            if (shouldPlay)
            {
                harvestParticleSystem.Play(true);
                continue;
            }

            harvestParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // 缓存当前建筑实例下的采集动画和粒子组件。
    private void CacheHarvestVisualComponents()
    {
        _harvesterAnimator = null;
        _harvestParticleSystemList.Clear();
        CollectHarvestVisualComponents(transform);
    }

    // 递归收集子节点上的采集视觉组件。
    private void CollectHarvestVisualComponents(Transform target)
    {
        if (!target)
            return;

        if (!_harvesterAnimator && target.TryGetComponent(out Animator animator))
            _harvesterAnimator = animator;

        if (target.TryGetComponent(out ParticleSystem harvestParticleSystem))
            _harvestParticleSystemList.Add(harvestParticleSystem);

        for (var i = 0; i < target.childCount; i++)
        {
            CollectHarvestVisualComponents(target.GetChild(i));
        }
    }

    // 采集单个资源。
    private void Harvest(ResourceNode node, int amountGained)
    {
        ResourceManager.Instance.AddResource(node.resourceSo, amountGained);
    }
}