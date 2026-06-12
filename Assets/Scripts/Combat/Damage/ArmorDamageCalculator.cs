using UnityEngine;

/// <summary>
/// 负责计算护甲和穿甲影响后的最终伤害。
/// </summary>
public static class ArmorDamageCalculator
{
    private const float ARMOR_FORMULA_BASE = 100f;

    // 根据原始伤害、护甲和穿甲比例计算最终伤害，先把护甲按“穿甲比例”削减，再用剩余护甲降低伤害
    public static int CalculateDamage(int rawDamage, int armor, float armorIgnorePercent)
    {
        int safeRawDamage = Mathf.Max(1, rawDamage);
        // 实际生效护甲
        float effectiveArmor = Mathf.Max(0, armor) * (1f - Mathf.Clamp01(armorIgnorePercent));
        // 伤害倍率
        float damageMultiplier = ARMOR_FORMULA_BASE / (ARMOR_FORMULA_BASE + effectiveArmor);
        return Mathf.Max(1, Mathf.RoundToInt(safeRawDamage * damageMultiplier));
    }
}
