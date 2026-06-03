using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/BuildingUpgradeConfigSo")]
public class BuildingUpgradeConfigSo : ScriptableObject
{
    [SerializeField] private List<BuildingUpgradeLevel> upgradeLevels = new List<BuildingUpgradeLevel>();

    public int MaxStarLevel => upgradeLevels.Count + 1;

    // 获取指定星级的升级配置。
    public BuildingUpgradeLevel GetUpgradeLevel(int starLevel)
    {
        for (int i = 0; i < upgradeLevels.Count; i++)
        {
            if (upgradeLevels[i].StarLevel == starLevel)
            {
                return upgradeLevels[i];
            }
        }

        return null;
    }
}

[System.Serializable]
public class BuildingUpgradeLevel
{
    [SerializeField] private int starLevel = 2;
    [SerializeField] private List<ResourceCost> upgradeCost = new List<ResourceCost>();

    public int StarLevel => starLevel;
    public IReadOnlyList<ResourceCost> UpgradeCost => upgradeCost;
}
