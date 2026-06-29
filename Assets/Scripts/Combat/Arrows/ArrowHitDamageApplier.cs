using UnityEngine;

/// <summary>
/// 箭矢伤害执行器，负责护甲修正、扣血、飘字和击杀通知。
/// </summary>
public static class ArrowHitDamageApplier
{
    // 对指定敌人应用经过护甲修正的箭矢伤害。
    public static void ApplyDamage(Enemy enemy, int rawDamage, float armorIgnorePercent, DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        if (!IsEnemyValid(enemy) || !enemy.HealthSystem)
        {
            return;
        }

        bool wasAlive = enemy.IsAlive;
        int adjustedDamage = ArmorDamageCalculator.CalculateDamage(rawDamage, enemy.Armor, armorIgnorePercent);
        int actualDamage = enemy.HealthSystem.TakeDamage(adjustedDamage);
        DamageFloatingTextEvent.ShowDamage(enemy.DamageFloatingTextPosition, actualDamage);

        if (wasAlive && !enemy.IsAlive && sourceDefenseTowerCombatSystem)
        {
            sourceDefenseTowerCombatSystem.NotifyEnemyKilled();
        }
    }

    // 对指定敌人应用不经过护甲的固定伤害。
    public static void ApplyRawDamage(Enemy enemy, int damage, DamageFloatingTextStyle floatingTextStyle, DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        if (!IsEnemyValid(enemy) || !enemy.HealthSystem)
        {
            return;
        }

        bool wasAlive = enemy.IsAlive;
        int actualDamage = enemy.HealthSystem.LoseHealth(Mathf.Max(0, damage));
        DamageFloatingTextEvent.ShowDamage(enemy.DamageFloatingTextPosition, actualDamage, floatingTextStyle);

        if (wasAlive && !enemy.IsAlive && sourceDefenseTowerCombatSystem)
        {
            sourceDefenseTowerCombatSystem.NotifyEnemyKilled();
        }
    }

    // 对指定敌人应用最大生命值百分比伤害。
    public static void ApplyMaxHealthPercentDamage(Enemy enemy, float damagePercent, DamageFloatingTextStyle floatingTextStyle, DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        if (!IsEnemyValid(enemy) || !enemy.HealthSystem)
        {
            return;
        }

        bool wasAlive = enemy.IsAlive;
        int actualDamage = enemy.HealthSystem.LoseMaxHealthPercent(damagePercent);
        DamageFloatingTextEvent.ShowDamage(enemy.DamageFloatingTextPosition, actualDamage, floatingTextStyle);

        if (wasAlive && !enemy.IsAlive && sourceDefenseTowerCombatSystem)
        {
            sourceDefenseTowerCombatSystem.NotifyEnemyKilled();
        }
    }

    // 判断敌人当前是否仍然可以被箭矢命中。
    public static bool IsEnemyValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }
}
