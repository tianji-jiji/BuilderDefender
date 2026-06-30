using UnityEngine;

/// <summary>
/// 防御塔穿透箭奖励应用器，负责让箭矢概率获得穿透次数。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/Appliers/Defense Tower Piercing Arrow Applier")]
public class TowerPiercingArrowApplierSo : TowerRewardApplierSo, ITowerArrowModifier
{
    // 修改单支箭的穿透能力。
    public void ModifyArrow(TowerEffectState runtimeState, TowerArrowContext context)
    {
        if (context == null || !ShouldTrigger(runtimeState.Config))
        {
            return;
        }

        int pierceCount = RewardEffectParameterReader.GetInt(runtimeState.Config, RewardEffectParameterIds.PIERCE_COUNT, 0, true);
        context.SetPierceCount(pierceCount);
    }

    // 判断本支箭是否触发穿透。
    private bool ShouldTrigger(RewardCardEffectConfig config)
    {
        float triggerChance = RewardEffectParameterReader.GetFloat(config, RewardEffectParameterIds.TRIGGER_CHANCE, GetValue(config));
        return triggerChance > 0f && Random.value < Mathf.Clamp01(triggerChance);
    }
}
