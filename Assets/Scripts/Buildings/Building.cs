using System;
using UnityEngine;

/// <summary>
/// 建筑实体组件，负责建筑生命、销毁、拆除按钮显示和奖励生命属性刷新。
/// </summary>
public class Building : MonoBehaviour
{
    [SerializeField] private BuildingSo buildingSo;

    // 建筑被摧毁时触发，通知建筑类型和位置。
    public static event Action<BuildingSo, Vector3> OnBuildingDestroyed;

    private BuildingDemolitionButton _buildingDemolitionButton;
    private HealthSystem _healthSystem;
    private GameObject _buildingDestroyedParticles;
    private int _baseMaxHealth;
    private float _upgradeMaxHealthMultiplier = 1f;

    public bool IsDefenseBuilding => buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Defense;
    public bool IsHomeBuilding => buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Home;

    // 初始化建筑需要缓存的组件和生命值。
    private void Awake()
    {
        _buildingDestroyedParticles = Resources.Load<GameObject>("Particles/BuildingDestroyedParticles");
        _buildingDemolitionButton = GetComponentInChildren<BuildingDemolitionButton>();
        _healthSystem = GetComponent<HealthSystem>();
        _baseMaxHealth = buildingSo ? buildingSo.maxHealth : 1;

        if (_healthSystem)
        {
            _healthSystem.Init(CalculateMaxHealth());
            _healthSystem.SetDamageTakenMultiplier(CalculateDamageTakenMultiplier());
        }
    }

    // 订阅全局奖励变化并注册到运行时查询表。
    private void OnEnable()
    {
        RewardBonusManager.OnRewardBonusChanged += RefreshRewardBonuses;
        RegisterToRuntimeRegistry();
    }

    // 根据升星配置提升建筑生命值。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (!_healthSystem || upgradeLevel == null)
        {
            return;
        }

        _upgradeMaxHealthMultiplier = upgradeLevel.MaxHealthMultiplier;
        RefreshRewardBonuses();
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

    // 订阅建筑生命值死亡事件并隐藏拆除按钮。
    private void Start()
    {
        if (_healthSystem)
        {
            _healthSystem.OnDied += Death;
        }

        HideBuildingDemolitionButton();
    }

    // 处理建筑被摧毁时的销毁、粒子和全局事件。
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
        if (!_buildingDestroyedParticles)
        {
            return;
        }

        if (PoolManager.Instance)
        {
            PoolManager.Instance.Spawn(_buildingDestroyedParticles, transform.position, Quaternion.identity);
            return;
        }

        Instantiate(_buildingDestroyedParticles, transform.position, Quaternion.identity);
    }

    // 根据升星倍率和全局奖励倍率刷新建筑生命值。
    private void RefreshRewardBonuses()
    {
        if (!_healthSystem)
        {
            return;
        }

        _healthSystem.SetMaxHealth(CalculateMaxHealth(), true);
        _healthSystem.SetDamageTakenMultiplier(CalculateDamageTakenMultiplier());
    }

    // 计算当前建筑应该拥有的最大生命值。
    private int CalculateMaxHealth()
    {
        float rewardMaxHealthMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseMaxHealthMultiplier
            : 1f;

        if (!IsDefenseBuilding)
        {
            rewardMaxHealthMultiplier = 1f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(_baseMaxHealth * _upgradeMaxHealthMultiplier * rewardMaxHealthMultiplier));
    }

    // 计算当前建筑受到伤害时使用的伤害倍率。
    private float CalculateDamageTakenMultiplier()
    {
        if (!IsDefenseBuilding || !RewardBonusManager.Instance)
        {
            return 1f;
        }

        return RewardBonusManager.Instance.DefenseDamageTakenMultiplier;
    }

    // 将建筑注册到运行时查询表。
    private void RegisterToRuntimeRegistry()
    {
        if (IsDefenseBuilding)
        {
            DefenseTowerRegistry.RegisterDefenseBuilding(this);
        }

        if (IsHomeBuilding && _healthSystem)
        {
            DefenseTowerRegistry.RegisterHomeHealthSystem(_healthSystem);
        }
    }

    // 从运行时查询表取消注册建筑。
    private void UnregisterFromRuntimeRegistry()
    {
        if (IsDefenseBuilding)
        {
            DefenseTowerRegistry.UnregisterDefenseBuilding(this);
        }

        if (IsHomeBuilding && _healthSystem)
        {
            DefenseTowerRegistry.UnregisterHomeHealthSystem(_healthSystem);
        }
    }

    // 取消订阅建筑生命值死亡事件。
    private void OnDisable()
    {
        RewardBonusManager.OnRewardBonusChanged -= RefreshRewardBonuses;
        UnregisterFromRuntimeRegistry();

        if (_healthSystem)
        {
            _healthSystem.OnDied -= Death;
        }
    }
}
