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
        foreach (var t in upgradeLevels)
        {
            if (t.StarLevel == starLevel)
            {
                return t;
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
    [SerializeField] private float maxHealthMultiplier = 1f;
    [SerializeField] private float attackDamageMultiplier = 1f;
    [SerializeField] private float attackIntervalMultiplier = 1f;
    [SerializeField] private float detectRadiusMultiplier = 1f;
    [SerializeField] private bool unlockSuperArrow;

    public int StarLevel => starLevel;
    public IReadOnlyList<ResourceCost> UpgradeCost => upgradeCost;
    public float MaxHealthMultiplier => Mathf.Max(0.01f, maxHealthMultiplier);
    public float AttackDamageMultiplier => Mathf.Max(0.01f, attackDamageMultiplier);
    public float AttackIntervalMultiplier => Mathf.Max(0.01f, attackIntervalMultiplier);
    public float DetectRadiusMultiplier => Mathf.Max(0.01f, detectRadiusMultiplier);
    public bool UnlockSuperArrow => unlockSuperArrow;
}
