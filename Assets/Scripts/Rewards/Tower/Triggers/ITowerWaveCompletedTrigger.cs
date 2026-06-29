/// <summary>
/// 防御塔波次完成奖励能力，处理每次波次结算后的规则。
/// </summary>
public interface ITowerWaveCompletedTrigger : ITowerRuntimeReward
{
    // 处理一次波次完成结算。
    void OnWaveCompleted(TowerRewardRuntimeState runtimeState);
}
