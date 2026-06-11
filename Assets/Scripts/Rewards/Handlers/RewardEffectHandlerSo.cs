using UnityEngine;

/// <summary>
/// Reward 鏁堟灉杩愯鏃跺鐞嗗櫒锛屾瘡涓€绉嶅鏉傛晥鏋滃彲浠ラ€氳繃鐙珛 Handler 瀹炵幇銆?/// </summary>
public abstract class RewardEffectHandlerSo : ScriptableObject
{
    // 搴旂敤褰撳墠 Reward 鏁堟灉閰嶇疆銆?
    public abstract void Apply(RewardEffectContext context, RewardEffectConfig config);
}
