using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event Action OnDied;
    public event Action OnDamaged;
    public event Action OnHealthChanged;

    public int MaxHealth { get; private set; }

    public int CurrentHealth { get; private set; }

    // 生命百分比
    public float CurrentHealthNormalized => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    // 初始化生命值并通知显示层刷新到满血状态。
    public void Init(int h)
    {
        MaxHealth = h;
        CurrentHealth = h;
        OnHealthChanged?.Invoke();
    }

    // 扣除生命值并在死亡时派发死亡事件。
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke();
        OnDamaged?.Invoke();

        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }
}
