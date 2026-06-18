/// <summary>
/// 资源奖励运行时触发接口，预留给后续资源类奖励触发点。
/// </summary>
public interface IResourceRewardTrigger
{
    bool ShouldRegisterRuntimeEffect { get; }
}
