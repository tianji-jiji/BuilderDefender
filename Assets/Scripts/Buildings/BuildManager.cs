using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    private const float RIGHT_CLICK_CANCEL_MAX_SQR_DISTANCE = 64f;

    public static BuildManager Instance;

    /// <summary>
    /// 建造占位数据，用于阻止建造中的建筑被其他建筑重叠。
    /// </summary>
    private class ConstructionSite
    {
        public readonly BuildingSo buildingSo;
        public readonly Vector3 position;

        public ConstructionSite(BuildingSo buildingSo, Vector3 position)
        {
            this.buildingSo = buildingSo;
            this.position = position;
        }
    }

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private BuildingConstructor buildingConstructorPrefab;

    private BuildingSo _currentBuildingSo;
    private readonly List<ConstructionSite> _constructionSites = new();
    private Vector3 _rightClickStartMousePosition;
    private bool _isWaitingForRightClickCancel;

    public event Action<BuildingSo> OnSelectedBuildingChanged;

    // 初始化建造管理器单例。
    private void Awake()
    {
        Instance = this;
    }

    // 处理建造选择、放置提示和建造输入。
    private void Update()
    {
        if (TryCancelCurrentBuildingSelection())
        {
            return;
        }

        if (!_currentBuildingSo)
        {
            HideBuildTooltips();
            return;
        }

        if (IsPointerOverUI())
        {
            HidePlacementPreviewOrTooltipIfNeeded();
            return;
        }

        Vector3 mousePosition = Utils.GetMousePosition();
        UpdatePlacementTooltip(mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceCurrentBuilding(mousePosition);
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
        HideBuildTooltips();
        return true;
    }

    // 设置当前选中的待建造建筑。
    public void SetCurrentBuilding(BuildingSo buildingSo)
    {
        _currentBuildingSo = buildingSo;
        OnSelectedBuildingChanged?.Invoke(buildingSo);

        if (!_currentBuildingSo)
        {
            TooltipManager.Instance.HideEfficiencyTooltip();
        }
    }

    // 判断指定位置是否允许建造。
    public bool IsAuthorizedConstructionZone(BuildingSo buildingSo, Vector3 position)
    {
        if (IsTooCloseToOthers(buildingSo, position))
        {
            return false;
        }

        if (IsTooCloseToConstructionSite(buildingSo, position))
        {
            return false;
        }

        return true;
    }

    // 判断当前位置是否与已有资源或建筑重叠。
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

    // 判断当前位置是否与正在建造的占位区域重叠。
    private bool IsTooCloseToConstructionSite(BuildingSo buildingSo, Vector3 position)
    {
        foreach (ConstructionSite constructionSite in _constructionSites)
        {
            Vector2 combinedSize = (buildingSo.size + constructionSite.buildingSo.size) * 0.5f;
            Vector2 distance = position - constructionSite.position;

            if (Mathf.Abs(distance.x) < combinedSize.x &&
                Mathf.Abs(distance.y) < combinedSize.y)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 计算建造在指定位置的采集效率。
    /// </summary>
    /// <param name="buildingSo">待建造建筑配置。</param>
    /// <param name="position">建造位置。</param>
    /// <returns>0 到 1 的效率比例。</returns>
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
            if (!node)
            {
                continue;
            }

            if (node.resourceSo == buildingSo.resourceSo)
            {
                validCount++;
            }
        }

        return Mathf.Clamp01((float)validCount / buildingSo.maxHarvestAmount);
    }

    // 开始建造指定建筑，并记录建造中占位。
    private bool StartConstruction(BuildingSo buildingSo, Vector3 position)
    {
        if (!buildingConstructorPrefab)
        {
            return false;
        }

        ConstructionSite constructionSite = new(buildingSo, position);
        _constructionSites.Add(constructionSite);

        BuildingConstructor buildingConstructor = Instantiate(buildingConstructorPrefab, position, Quaternion.identity);
        buildingConstructor.gameObject.SetActive(true);
        buildingConstructor.Init(buildingSo, () => _constructionSites.Remove(constructionSite));
        return true;
    }

    // 判断当前资源是否足够建造指定建筑。
    public bool CanAfford(BuildingSo buildingSo)
    {
        return ResourceManager.Instance && ResourceManager.Instance.CanAfford(buildingSo);
    }

    // 隐藏建造流程使用的提示。
    private void HideBuildTooltips()
    {
        TooltipManager.Instance.HidePlacementTooltip();
        TooltipManager.Instance.HideEfficiencyTooltip();
        TooltipManager.Instance.HidePriceTooltip();
    }

    // 判断鼠标是否悬停在 UI 上。
    private bool IsPointerOverUI()
    {
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }

    // 鼠标悬停 UI 时隐藏放置相关提示，避免保留上一帧状态。
    private void HidePlacementPreviewOrTooltipIfNeeded()
    {
        TooltipManager.Instance.HidePlacementTooltip();
        TooltipManager.Instance.HideEfficiencyTooltip();
    }

    // 根据当前位置刷新放置失败原因提示。
    private void UpdatePlacementTooltip(Vector3 position)
    {
        if (!IsAuthorizedConstructionZone(_currentBuildingSo, position))
        {
            TooltipManager.Instance.ShowPlacementTooltip("离资源太近了!");
            return;
        }

        if (!CanAfford(_currentBuildingSo))
        {
            TooltipManager.Instance.ShowPlacementTooltip("买不起这个建筑!");
            return;
        }

        TooltipManager.Instance.HidePlacementTooltip();
    }

    // 尝试在当前位置放置当前选中的建筑。
    private void TryPlaceCurrentBuilding(Vector3 position)
    {
        if (!_currentBuildingSo
            || !IsAuthorizedConstructionZone(_currentBuildingSo, position)
            || !CanAfford(_currentBuildingSo))
        {
            return;
        }

        TrySpendAndStartConstruction(_currentBuildingSo, position);
    }

    // 成功启动建造后扣除资源。
    private void TrySpendAndStartConstruction(BuildingSo buildingSo, Vector3 position)
    {
        bool constructionStarted = StartConstruction(buildingSo, position);
        if (constructionStarted)
        {
            ResourceManager.Instance.Spend(buildingSo);
        }
    }
}
