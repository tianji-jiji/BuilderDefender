using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private GameObject bar;

    private void Start()
    {
        healthSystem.OnDamaged += UpdateBarSize;
       
        UpdateBarSize();
        UpdateBarVisible();
    }

    private void UpdateBarSize()
    {
        bar.transform.localScale = new Vector3(healthSystem.CurrentHealthNormalized, 1f, 1f);
        UpdateBarVisible();
    }
    
    private void UpdateBarVisible()
    {
        gameObject.SetActive(healthSystem.CurrentHealth < healthSystem.MaxHealth);
    }
}
