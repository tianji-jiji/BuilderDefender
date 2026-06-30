using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 建筑升星按钮，负责升级交互、资源提示和星级视觉显示。
/// </summary>
public class BuildingUpgradeButton : MonoBehaviour
{
    private const int MIN_STAR_LEVEL = 1;

    [SerializeField] private Button button;
    [SerializeField] private TowerUpgradeController upgradeController;
    [SerializeField] private Transform[] starVisuals;
    [SerializeField] private float starSpacing = 1.5f;
    [SerializeField] private bool hideButtonAtMaxStar = true;

    private void Start()
    {
        CacheReferences();
        BindEvents();
        RefreshStarVisuals();
        RefreshButtonState();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    // 缓存按钮依赖的本地组件。
    private void CacheReferences()
    {
        if (!button)
        {
            button = GetComponentInChildren<Button>();
        }

        if (!upgradeController)
        {
            upgradeController = GetComponent<TowerUpgradeController>();
        }
    }

    // 绑定升级、点击和资源变化事件。
    private void BindEvents()
    {
        if (button)
        {
            button.onClick.AddListener(TryUpgrade);
        }

        if (upgradeController)
        {
            upgradeController.OnStarLevelChanged += HandleStarLevelChanged;
        }

        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.OnResourceAmountChanged += RefreshButtonState;
        }
    }

    // 解绑升级、点击和资源变化事件。
    private void UnbindEvents()
    {
        if (button)
        {
            button.onClick.RemoveListener(TryUpgrade);
        }

        if (upgradeController)
        {
            upgradeController.OnStarLevelChanged -= HandleStarLevelChanged;
        }

        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.OnResourceAmountChanged -= RefreshButtonState;
        }
    }

    // 尝试消耗资源升级建筑星级。
    private void TryUpgrade()
    {
        if (!upgradeController)
        {
            RefreshButtonState();
            return;
        }

        if (upgradeController.TryUpgradeWithCost(ResourceManager.Instance))
        {
            return;
        }

        IReadOnlyList<ResourceCost> upgradeCostList = upgradeController.NextUpgradeCostList;
        if (upgradeCostList.Count > 0)
        {
            ShowUpgradeResourceWarning(upgradeCostList);
        }

        RefreshButtonState();
    }

    // 在星级变化后刷新按钮表现。
    private void HandleStarLevelChanged(int starLevel)
    {
        RefreshStarVisuals();
        RefreshButtonState();
    }

    // 显示资源不足的升星提示。
    private static void ShowUpgradeResourceWarning(IReadOnlyList<ResourceCost> upgradeCostList)
    {
        if (PopupManager.Instance)
        {
            PopupManager.Instance.ShowUpgradeResourceWarning(upgradeCostList);
        }
    }

    // 刷新星级视觉显示并让已显示星星保持居中排列。
    private void RefreshStarVisuals()
    {
        if (starVisuals == null || starVisuals.Length == 0)
        {
            return;
        }

        int currentStarLevel = upgradeController
            ? upgradeController.CurrentStarLevel
            : MIN_STAR_LEVEL;
        int visibleCount = Mathf.Clamp(currentStarLevel, MIN_STAR_LEVEL, starVisuals.Length);
        float startX = -(visibleCount - 1) * starSpacing * 0.5f;

        for (int i = 0; i < starVisuals.Length; i++)
        {
            Transform starVisual = starVisuals[i];
            if (!starVisual)
            {
                continue;
            }

            bool shouldShow = i < visibleCount;
            starVisual.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                starVisual.localPosition = new Vector3(startX + i * starSpacing, 0f, 0f);
            }
        }
    }

    // 刷新升星按钮的可点击和显示状态。
    private void RefreshButtonState()
    {
        bool canUpgrade = upgradeController && !upgradeController.IsMaxStar;
        if (hideButtonAtMaxStar)
        {
            gameObject.SetActive(canUpgrade);
        }

        if (button)
        {
            button.interactable = canUpgrade;
        }
    }
}
