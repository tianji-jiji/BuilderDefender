/// <summary>
/// 防御塔箭矢修改能力，在箭矢发射前向上下文添加运行时奖励能力。
/// </summary>
public interface ITowerArrowModifier : ITowerRuntimeReward
{
    // 修改单支待发射箭矢的奖励能力配置。
    void ModifyArrow(TowerEffectState runtimeState, TowerArrowContext context);
}
