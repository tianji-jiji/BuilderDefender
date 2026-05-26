using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    /// <summary>
    /// 建造地址
    /// </summary>
    private class ConstructionSite
    {
        public readonly BuildingSo BuildingSo;
        public readonly Vector3 Position;

        public ConstructionSite(BuildingSo buildingSo, Vector3 position)
        {
            BuildingSo = buildingSo;
            Position = position;
        }
    }

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private BuildingConstructor buildingConstructorPrefab;

    // 所有建筑
    private BuildingSoList _buildingSoList;

    // 当前选择的建筑
    private BuildingSo _currentBuildingSo;
    private readonly List<ConstructionSite> _constructionSites = new();

    public event Action<BuildingSo> OnSelectedBuildingChanged;

    private void Awake()
    {
        Instance = this;
        _buildingSoList = Resources.Load<BuildingSoList>("BuildingSoList");
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && _currentBuildingSo &&
            CanBuild(_currentBuildingSo, Utils.GetMousePosition()))
        {
            // 显示 tooltip(效率 = Physics2D.OverlapCircleNonAlloc的数量 / _currentBuildingSo.maxHarvestAmount)
            
            // 点击建造
            if (Input.GetMouseButtonDown(0))
            {
                // 建造过程动画
                bool constructionStarted = StartConstruction(_currentBuildingSo, Utils.GetMousePosition());
                // 花费资源
                if (constructionStarted)
                {
                    ResourceManager.Instance.Spend(_currentBuildingSo);
                }
            }
        }
    }

    public void SetCurrentBuilding(BuildingSo buildingSo)
    {
        _currentBuildingSo = buildingSo;
        OnSelectedBuildingChanged?.Invoke(buildingSo);
        
        if (!_currentBuildingSo)
        {
            TooltipManager.Instance.HideEfficiencyTooltip();
        }
    }

    public bool CanBuild(BuildingSo buildingSo, Vector3 position)
    {
        // 离一些物体太近不能建造（资源、建筑）
        if (IsTooCloseToOthers(buildingSo, position))
            return false;

        //离正在建造的建筑太近不能建筑
        if (IsTooCloseToConstructionSite(buildingSo, position))
            return false;

        // 钱不够不能建造
        if (!CanAfford(buildingSo))
            return false;

        return true;
    }

    private bool IsTooCloseToOthers(BuildingSo buildingSo, Vector3 position)
    {
        Collider2D[] results = new Collider2D[10];

        int size = Physics2D.OverlapBoxNonAlloc(
            position,
            buildingSo.size,
            0f,
            results,
            layerMask
        );

        return size > 0;
    }

    private bool IsTooCloseToConstructionSite(BuildingSo buildingSo, Vector3 position)
    {
        foreach (ConstructionSite constructionSite in _constructionSites)
        {
            Vector2 combinedSize = (buildingSo.size + constructionSite.BuildingSo.size) * 0.5f;
            Vector2 distance = position - constructionSite.Position;

            if (Mathf.Abs(distance.x) < combinedSize.x &&
                Mathf.Abs(distance.y) < combinedSize.y)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// 计算建造在此处的效率
    /// </summary>
    /// <param name="buildingSo"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public float CalculateEfficiency(BuildingSo buildingSo, Vector3 position)
    {
        Collider2D[] results = new Collider2D[50];

        int count = Physics2D.OverlapCircleNonAlloc(
            position,
            buildingSo.harvestRadius,
            results,
            layerMask
        );

        int validCount = 0;

        for (int i = 0; i < count; i++)
        {
            ResourceNode node = results[i].GetComponent<ResourceNode>();

            if (!node) continue;

            // 关键：类型匹配
            if (node.resourceSo == buildingSo.resourceSo)
            {
                validCount++;
            }
        }
        return Mathf.Clamp01(
            (float)validCount / buildingSo.maxHarvestAmount
        );
    }
    
    private bool StartConstruction(BuildingSo buildingSo, Vector3 position)
    {
        if (!buildingConstructorPrefab)
        {
            return false;
        }

        var constructionSite = new ConstructionSite(buildingSo, position);
        _constructionSites.Add(constructionSite);

        BuildingConstructor buildingConstructor = Instantiate(buildingConstructorPrefab, position, Quaternion.identity);
        buildingConstructor.gameObject.SetActive(true);
        buildingConstructor.Init(buildingSo, () => _constructionSites.Remove(constructionSite));
        return true;
    }

    private bool CanAfford(BuildingSo buildingSo)
    {
        foreach (var resourceCost in buildingSo.resourceCost)
        {
            if (resourceCost.amount >
                ResourceManager.Instance.GetResourceAmount(resourceCost.resourceSo))
            {
                return false;
            }
        }

        return true;
    }
}