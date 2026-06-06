using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励详情面板，负责展示玩家已获得卡牌列表和当前累计加成摘要。
/// </summary>
public class RewardSummaryPanel : MonoBehaviour
{
    private const string EMPTY_BONUS_TEXT = "当前暂无已获得加成";

    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Transform recordRoot;
    [SerializeField] private RewardSummaryRecordItem recordItemPrefab;
    [SerializeField] private TMP_Text totalBonusText;
    [SerializeField] private Button closeButton;

    private readonly List<RewardSummaryRecordItem> _recordItemList = new();

    // 缓存详情面板依赖并隐藏初始状态。
    private void Awake()
    {
        CacheReferences();
        HideImmediate();
    }

    // 订阅关闭按钮事件。
    private void OnEnable()
    {
        if (closeButton)
        {
            closeButton.onClick.AddListener(Close);
        }
    }

    // 取消订阅关闭按钮事件。
    private void OnDisable()
    {
        if (closeButton)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    // 打开奖励详情面板并刷新列表内容。
    public void Open()
    {
        Refresh();
        ShowImmediate();
    }

    // 关闭奖励详情面板。
    public void Close()
    {
        HideImmediate();
    }

    // 刷新奖励详情面板中的所有显示内容。
    public void Refresh()
    {
        RefreshRecordList();
        RefreshTotalBonusText();
    }

    // 缓存详情面板依赖的组件引用。
    private void CacheReferences()
    {
        if (!panelCanvasGroup)
        {
            TryGetComponent(out panelCanvasGroup);
        }
    }

    // 刷新已获得奖励记录列表。
    private void RefreshRecordList()
    {
        ClearRecordItems();

        if (!RewardSelectionHistory.Instance || !recordRoot || !recordItemPrefab)
        {
            return;
        }

        IReadOnlyList<RewardSelectionRecord> rewardRecordList = RewardSelectionHistory.Instance.GetRecordList();
        foreach (RewardSelectionRecord rewardRecord in rewardRecordList)
        {
            RewardSummaryRecordItem recordItem = Instantiate(recordItemPrefab, recordRoot);
            recordItem.SetRecord(rewardRecord);
            _recordItemList.Add(recordItem);
        }
    }

    // 清理当前已经生成的奖励记录项。
    private void ClearRecordItems()
    {
        foreach (RewardSummaryRecordItem recordItem in _recordItemList)
        {
            if (recordItem)
            {
                Destroy(recordItem.gameObject);
            }
        }

        _recordItemList.Clear();

        if (!recordRoot)
        {
            return;
        }

        for (int i = recordRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(recordRoot.GetChild(i).gameObject);
        }
    }

    // 刷新当前累计加成摘要文本。
    private void RefreshTotalBonusText()
    {
        if (!totalBonusText)
        {
            return;
        }

        totalBonusText.richText = true;
        totalBonusText.text = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.BuildDefenseRewardSummaryText()
            : EMPTY_BONUS_TEXT;
    }

    // 立即显示详情面板。
    private void ShowImmediate()
    {
        if (!panelCanvasGroup)
        {
            return;
        }

        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    // 立即隐藏详情面板。
    private void HideImmediate()
    {
        if (!panelCanvasGroup)
        {
            return;
        }

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }
}

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
