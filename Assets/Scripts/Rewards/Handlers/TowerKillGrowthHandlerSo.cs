using UnityEngine;

/// <summary>
/// 闃插尽濉斿嚮鏉€鎴愰暱 Handler锛屽綋鍓嶆帴鍏ュ凡鏈夊嚮鏉€鑷姩鍗囨槦濂栧姳鐘舵€併€?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Kill Growth Handler")]
public class TowerKillGrowthHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤闃插尽濉斿嚮鏉€鎴愰暱閰嶇疆銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state) || config == null)
        {
            return;
        }

        int killCountToUpgrade = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.KILL_COUNT_TO_UPGRADE, 0, true);
        state.AddKillCountAutoUpgrade(killCountToUpgrade);

        // TODO锛氬鏋滃悗缁鍋氣€滃崟濉斿嚮鏉€鍚庢案涔呭姞鏀诲嚮鈥濓紝鍦ㄨ繖閲屾帴鍏ラ槻寰″鍑绘潃浜嬩欢鍜岃繍琛屾椂鎴愰暱鐘舵€併€?
    }
}
