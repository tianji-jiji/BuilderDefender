using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励详情记录项，负责把单条已获得卡牌记录刷新到列表 UI。
/// </summary>
public class RewardSummaryRecordItem : MonoBehaviour
{
    private const string STACK_FORMAT = "x{0}";
    private const string WAVE_FORMAT = "第 {0} 波";

    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private Image rarityBorderImage;
    [SerializeField] private Color normalRarityColor = Color.white;
    [SerializeField] private Color rareRarityColor = new(0.3f, 0.65f, 1f, 1f);
    [SerializeField] private Color epicRarityColor = new(0.75f, 0.35f, 1f, 1f);
    [SerializeField] private Color legendaryRarityColor = new(1f, 0.7f, 0.2f, 1f);
    [SerializeField] private Color mythicRarityColor = new(1f, 0.18f, 0.18f, 1f);

    // 刷新单条奖励记录的 UI 文本。
    public void SetRecord(RewardCardAcquisitionRecord rewardRecord)
    {
        if (rewardRecord == null)
        {
            ClearText();
            return;
        }

        RefreshText(rewardRecord);
        RefreshRarityBorder(rewardRecord.Rarity);
    }

    // 刷新有效奖励记录的所有文本。
    private void RefreshText(RewardCardAcquisitionRecord rewardRecord)
    {
        if (cardNameText)
        {
            cardNameText.text = rewardRecord.CardName;
        }

        if (stackText)
        {
            stackText.text = string.Format(STACK_FORMAT, rewardRecord.StackCount);
        }

        if (rarityText)
        {
            rarityText.text = rewardRecord.Rarity.ToString();
        }

        if (descriptionText)
        {
            descriptionText.richText = true;
            descriptionText.text = rewardRecord.DescriptionText;
        }

        if (waveText)
        {
            waveText.text = string.Format(WAVE_FORMAT, rewardRecord.AcquiredWaveIndex);
        }
    }

    // 清空当前记录项显示文本。
    private void ClearText()
    {
        if (cardNameText)
        {
            cardNameText.text = string.Empty;
        }

        if (stackText)
        {
            stackText.text = string.Empty;
        }

        if (rarityText)
        {
            rarityText.text = string.Empty;
        }

        if (descriptionText)
        {
            descriptionText.text = string.Empty;
        }

        if (waveText)
        {
            waveText.text = string.Empty;
        }

        RefreshRarityBorder(RewardCardRarity.Normal);
    }

    // 根据卡牌稀有度刷新记录项边框颜色。
    private void RefreshRarityBorder(RewardCardRarity rarity)
    {
        if (!rarityBorderImage)
        {
            return;
        }

        rarityBorderImage.color = GetRarityColor(rarity);
    }

    // 获取卡牌稀有度对应的边框颜色。
    private Color GetRarityColor(RewardCardRarity rarity)
    {
        switch (rarity)
        {
            case RewardCardRarity.Rare:
                return rareRarityColor;
            case RewardCardRarity.Epic:
                return epicRarityColor;
            case RewardCardRarity.Legendary:
                return legendaryRarityColor;
            case RewardCardRarity.Mythic:
                return mythicRarityColor;
            default:
                return normalRarityColor;
        }
    }
}
