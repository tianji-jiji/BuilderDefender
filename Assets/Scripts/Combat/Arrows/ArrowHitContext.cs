using UnityEngine;

/// <summary>
/// 箭矢命中时的只读公共上下文，负责向组合能力传递本次命中的基础数据。
/// </summary>
public readonly struct ArrowHitContext
{
    public Enemy DirectHitEnemy { get; }
    public Vector3 HitPosition { get; }
    public int Damage { get; }
    public float ArmorIgnorePercent { get; }
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }

    // 创建本次箭矢命中的公共上下文。
    public ArrowHitContext(
        Enemy directHitEnemy,
        Vector3 hitPosition,
        int damage,
        float armorIgnorePercent,
        DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        DirectHitEnemy = directHitEnemy;
        HitPosition = hitPosition;
        Damage = Mathf.Max(1, damage);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
    }
}
