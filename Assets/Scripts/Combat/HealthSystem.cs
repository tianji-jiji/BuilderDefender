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
    // 受到伤害并产生实际扣血时触发，参数为实际扣除的生命值。
    public event Action<int> OnDamaged;
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
        bool wasFullHealth = this.maxHealth > 0 && currentHealth >= this.maxHealth;
        this.maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = healToFull || wasFullHealth
            ? this.maxHealth
            : Mathf.Clamp(currentHealth, 0, this.maxHealth);
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

        int previousHealth = currentHealth;
        currentHealth -= healthLoss;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        int actualHealthLoss = previousHealth - currentHealth;
        if (actualHealthLoss <= 0)
        {
            return 0;
        }

        OnHealthChanged?.Invoke();
        OnDamaged?.Invoke(actualHealthLoss);

        if (currentHealth <= 0)
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
