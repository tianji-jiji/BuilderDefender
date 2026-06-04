using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    public event Action OnDied;
    public event Action OnDamaged;
    public event Action OnHealthChanged;

    public int MaxHealth => maxHealth;

    public int CurrentHealth => currentHealth;

    // 生命百分比
    public float CurrentHealthNormalized => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    // 初始化生命值并通知显示层刷新到满血状态。
    public void Init(int h)
    {
        maxHealth = h;
        currentHealth = h;
        OnHealthChanged?.Invoke();
    }

    // 调整最大生命值，并按需要将当前生命恢复到新的上限。
    public void SetMaxHealth(int maxHealth, bool healToFull)
    {
        this.maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = healToFull ? this.maxHealth : Mathf.Clamp(currentHealth, 0, this.maxHealth);
        OnHealthChanged?.Invoke();
    }

    // 扣除生命值并在死亡时派发死亡事件。
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke();
        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }
}
