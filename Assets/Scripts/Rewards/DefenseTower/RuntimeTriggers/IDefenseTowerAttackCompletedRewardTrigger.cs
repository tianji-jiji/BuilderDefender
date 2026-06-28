/// <summary>
/// 防御塔攻击完成奖励能力，处理一次成功主动攻击后的规则。
/// </summary>
public interface IDefenseTowerAttackCompletedRewardTrigger : IDefenseTowerRuntimeReward
{
    // 处理一次成功完成的防御塔主动攻击。
    void OnAttackCompleted(
        DefenseTowerRewardRuntimeState runtimeState,
        DefenseTowerAttackCompletedContext context);
}
