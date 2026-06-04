using TMPro;
using UnityEngine;

/// <summary>
/// 奖励卡牌视图组件，负责把奖励卡牌数据刷新到 UI 显示节点。
/// </summary>
public class RewardCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text descriptionText;

    // 根据奖励卡牌数据刷新整张卡牌视图。
    public void SetCard(RewardCardSo rewardCard)
    {
        RefreshCardName(rewardCard);
        RefreshDescription(rewardCard);
    }

    // 刷新奖励卡牌名称文本。
    private void RefreshCardName(RewardCardSo rewardCard)
    {
        if (!cardNameText)
        {
            return;
        }

        cardNameText.richText = true;
        cardNameText.text = rewardCard ? rewardCard.CardName : string.Empty;
    }

    // 刷新奖励卡牌描述文本。
    private void RefreshDescription(RewardCardSo rewardCard)
    {
        if (!descriptionText)
        {
            return;
        }

        descriptionText.richText = true;
        descriptionText.text = RewardCardDescriptionFormatter.BuildDescriptionText(rewardCard);
    }
}
