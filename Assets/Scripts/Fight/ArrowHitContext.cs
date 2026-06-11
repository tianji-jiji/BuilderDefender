using UnityEngine;

/// <summary>
/// 箭矢命中时的一次性数据上下文，负责传递本次命中需要的运行时数据。
/// </summary>
public class ArrowHitContext
{
    public Enemy DirectHitEnemy { get; }
    public Vector3 HitPosition { get; }
    public int Damage { get; }
    public float ArmorIgnorePercent { get; }
    public DefenseSystem SourceDefenseSystem { get; }
    public float ExplosionRadius { get; }
    public float ExplosionDamageMultiplier { get; }
    public Collider2D[] EffectHitResults { get; }

    // 初始化本次箭矢命中的上下文数据。
    public ArrowHitContext(
        Enemy directHitEnemy,
        Vector3 hitPosition,
        int damage,
        float armorIgnorePercent,
        DefenseSystem sourceDefenseSystem,
        float explosionRadius,
        float explosionDamageMultiplier,
        Collider2D[] effectHitResults)
    {
        DirectHitEnemy = directHitEnemy;
        HitPosition = hitPosition;
        Damage = Mathf.Max(1, damage);
        ArmorIgnorePercent = Mathf.Clamp01(armorIgnorePercent);
        SourceDefenseSystem = sourceDefenseSystem;
        ExplosionRadius = Mathf.Max(0f, explosionRadius);
        ExplosionDamageMultiplier = Mathf.Max(0f, explosionDamageMultiplier);
        EffectHitResults = effectHitResults;
    }
}
