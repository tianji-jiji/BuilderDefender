using UnityEngine;

/// <summary>
/// Unity 随机数桥接实现，为奖励抽取服务提供整数随机结果。
/// </summary>
public sealed class UnityRewardRandom : IRewardRandom
{
    // 返回指定左闭右开区间内的 Unity 随机整数。
    public int Range(int minInclusive, int maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
}
