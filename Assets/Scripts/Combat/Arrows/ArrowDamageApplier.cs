using UnityEngine;

/// <summary>
/// 箭矢伤害执行器，负责护甲修正、扣血、飘字和击杀通知。
/// </summary>
public static class ArrowDamageApplier
{
    // 对指定敌人应用箭矢伤害，并返回实际扣除的生命值。
    public static int ApplyDamage(Enemy enemy, int rawDamage, float armorIgnorePercent, DefenseSystem sourceDefenseSystem)
    {
        if (!IsEnemyValid(enemy) || !enemy.gameObject.TryGetComponent(out HealthSystem healthSystem))
        {
            return 0;
        }

        bool wasAlive = enemy.IsAlive;
        int adjustedDamage = ArmorDamageCalculator.CalculateDamage(rawDamage, enemy.Armor, armorIgnorePercent);
        int actualDamage = healthSystem.TakeDamage(adjustedDamage);
        DamageFloatingTextService.ShowEnemyDamage(enemy.DamageFloatingTextPosition, actualDamage);

        if (actualDamage > 0 && sourceDefenseSystem)
        {
            sourceDefenseSystem.NotifyEnemyHit(enemy, actualDamage);
        }

        if (wasAlive && !enemy.IsAlive && sourceDefenseSystem)
        {
            sourceDefenseSystem.NotifyEnemyKilled(enemy);
        }

        return actualDamage;
    }

    // 判断敌人当前是否仍然可以被箭矢命中。
    public static bool IsEnemyValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }
}
