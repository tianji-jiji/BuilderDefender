using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event Action OnDied;
    public event Action OnDamaged;

    public int MaxHealth { get; private set; }

    public int CurrentHealth { get; private set; }

    // 生命百分比
    public float CurrentHealthNormalized => (float)CurrentHealth / MaxHealth;

    public void Init(int h)
    {
        MaxHealth = h;
        CurrentHealth = h;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnDamaged?.Invoke();

        if (CurrentHealth <= 0)
            OnDied?.Invoke();
    }
}