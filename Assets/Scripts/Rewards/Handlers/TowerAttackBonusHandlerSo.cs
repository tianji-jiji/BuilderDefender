using UnityEngine;

/// <summary>
/// 闃插尽濉旀敾鍑诲姏鍔犳垚 Handler锛岃礋璐ｆ妸鍗＄墝鍙傛暟鍐欏叆闃插尽濉斿鍔辩姸鎬併€?/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Tower Attack Bonus Handler")]
public class TowerAttackBonusHandlerSo : DefenseRewardHandlerSo
{
    // 搴旂敤闃插尽濉旀敾鍑诲姏鍔犳垚銆?
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state) || config == null)
        {
            return;
        }

        state.AddAttackDamageBonus(GetValue(config));
    }
}
