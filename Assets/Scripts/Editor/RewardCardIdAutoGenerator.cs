using UnityEditor;

/// <summary>
/// 奖励卡牌 ID 自动生成器，负责在编辑器中手动刷新所有卡牌资产的稳定 ID。
/// </summary>
public static class RewardCardIdAutoGenerator
{
    // 扫描所有奖励卡牌资产并刷新自动生成的 ID。
    [MenuItem("Tools/RewardCard/Refresh All Card Ids")]
    public static void RefreshAllRewardCardIds()
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
