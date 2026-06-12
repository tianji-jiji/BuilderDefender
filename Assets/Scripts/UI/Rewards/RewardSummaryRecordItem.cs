using TMPro;
using UnityEngine;

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

    // 刷新单条奖励记录的 UI 文本。
    public void SetRecord(RewardSelectionRecord rewardRecord)
    {
        if (rewardRecord == null)
        {
            ClearText();
            return;
        }

        RefreshText(rewardRecord);
    }

    // 刷新有效奖励记录的所有文本。
    private void RefreshText(RewardSelectionRecord rewardRecord)
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
    }
}
