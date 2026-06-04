using System;
using UnityEngine;

/// <summary>
/// 生命系统，负责保存生命值、处理伤害、治疗和死亡事件。
/// </summary>
public class HealthSystem : MonoBehaviour
{
    private const float DEFAULT_DAMAGE_TAKEN_MULTIPLIER = 1f;

    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;

    // 生命归零时触发。
    public event Action OnDied;
    // 受到伤害时触发。
    public event Action OnDamaged;
    // 生命值或最大生命值变化时触发。
    public event Action OnHealthChanged;

    private float _damageTakenMultiplier = DEFAULT_DAMAGE_TAKEN_MULTIPLIER;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float CurrentHealthNormalized => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    // 初始化生命值并通知显示层刷新到满血状态。
    public void Init(int h)
    {
        maxHealth = Mathf.Max(1, h);
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    // 调整最大生命值，并按需要将当前生命恢复到新的上限。
    public void SetMaxHealth(int maxHealth, bool healToFull)
    {
        this.maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = healToFull ? this.maxHealth : Mathf.Clamp(currentHealth, 0, this.maxHealth);
        OnHealthChanged?.Invoke();
    }

    // 设置受到伤害倍率。
    public void SetDamageTakenMultiplier(float multiplier)
    {
        _damageTakenMultiplier = Mathf.Max(0f, multiplier);
    }

    // 扣除生命值并在死亡时派发死亡事件。
    public void TakeDamage(int damage)
    {
        int adjustedDamage = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0, damage) * _damageTakenMultiplier));
        LoseHealth(adjustedDamage);
    }

    // 直接扣除生命值，不经过受伤倍率。
    public void LoseHealth(int amount)
    {
        int healthLoss = Mathf.Max(0, amount);
        if (healthLoss <= 0)
        {
            return;
        }

        currentHealth -= healthLoss;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke();
        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }

    // 按最大生命值百分比治疗。
    public void HealByMaxHealthPercent(float healPercent)
    {
        int healAmount = Mathf.RoundToInt(maxHealth * Mathf.Max(0f, healPercent));
        Heal(healAmount);
    }

    // 恢复指定生命值。
    public void Heal(int amount)
    {
        int healAmount = Mathf.Max(0, amount);
        if (healAmount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }
}
