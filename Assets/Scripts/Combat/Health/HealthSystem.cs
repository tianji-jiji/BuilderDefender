using System;
using UnityEngine;

/// <summary>
/// 生命系统，负责保存生命值、处理伤害、治疗和死亡事件。
/// </summary>
public class HealthSystem : MonoBehaviour
{
    private const float DEFAULT_DAMAGE_TAKEN_MULTIPLIER = 1f;

    [SerializeField] private int maxHealth;

    public event Action OnDied;
    public event Action OnHealthChanged;

    private float _damageTakenMultiplier = DEFAULT_DAMAGE_TAKEN_MULTIPLIER;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public float CurrentHealthNormalized => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    // 初始化生命值并通知显示层刷新到满血状态。
    public void Init(int h)
    {
        maxHealth = Mathf.Max(1, h);
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    // 调整最大生命值，并按需要将当前生命恢复到新的上限。
    public void SetMaxHealth(int health, bool healToFull)
    {
        bool wasFullHealth = maxHealth > 0 && CurrentHealth >= maxHealth;
        maxHealth = Mathf.Max(1, health);
        CurrentHealth = healToFull || wasFullHealth
            ? maxHealth
            : Mathf.Clamp(CurrentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }

    // 设置受到伤害倍率。
    public void SetDamageTakenMultiplier(float multiplier)
    {
        _damageTakenMultiplier = Mathf.Max(0f, multiplier);
    }

    // 扣除生命值并返回实际造成的伤害。
    public int TakeDamage(int damage)
    {
        int adjustedDamage = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0, damage) * _damageTakenMultiplier));
        return LoseHealth(adjustedDamage);
    }

    // 直接扣除生命值，不经过受伤倍率，并返回实际扣除值。
    public int LoseHealth(int amount)
    {
        int healthLoss = Mathf.Max(0, amount);
        if (healthLoss <= 0)
        {
            return 0;
        }

        int previousHealth = CurrentHealth;
        CurrentHealth -= healthLoss;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        int actualHealthLoss = previousHealth - CurrentHealth;
        if (actualHealthLoss <= 0)
        {
            return 0;
        }

        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
        }

        return actualHealthLoss;
    }

    // 按最大生命值百分比治疗。
    public void HealByMaxHealthPercent(float healPercent)
    {
        int healAmount = Mathf.RoundToInt(maxHealth * Mathf.Max(0f, healPercent));
        Heal(healAmount);
    }

    // 恢复指定生命值。
    private void Heal(int amount)
    {
        int healAmount = Mathf.Max(0, amount);
        if (healAmount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount, 0, maxHealth);
        OnHealthChanged?.Invoke();
    }
}