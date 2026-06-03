using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingSo buildingSo;

    public static event Action<BuildingSo, Vector3> OnBuildingDestroyed;

    private BuildingDemolitionButton _buildingDemolitionButton;
    private HealthSystem _healthSystem;
    private GameObject _buildingDestroyedParticles;

    // 初始化建筑需要缓存的组件和生命值。
    private void Awake()
    {
        _buildingDestroyedParticles = Resources.Load<GameObject>("Particles/BuildingDestroyedParticles");
        _buildingDemolitionButton = GetComponentInChildren<BuildingDemolitionButton>();
        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem)
        {
            _healthSystem.Init(buildingSo.maxHealth);
        }
    }

    // 订阅建筑生命值死亡事件并隐藏拆除按钮。
    private void Start()
    {
        _healthSystem.OnDied += Death;
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
        if (_buildingDemolitionButton)
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

    // 取消订阅建筑生命值死亡事件。
    private void OnDisable()
    {
        if (_healthSystem)
        {
            _healthSystem.OnDied -= Death;
        }
    }
}
