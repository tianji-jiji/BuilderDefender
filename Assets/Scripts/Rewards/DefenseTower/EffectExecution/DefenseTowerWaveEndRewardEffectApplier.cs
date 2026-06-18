/// <summary>
/// 防御塔波次结算奖励执行器，处理“每波结束”的效果，比如所有防御塔回血。
/// </summary>
public static class DefenseTowerWaveEndRewardEffectApplier
{
    // 按配置比例治疗所有防御塔。
    public static void ApplyWaveEndHeal(float healPercent)
    {
        DefenseTowerRegistry.HealAllDefenseTowers(healPercent);
    }
}
