using UnityEditor;

/// <summary>
/// 奖励卡牌 ID 自动生成器，负责在编辑器中统一刷新所有卡牌资产的稳定 ID。
/// </summary>
[InitializeOnLoad]
public static class RewardCardIdAutoGenerator
{
    // 注册编辑器延迟刷新，避免脚本重载期间直接访问资产数据库。
    static RewardCardIdAutoGenerator()
    {
        EditorApplication.delayCall += RefreshAllRewardCardIds;
    }

    // 扫描所有奖励卡牌资产并刷新自动生成的 ID。
    private static void RefreshAllRewardCardIds()
    {
        string[] rewardCardGuidArray = AssetDatabase.FindAssets("t:RewardCardSo");
        bool hasChanged = false;

        foreach (string rewardCardGuid in rewardCardGuidArray)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(rewardCardGuid);
            RewardCardSo rewardCard = AssetDatabase.LoadAssetAtPath<RewardCardSo>(assetPath);
            if (!rewardCard)
            {
                continue;
            }

            hasChanged |= rewardCard.RefreshGeneratedCardIdInEditor();
        }

        if (hasChanged)
        {
            AssetDatabase.SaveAssets();
        }
    }
}
