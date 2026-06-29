using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 建筑升星按钮，负责消耗资源升星和对外提供无消耗升星能力。
/// </summary>
public class BuildingUpgradeButton : MonoBehaviour
{
    private const int MIN_STAR_LEVEL = 1;

    [SerializeField] private Button button;
    [SerializeField] private BuildingUpgradeConfigSo upgradeConfig;
    [SerializeField] private Building building;
    [FormerlySerializedAs("defenseTowerCombatSystem")]
    [FormerlySerializedAs("defenseTowerSystem")]
    [FormerlySerializedAs("defenseSystem")]
    [SerializeField] private TowerCombatSystem towerCombatSystem;
    [SerializeField] private Transform[] starVisuals;
    [SerializeField] private float starSpacing = 1.5f;
    [SerializeField] private int currentStarLevel = MIN_STAR_LEVEL;
    [SerializeField] private bool hideButtonAtMaxStar = true;

    public int CurrentStarLevel => currentStarLevel;
    public int MaxStarLevel => upgradeConfig ? upgradeConfig.MaxStarLevel : currentStarLevel;
    public bool IsMaxStar => currentStarLevel >= MaxStarLevel;
    public TowerCombatSystem TowerCombatSystem => towerCombatSystem;

    // 初始化按钮监听、初始星级和视觉状态。
    private void Start()
    {
        CacheUpgradeTargets();
        ApplyInitialStarBonusFromReward();
        TowerRegistry.RegisterUpgradeButton(this);

        if (button)
        {
            button.onClick.AddListener(TryUpgrade);
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceAmountChanged += RefreshButtonState;
        }

        RefreshStarVisuals();
        RefreshButtonState();
    }

    // 尝试消耗资源升级建筑星级。
    private void TryUpgrade()
    {
        if (!TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel))
        {
            RefreshButtonState();
            return;
        }

        if (!ResourceManager.Instance || !ResourceManager.Instance.CanAfford(upgradeLevel.UpgradeCost))
        {
            ShowUpgradeResourceWarning(upgradeLevel);
            return;
        }

        ResourceManager.Instance.Spend(upgradeLevel.UpgradeCost);
        ApplyStarLevelWithoutCost(currentStarLevel + 1);
    }

    // 无消耗提升一星。
    public bool UpgradeOneLevelWithoutCost()
    {
        if (IsMaxStar)
        {
            return false;
        }

        return ApplyStarLevelWithoutCost(currentStarLevel + 1);
    }

    // 无消耗升到满星。
    public bool UpgradeToMaxStarWithoutCost()
    {
        if (IsMaxStar)
        {
            return false;
        }

        return ApplyStarLevelWithoutCost(MaxStarLevel);
    }

    // 按奖励配置应用新建防御塔初始星级。
    public bool ApplyInitialStarBonus(int initialStarBonus)
    {
        if (initialStarBonus <= 0 || IsMaxStar)
        {
            return false;
        }

        int targetStarLevel = Mathf.Clamp(currentStarLevel + initialStarBonus, MIN_STAR_LEVEL, MaxStarLevel);
        return ApplyStarLevelWithoutCost(targetStarLevel);
    }

    // 缓存当前升级按钮控制的建筑和防御组件。
    private void CacheUpgradeTargets()
    {
        if (!building)
        {
            building = GetComponentInParent<Building>();
        }

        if (!towerCombatSystem)
        {
            towerCombatSystem = GetComponentInParent<TowerCombatSystem>();
        }
    }

    // 应用奖励提供的新建防御塔初始星级。
    private void ApplyInitialStarBonusFromReward()
    {
        if (!RewardRuntimeCoordinator.Instance || !towerCombatSystem)
        {
            return;
        }

        ApplyInitialStarBonus(RewardRuntimeCoordinator.Instance.TowerRewards.ActiveRewards.NewTowerInitialStarBonus);
    }

    // 将目标星级配置应用到建筑生命和战斗系统。
    private bool ApplyStarLevelWithoutCost(int targetStarLevel)
    {
        if (!upgradeConfig)
        {
            return false;
        }

        targetStarLevel = Mathf.Clamp(targetStarLevel, MIN_STAR_LEVEL, MaxStarLevel);
        if (targetStarLevel <= currentStarLevel)
        {
            return false;
        }

        BuildingUpgradeLevel upgradeLevel = upgradeConfig.GetUpgradeLevel(targetStarLevel);
        if (upgradeLevel == null)
        {
            return false;
        }

        currentStarLevel = targetStarLevel;
        ApplyUpgradeLevel(upgradeLevel);
        RefreshStarVisuals();
        RefreshButtonState();
        return true;
    }

    // 将当前星级配置应用到建筑生命和战斗系统。
    private void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (building)
        {
            building.ApplyUpgradeLevel(upgradeLevel);
        }

        if (towerCombatSystem)
        {
            towerCombatSystem.ApplyUpgradeLevel(upgradeLevel);
        }
    }

    // 显示资源不足的升星提示。
    private void ShowUpgradeResourceWarning(BuildingUpgradeLevel upgradeLevel)
    {
        if (PopupManager.Instance)
        {
            PopupManager.Instance.ShowUpgradeResourceWarning(upgradeLevel.UpgradeCost);
        }
    }

    // 刷新星级视觉显示并让已显示星星保持居中排列。
    private void RefreshStarVisuals()
    {
        if (starVisuals == null || starVisuals.Length == 0)
        {
            return;
        }

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
        bool hasNextLevel = TryGetNextUpgradeLevel(out _);

        if (hideButtonAtMaxStar)
        {
            gameObject.SetActive(hasNextLevel);
        }

        if (button)
        {
            button.interactable = hasNextLevel;
        }
    }

    // 获取下一星级的配置。
    private bool TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel)
    {
        upgradeLevel = null;

        if (!upgradeConfig)
        {
            return false;
        }

        upgradeLevel = upgradeConfig.GetUpgradeLevel(currentStarLevel + 1);
        return upgradeLevel != null;
    }

    // 取消按钮监听，避免对象销毁后继续持有回调。
    private void OnDestroy()
    {
        TowerRegistry.UnregisterUpgradeButton(this);

        if (button)
        {
            button.onClick.RemoveListener(TryUpgrade);
        }

        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.OnResourceAmountChanged -= RefreshButtonState;
        }
    }
}
