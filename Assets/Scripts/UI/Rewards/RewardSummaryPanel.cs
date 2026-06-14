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

        if (!RewardCardSelectionHistory.Instance || !recordRoot || !recordItemPrefab)
        {
            return;
        }

        IReadOnlyList<RewardCardSelectionRecord> rewardRecordList = RewardCardSelectionHistory.Instance.GetRecordList();
        foreach (RewardCardSelectionRecord rewardRecord in rewardRecordList)
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
            ? RewardBonusManager.Instance.BuildDefenseTowerRewardSummaryText()
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
