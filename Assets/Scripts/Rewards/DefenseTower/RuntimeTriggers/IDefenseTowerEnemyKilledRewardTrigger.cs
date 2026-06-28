/// <summary>
/// 防御塔击杀奖励能力，处理由防御塔造成的敌人击杀。
/// </summary>
public interface IDefenseTowerEnemyKilledRewardTrigger : IDefenseTowerRuntimeReward
{
    // 处理一次由防御塔造成的敌人击杀。
    void OnEnemyKilled(
        DefenseTowerRewardRuntimeState runtimeState,
        DefenseTowerEnemyKilledContext context);
}
