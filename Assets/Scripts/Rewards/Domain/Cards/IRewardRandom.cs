/// <summary>
/// 奖励抽取随机数接口，用于隔离领域抽取逻辑与具体随机数实现。
/// </summary>
public interface IRewardRandom
{
    // 返回指定左闭右开区间内的随机整数。
    int Range(int minInclusive, int maxExclusive);
}
