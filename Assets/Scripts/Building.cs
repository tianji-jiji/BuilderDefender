using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingSo buildingSo;
    private BuildingDemolitionButton _buildingDemolitionButton;
    private HealthSystem _healthSystem;
    private GameObject _buildingDestroyedParticles;
    
    private void Awake()
    {
        _buildingDestroyedParticles = Resources.Load<GameObject>("Particles/BuildingPlacedParticles");
        _buildingDemolitionButton = GetComponentInChildren<BuildingDemolitionButton>();
        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem)
        {
            _healthSystem.Init(buildingSo.maxHealth);
        }
    }

    private void Start()
    {
        _healthSystem.OnDied += Death;
        HideBuildingDemolitionButton();
    }

    private void Death()
    {
        Destroy(gameObject);
        Instantiate(_buildingDestroyedParticles, transform.position, Quaternion.identity);
    }

    private void OnMouseEnter()
    {
        ShowBuildingDemolitionButton();
    }

    private void OnMouseExit()
    {
        HideBuildingDemolitionButton();
    }

    private void ShowBuildingDemolitionButton()
    {
        if (_buildingDemolitionButton)
        {
            _buildingDemolitionButton.gameObject.SetActive(true);
        }
    }

    private void HideBuildingDemolitionButton()
    {
        if (_buildingDemolitionButton)
        {
            _buildingDemolitionButton.gameObject.SetActive(false);
        }
    }
}