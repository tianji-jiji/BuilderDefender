/// <summary>
/// 基地奖励运行时触发接口，预留给后续基地类奖励触发点。
/// </summary>
public interface IHomeRewardTrigger
{
    bool ShouldRegisterRuntimeEffect { get; }
}
