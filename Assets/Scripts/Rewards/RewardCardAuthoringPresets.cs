using System;

/// <summary>
/// 奖励卡牌编辑配置预设，负责提供 Inspector 需要的卡牌基础枚举显示名。
/// </summary>
public static class RewardCardAuthoringPresets
{
    private static readonly Lazy<string[]> RarityDisplayNameArrayLazy = new(BuildRarityDisplayNameArray);
    private static readonly Lazy<string[]> CategoryDisplayNameArrayLazy = new(BuildCategoryDisplayNameArray);

    public static string[] RarityDisplayNameArray => RarityDisplayNameArrayLazy.Value;
    public static string[] CategoryDisplayNameArray => CategoryDisplayNameArrayLazy.Value;

    // 获取稀有度中文名。
    private static string GetRarityDisplayName(RewardCardRarity rarity)
    {
        switch (rarity)
        {
            case RewardCardRarity.Normal:
                return "普通";
            case RewardCardRarity.Rare:
                return "稀有";
            case RewardCardRarity.Epic:
                return "史诗";
            case RewardCardRarity.Legendary:
                return "传说";
            default:
                return rarity.ToString();
        }
    }

    // 获取卡牌分类中文名。
    private static string GetCategoryDisplayName(RewardCardCategory category)
    {
        switch (category)
        {
            case RewardCardCategory.Defense:
                return "防御塔";
            case RewardCardCategory.Resources:
                return "资源";
            case RewardCardCategory.Home:
                return "基地";
            case RewardCardCategory.Risk:
                return "风险";
            default:
                return category.ToString();
        }
    }

    // 构建稀有度下拉框中文名。
    private static string[] BuildRarityDisplayNameArray()
    {
        Array rarityArray = Enum.GetValues(typeof(RewardCardRarity));
        string[] displayNameArray = new string[rarityArray.Length];
        for (int i = 0; i < rarityArray.Length; i++)
        {
            displayNameArray[i] = GetRarityDisplayName((RewardCardRarity)rarityArray.GetValue(i));
        }

        return displayNameArray;
    }

    // 构建卡牌分类下拉框中文名。
    private static string[] BuildCategoryDisplayNameArray()
    {
        Array categoryArray = Enum.GetValues(typeof(RewardCardCategory));
        string[] displayNameArray = new string[categoryArray.Length];
        for (int i = 0; i < categoryArray.Length; i++)
        {
            displayNameArray[i] = GetCategoryDisplayName((RewardCardCategory)categoryArray.GetValue(i));
        }

        return displayNameArray;
    }
}
