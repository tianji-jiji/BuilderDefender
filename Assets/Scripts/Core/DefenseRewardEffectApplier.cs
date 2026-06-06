using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 防御塔奖励效果应用器，负责把卡牌效果配置写入防御塔奖励状态。
/// </summary>
public static class DefenseRewardEffectApplier
{
    // 批量应用奖励效果配置。
    public static void ApplyEffects(IReadOnlyList<RewardEffectConfig> effectConfigList, DefenseRewardState defenseRewardState)
    {
        if (effectConfigList == null || defenseRewardState == null)
        {
            return;
        }

        foreach (RewardEffectConfig effectConfig in effectConfigList)
        {
            ApplyEffect(effectConfig, defenseRewardState);
        }
    }

    // 根据效果类型累加对应的防御塔奖励值。
    private static void ApplyEffect(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        if (effectConfig == null)
        {
            return;
        }

        switch (effectConfig.EffectType)
        {
            case RewardEffectType.DefenseAttackDamageMultiplier:
                defenseRewardState.AddAttackDamageBonus(GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseAttackSpeedMultiplier:
                defenseRewardState.AddAttackSpeedBonus(GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseDetectRadiusMultiplier:
                defenseRewardState.AddDetectRadiusBonus(GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseMaxHealthMultiplier:
                defenseRewardState.AddMaxHealthBonus(GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseBuildCostMultiplier:
                defenseRewardState.AddBuildCostBonus(GetValue(effectConfig));
                break;

            case RewardEffectType.DefenseArmorIgnorePercent:
                defenseRewardState.AddArmorIgnorePercent(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ArmorIgnorePercent, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseExtraArrowEveryAttackCount:
                AddExtraAttackRule(effectConfig, defenseRewardState);
                break;

            case RewardEffectType.DefenseDamageTakenMultiplier:
                defenseRewardState.AddDamageTakenBonus(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DamageTakenMultiplier, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseWaveEndHealPercent:
                defenseRewardState.AddWaveEndHealPercent(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.WaveEndHealPercent, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseKillCountAutoUpgrade:
                AddKillCountAutoUpgrade(effectConfig, defenseRewardState);
                break;

            case RewardEffectType.DefenseAttackSpeedOverloadMultiplier:
                defenseRewardState.AddAttackSpeedOverloadBonus(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.AttackSpeedMultiplier, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseAttackHealthCost:
                AddAttackHealthCostRule(effectConfig, defenseRewardState);
                break;

            case RewardEffectType.DefenseAttackDamagePerThreeStarTower:
                defenseRewardState.AddAttackDamagePerThreeStarTower(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DamageBonusPerThreeStarTower, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseDoubleDamageChance:
                defenseRewardState.AddDoubleDamageChance(RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.DoubleDamageChance, GetValue(effectConfig)));
                break;

            case RewardEffectType.DefenseLinkedAttackSpeedMultiplier:
                AddLinkedAttackSpeed(effectConfig, defenseRewardState);
                break;

            case RewardEffectType.DefenseRandomTowerMaxStar:
                DefenseRewardImmediateEffectApplier.UpgradeRandomTowerToMaxStar();
                break;

            case RewardEffectType.DefenseNewTowerInitialStarBonus:
                defenseRewardState.AddNewTowerInitialStarBonus(RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.InitialStarBonus, Mathf.RoundToInt(GetValue(effectConfig))));
                break;

            case RewardEffectType.DefenseFinalDefenseAttackDamageMultiplier:
                AddFinalDefense(effectConfig, defenseRewardState);
                break;

            case RewardEffectType.DefenseThreeStarExplosiveArrow:
                AddThreeStarExplosiveArrow(effectConfig, defenseRewardState);
                break;
        }
    }

    // 添加一条额外攻击规则。
    private static void AddExtraAttackRule(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.TriggerAttackCount, 0, true);
        int extraAttackCount = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.ExtraAttackCount, 1);
        defenseRewardState.AddExtraAttackRule(triggerAttackCount, extraAttackCount);
    }

    // 添加攻击损失生命规则。
    private static void AddAttackHealthCostRule(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        int triggerAttackCount = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.TriggerAttackCount, 1);
        int healthCost = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.AttackHealthCost, Mathf.RoundToInt(GetValue(effectConfig)));
        defenseRewardState.AddAttackHealthCostRule(triggerAttackCount, healthCost);
    }

    // 应用击杀自动升星阈值。
    private static void AddKillCountAutoUpgrade(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        int killCountToUpgrade = RewardEffectParameterReader.GetInt(effectConfig, RewardEffectParameterKey.KillCountToUpgrade, 0, true);
        defenseRewardState.AddKillCountAutoUpgrade(killCountToUpgrade);
    }

    // 应用防线联动攻速奖励。
    private static void AddLinkedAttackSpeed(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        float attackSpeedBonus = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.AttackSpeedMultiplier, GetValue(effectConfig));
        float linkRadius = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.LinkRadius, 0f, true);
        defenseRewardState.AddLinkedAttackSpeed(attackSpeedBonus, linkRadius);
    }

    // 应用最终防线奖励配置。
    private static void AddFinalDefense(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        float threshold = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.HomeHealthThreshold, 0f, true);
        defenseRewardState.AddFinalDefense(GetValue(effectConfig), threshold);
    }

    // 应用三星爆裂箭奖励配置。
    private static void AddThreeStarExplosiveArrow(RewardEffectConfig effectConfig, DefenseRewardState defenseRewardState)
    {
        float explosionRadius = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ExplosionRadius, 0f, true);
        float explosionDamageMultiplier = RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.ExplosionDamageMultiplier, 0f, true);
        defenseRewardState.AddThreeStarExplosiveArrow(explosionRadius, explosionDamageMultiplier);
    }

    // 读取旧版主数值参数。
    private static float GetValue(RewardEffectConfig effectConfig)
    {
        return RewardEffectParameterReader.GetFloat(effectConfig, RewardEffectParameterKey.Value, effectConfig.LegacyValue);
    }
}
