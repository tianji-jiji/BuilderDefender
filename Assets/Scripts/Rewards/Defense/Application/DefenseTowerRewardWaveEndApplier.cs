/// <summary>
/// 防御塔波次结算奖励执行器，负责处理每波结束时触发的奖励。
/// </summary>
public static class DefenseTowerRewardWaveEndApplier
{
    // 按配置比例治疗所有防御塔。
    public static void ApplyWaveEndHeal(float healPercent)
    {
        DefenseTowerTracker.HealAllDefenseTowers(healPercent);
    }
}
