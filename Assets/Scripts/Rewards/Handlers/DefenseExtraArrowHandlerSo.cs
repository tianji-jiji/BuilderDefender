using UnityEngine;

/// <summary>
/// 防御塔额外攻击规则 Handler。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Handlers/Defense Extra Arrow Handler")]
public class DefenseExtraArrowHandlerSo : DefenseRewardHandlerSo
{
    private const string ATTACK_COUNTER_ID = "ExtraArrowAttack";

    public override bool ShouldRegisterRuntimeEffect => true;

    // 应用额外攻击规则。
    public override void Apply(RewardEffectContext context, RewardEffectConfig config)
    {
        if (!TryGetDefenseRewardState(context, out DefenseRewardState state))
        {
            return;
        }

        int triggerAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        state.AddExtraAttackRule(triggerAttackCount, extraAttackCount);
    }

    // 按攻击次数触发额外普通箭。
    public override void OnAfterAttack(DefenseCardEffectInstance instance, DefenseAttackContext context)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 0);
        int extraAttackCount = RewardEffectParameterReader.GetInt(instance.Config, RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 1);
        if (triggerAttackCount <= 0 || extraAttackCount <= 0)
        {
            return;
        }

        int attackCount = instance.IncrementCounter(ATTACK_COUNTER_ID);
        if (attackCount < triggerAttackCount)
        {
            return;
        }

        instance.ResetCounter(ATTACK_COUNTER_ID);
        context.RequestExtraAttack(extraAttackCount);
    }
}
