using System;
using UnityEngine;

/// <summary>
/// 建筑实体组件，负责建筑生命、销毁、拆除按钮显示和奖励生命属性刷新。
/// </summary>
public class Building : MonoBehaviour
{
    [SerializeField] private BuildingSo buildingSo;
    [SerializeField] private GameObject destroyedParticlePrefab;

    // 建筑被摧毁时触发，通知建筑类型和位置。
    public static event Action<BuildingSo, Vector3> OnBuildingDestroyed;

    private BuildingDemolitionButton _buildingDemolitionButton;
    private HealthSystem _healthSystem;
    private int _baseMaxHealth;
    private float _upgradeMaxHealthMultiplier = 1f;
    private bool _isSubscribedToHealthSystem;
    private bool _isSubscribedToRewardBonuses;
    private bool _isRegisteredToRuntimeRegistry;
    private BuildingRuntimeRegistry _buildingRegistry;
    private RewardRuntimeCoordinator _rewardCoordinator;

    private bool IsDefenseBuilding => buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Defense;
    private bool IsHomeBuilding => buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Home;

    // 初始化建筑需要缓存的组件和生命值。
    private void Awake()
    {
        _buildingDemolitionButton = GetComponentInChildren<BuildingDemolitionButton>();
        _healthSystem = GetComponent<HealthSystem>();
        _baseMaxHealth = buildingSo ? buildingSo.maxHealth : 1;

        if (_healthSystem)
        {
            _healthSystem.Init(CalculateMaxHealth());
            _healthSystem.SetDamageTakenMultiplier(CalculateDamageTakenMultiplier());
        }
    }

    private void OnEnable()
    {
        TrySubscribeRewardBonuses();
        TrySubscribeHealthSystem();
        if (_buildingRegistry != null)
        {
            RegisterToRuntimeRegistry();
        }
    }

    private void OnDisable()
    {
        TryUnsubscribeRewardBonuses();
        UnregisterFromRuntimeRegistry();
        TryUnsubscribeHealthSystem();
    }

    // 启动时缓存注册表、注册建筑并隐藏拆除按钮。
    private void Start()
    {
        CacheBuildingRegistry();
        RegisterToRuntimeRegistry();
        TrySubscribeRewardBonuses();
        HideBuildingDemolitionButton();
    }
    
    // 根据升星配置提升建筑生命值。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (!_healthSystem || upgradeLevel == null)
        {
            return;
        }

