/// <summary>
/// Tower 奖励运行时，协调奖励状态、触发器和场景入口。
/// </summary>
public sealed class TowerRewardRuntime
{
    public TowerRewardState State { get; }
    public TowerRewardTriggerDispatcher TriggerDispatcher { get; }
    public ITowerRewardWorld World { get; }

    // 创建使用指定场景入口的 Tower 奖励运行时。
    public TowerRewardRuntime(ITowerRewardWorld world)
    {
        World = world;
        State = new TowerRewardState();
        TriggerDispatcher = new TowerRewardTriggerDispatcher(world);
    }

    // 计算 Tower 建筑经过奖励修正后的建造消耗。
    public int GetAdjustedBuildCostAmount(BuildingSo buildingSo, ResourceCost resourceCost)
    {
        return TowerBuildCostCalculator.GetAdjustedAmount(buildingSo, resourceCost, State);
    }

    // 创建当前 Tower 体系战力快照。
    public TowerPowerSnapshot CreatePowerSnapshot()
    {
        return TowerPowerCalculator.CreateSnapshot(State);
    }

    // 获取最终防线激活后的攻击力倍率。
    public float GetFinalDefenseTowerAttackDamageMultiplier()
    {
        float homeHealthNormalized = World?.HomeHealthNormalized ?? 1f;
        return TowerPowerCalculator.GetFinalDefenseMultiplier(State, homeHealthNormalized);
    }

    // 波次结束时转发给 Tower 奖励触发器。
    public void OnWaveCompleted()
    {
        TriggerDispatcher.OnWaveCompleted();
    }
}
