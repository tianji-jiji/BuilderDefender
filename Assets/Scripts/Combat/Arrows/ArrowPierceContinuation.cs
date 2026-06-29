/// <summary>
/// 箭矢穿透续飞规则，负责消费额外可穿透敌人的次数。
/// </summary>
public class ArrowPierceContinuation : ArrowHitContinuation
{
    private int _remainingPierceCount;

    // 复制本次发射的可穿透次数。
    public override void Configure(ArrowLaunchData launchData)
    {
        _remainingPierceCount = launchData.PierceCount;
    }

    // 有剩余穿透次数时消费一次并允许继续飞行。
    public override bool ShouldContinue(ArrowHitContext context)
    {
        if (_remainingPierceCount <= 0)
        {
            return false;
        }

        _remainingPierceCount--;
        return true;
    }

    // 清空对象池实例上的剩余穿透次数。
    public override void ResetState()
    {
        _remainingPierceCount = 0;
    }
}
