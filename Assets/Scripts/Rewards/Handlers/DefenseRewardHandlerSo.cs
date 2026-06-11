/// <summary>
/// 闃插尽濉斿鍔?Handler 鍩虹被锛屾彁渚涜鍙栧弬鏁板拰濂栧姳鐘舵€佺殑閫氱敤鏂规硶銆?/// </summary>
public abstract class DefenseRewardHandlerSo : RewardEffectHandlerSo
{
    // 璇诲彇褰撳墠鏁堟灉鐨勪富鏁板€笺€?
    protected float GetValue(RewardEffectConfig config)
    {
        return RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.VALUE, config.LegacyValue);
    }

    // 灏濊瘯鍙栧緱闃插尽濉斿鍔辩姸鎬併€?
    protected bool TryGetDefenseRewardState(RewardEffectContext context, out DefenseRewardState defenseRewardState)
    {
        defenseRewardState = context?.DefenseRewardState;
        return defenseRewardState != null;
    }
}
