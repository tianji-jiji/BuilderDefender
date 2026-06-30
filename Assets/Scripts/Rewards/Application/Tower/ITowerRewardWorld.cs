/// <summary>
/// Tower 奖励访问游戏场景所需的查询与命令接口。
/// </summary>
public interface ITowerRewardWorld
{
    float HomeHealthNormalized { get; }

    // 获取当前三星 Tower 数量。
    int GetThreeStarTowerCount();

    // 判断指定 Tower 附近是否存在另一座 Tower。
    bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius);

    // 随机选择一座可升级 Tower 并升到满星。
    bool UpgradeRandomTowerToMaxStar();

    // 将指定 Tower 免费提升一级。
    bool UpgradeTowerOneLevel(TowerCombatSystem sourceTowerCombatSystem);

    // 按最大生命比例治疗全部 Tower。
    void HealAllTowers(float healPercent);
}
