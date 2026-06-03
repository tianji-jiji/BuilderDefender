using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

public class BuildManager : MonoBehaviour
{
    private const float RIGHT_CLICK_CANCEL_MAX_SQR_DISTANCE = 64f;

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
    private Vector3 _rightClickStartMousePosition;
    private bool _isWaitingForRightClickCancel;

    public event Action<BuildingSo> OnSelectedBuildingChanged;

    private void Awake()
    {
        Instance = this;
        _buildingSoList = Resources.Load<BuildingSoList>("BuildingSoList");
    }

    private void Update()
    {
        if (TryCancelCurrentBuildingSelection())
        {
            return;
        }

        if (!_currentBuildingSo)
        {
            TooltipManager.Instance.HidePlacementTooltip();
            TooltipManager.Instance.HideEfficiencyTooltip();
            TooltipManager.Instance.HidePriceTooltip();;
        }
        
        // 鼠标在正确位置并且点击建筑按钮
        if (!EventSystem.current.IsPointerOverGameObject() && _currentBuildingSo)

            // 是合法建造区域
            if (IsAuthorizedConstructionZone(_currentBuildingSo, Utils.GetMousePosition()))
            {
                TooltipManager.Instance.HidePlacementTooltip();
                // 钱足够
                if (CanAfford(_currentBuildingSo))
                {
                    TooltipManager.Instance.HidePlacementTooltip();
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
                else
                {
                    TooltipManager.Instance.ShowPlacementTooltip("Can not afford this building!!!");
                }
            }
            else
            {
                TooltipManager.Instance.ShowPlacementTooltip("Too Close to Construction or ResourceNode!!!");
        }
    }

    // 右键短点击取消当前已选择的待建造建筑，右键拖拽镜头时不取消。
    private bool TryCancelCurrentBuildingSelection()
    {
        if (!_currentBuildingSo)
        {
            _isWaitingForRightClickCancel = false;
            return false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            _rightClickStartMousePosition = Input.mousePosition;
            _isWaitingForRightClickCancel = true;
            return false;
        }

        if (!_isWaitingForRightClickCancel)
        {
            return false;
        }

        float sqrDistance = (Input.mousePosition - _rightClickStartMousePosition).sqrMagnitude;
        if (Input.GetMouseButton(1) && sqrDistance > RIGHT_CLICK_CANCEL_MAX_SQR_DISTANCE)
        {
            _isWaitingForRightClickCancel = false;
            return false;
        }

        if (!Input.GetMouseButtonUp(1))
        {
            return false;
        }

        _isWaitingForRightClickCancel = false;
        if (sqrDistance > RIGHT_CLICK_CANCEL_MAX_SQR_DISTANCE)
        {
            return false;
        }

        SetCurrentBuilding(null);
        TooltipManager.Instance.HidePlacementTooltip();
        TooltipManager.Instance.HideEfficiencyTooltip();
        TooltipManager.Instance.HidePriceTooltip();
        return true;
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

    // 是否是合法建造区域
    public bool IsAuthorizedConstructionZone(BuildingSo buildingSo, Vector3 position)
    {
        // 离一些物体太近不能建造（资源、建筑）
        if (IsTooCloseToOthers(buildingSo, position))
            return false;

        //离正在建造的建筑太近不能建筑
        if (IsTooCloseToConstructionSite(buildingSo, position))
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

    public bool CanAfford(BuildingSo buildingSo)
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
