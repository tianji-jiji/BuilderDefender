using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingSo buildingSo;

    public static event Action<BuildingSo, Vector3> OnBuildingDestroyed;

    private BuildingDemolitionButton _buildingDemolitionButton;
    private HealthSystem _healthSystem;
    private GameObject _buildingDestroyedParticles;
    private int _baseMaxHealth;
    private float _upgradeMaxHealthMultiplier = 1f;

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
        }
    }

    // 订阅全局奖励变化事件。
    private void OnEnable()
    {
        RewardBonusManager.OnRewardBonusChanged += RefreshRewardBonuses;
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

    // 订阅建筑生命值死亡事件并隐藏拆除按钮。
    private void Start()
    {
        if (_healthSystem)
        {
            _healthSystem.OnDied += Death;
        }

        HideBuildingDemolitionButton();
    }

    // 处理建筑被战斗摧毁时的销毁、粒子和全局事件。
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
    }

    // 计算当前建筑应该拥有的最大生命值。
    private int CalculateMaxHealth()
    {
        float rewardMaxHealthMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseMaxHealthMultiplier
            : 1f;

        if (!buildingSo || buildingSo.buildingType != BuildingSo.BuildingType.Defense)
        {
            rewardMaxHealthMultiplier = 1f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(_baseMaxHealth * _upgradeMaxHealthMultiplier * rewardMaxHealthMultiplier));
    }

    // 取消订阅建筑生命值死亡事件。
    private void OnDisable()
    {
        RewardBonusManager.OnRewardBonusChanged -= RefreshRewardBonuses;

        if (_healthSystem)
        {
            _healthSystem.OnDied -= Death;
        }
    }
}
