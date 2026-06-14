using UnityEngine;

/// <summary>
/// 箭矢命中特殊效果基类，只保存配置数据并通过命中上下文执行效果。
/// </summary>
public abstract class ArrowHitEffectSo : ScriptableObject
{
    // 应用箭矢命中特殊效果。
    public abstract void Apply(ArrowHitContext context);
}