        _upgradeMaxHealthMultiplier = upgradeLevel.MaxHealthMultiplier;
        RefreshRewardBonuses(true);
    }

    // 按最大生命值百分比治疗当前建筑。
    public void HealByMaxHealthPercent(float healPercent)
    {
        if (!_healthSystem)
        {
            return;
        }

        _healthSystem.HealByMaxHealthPercent(healPercent);
    }

    // 处理建筑被摧毁时的事件、粒子和对象销毁。
    private void Death()
    {
        OnBuildingDestroyed?.Invoke(buildingSo, transform.position);
        SpawnDestroyedParticles();
        Destroy(gameObject);
    }

    // 鼠标进入建筑时显示拆除按钮。
    private void OnMouseEnter()
    {
        ShowBuildingDemolitionButton();
    }

    // 鼠标离开建筑时隐藏拆除按钮。
    private void OnMouseExit()
    {
        HideBuildingDemolitionButton();
    }

    // 显示当前建筑的拆除按钮。
    private void ShowBuildingDemolitionButton()
    {
        if (_buildingDemolitionButton)
        {
            _buildingDemolitionButton.gameObject.SetActive(true);
        }
    }

    // 隐藏当前建筑的拆除按钮。
    private void HideBuildingDemolitionButton()
    {
        if (_buildingDemolitionButton && !_buildingDemolitionButton.IsAwaitingConfirmation)
        {
            _buildingDemolitionButton.gameObject.SetActive(false);
        }
    }

    // 生成建筑摧毁粒子。
    private void SpawnDestroyedParticles()
    {
        if (!destroyedParticlePrefab)
        {
            return;
        }

        if (PoolManager.Instance)
        {
            PoolManager.Instance.Spawn(destroyedParticlePrefab, transform.position, Quaternion.identity);
            return;
        }

        Instantiate(destroyedParticlePrefab, transform.position, Quaternion.identity);
    }

    // 刷新全局奖励带来的建筑属性变化，保留当前生命值。
    private void RefreshRewardBonuses()
    {
        RefreshRewardBonuses(false);
    }

    // 按指定回血策略刷新建筑生命上限和受伤倍率。
    private void RefreshRewardBonuses(bool healToFull)
    {
        if (!_healthSystem)
        {
            return;
        }

        _healthSystem.SetMaxHealth(CalculateMaxHealth(), healToFull);
        _healthSystem.SetDamageTakenMultiplier(CalculateDamageTakenMultiplier());
    }

    // 计算当前建筑应该拥有的最大生命值。
    private int CalculateMaxHealth()
    {
        TowerRewardState activeRewards = GetTowerRewardState();
        float rewardMaxHealthMultiplier = activeRewards != null ? activeRewards.MaxHealthMultiplier : 1f;

        if (!IsDefenseBuilding)
        {
            rewardMaxHealthMultiplier = 1f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(_baseMaxHealth * _upgradeMaxHealthMultiplier * rewardMaxHealthMultiplier));
    }

    // 计算当前建筑受到伤害时使用的伤害倍率。
    private float CalculateDamageTakenMultiplier()
    {
        if (!IsDefenseBuilding || !RewardRuntimeCoordinator.Instance)
        {
            return 1f;
        }

        TowerRewardState activeRewards = GetTowerRewardState();
        return activeRewards != null ? activeRewards.DamageTakenMultiplier : 1f;
    }

    // 获取当前防御塔奖励状态。
    private TowerRewardState GetTowerRewardState()
    {
        _rewardCoordinator = _rewardCoordinator
            ? _rewardCoordinator
            : RewardRuntimeCoordinator.Instance;
        return _rewardCoordinator
            ? _rewardCoordinator.TowerRewards?.State
            : null;
    }

    // 订阅建筑生命值死亡事件。
    private void TrySubscribeHealthSystem()
    {
        if (_isSubscribedToHealthSystem || !_healthSystem)
        {
            return;
        }

        _healthSystem.OnDied += Death;
        _isSubscribedToHealthSystem = true;
    }

    // 取消订阅建筑生命值死亡事件。
    private void TryUnsubscribeHealthSystem()
    {
        if (!_isSubscribedToHealthSystem || !_healthSystem)
        {
            return;
        }

        _healthSystem.OnDied -= Death;
        _isSubscribedToHealthSystem = false;
    }

    // 订阅全局奖励变化事件。
    private void TrySubscribeRewardBonuses()
    {
        if (_isSubscribedToRewardBonuses)
        {
            return;
        }

        _rewardCoordinator = RewardRuntimeCoordinator.Instance;
        if (!_rewardCoordinator)
        {
            return;
        }

        _rewardCoordinator.OnActiveRewardsChanged += RefreshRewardBonuses;
        _isSubscribedToRewardBonuses = true;
    }

    // 取消订阅全局奖励变化事件。
    private void TryUnsubscribeRewardBonuses()
    {
        if (!_isSubscribedToRewardBonuses)
        {
            return;
        }

        if (_rewardCoordinator)
        {
            _rewardCoordinator.OnActiveRewardsChanged -= RefreshRewardBonuses;
        }

        _rewardCoordinator = null;
        _isSubscribedToRewardBonuses = false;
    }

    // 将建筑注册到运行时查询表。
    private void RegisterToRuntimeRegistry()
    {
        if (_isRegisteredToRuntimeRegistry || _buildingRegistry == null)
        {
            return;
        }

        if (IsDefenseBuilding)
        {
            _buildingRegistry.RegisterBuilding(this);
        }

        if (IsHomeBuilding && _healthSystem)
        {
            _buildingRegistry.RegisterHomeHealth(_healthSystem);
        }

        _isRegisteredToRuntimeRegistry = true;
    }

    // 从运行时查询表取消注册建筑。
    private void UnregisterFromRuntimeRegistry()
    {
        if (!_isRegisteredToRuntimeRegistry)
        {
            return;
        }

        if (IsDefenseBuilding)
        {
            _buildingRegistry?.UnregisterBuilding(this);
        }

        if (IsHomeBuilding && _healthSystem)
        {
            _buildingRegistry?.UnregisterHomeHealth(_healthSystem);
        }

        _isRegisteredToRuntimeRegistry = false;
    }

    // 缓存场景建筑注册表。
    private void CacheBuildingRegistry()
    {
        _buildingRegistry = BuildingPlacementManager.Instance
            ? BuildingPlacementManager.Instance.BuildingRegistry
            : null;
    }
}
