using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励详情面板，负责展示玩家已获得卡牌列表和当前累计加成摘要。
/// </summary>
public class RewardSummaryPanel : MonoBehaviour
{
    private const string EMPTY_BONUS_TEXT = "当前暂无已获得加成";
    private const int MIN_BONUS_LINES_PER_COLUMN = 1;

    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Transform recordRoot;
    [SerializeField] private RewardSummaryRecordItem recordItemPrefab;
    [SerializeField] private TMP_Text totalBonusText;
    [SerializeField] private Button closeButton;
    [SerializeField] private int maxBonusLinesPerColumn = 6;
    [SerializeField] private float bonusColumnSpacing = 24f;

    private readonly List<RewardSummaryRecordItem> _recordItemList = new();
    private readonly List<TMP_Text> _bonusColumnTextList = new();
    private Vector2 _bonusBaseAnchoredPosition;
    private Vector2 _bonusBaseSize;
    private bool _hasBonusBaseLayout;

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

        CacheBonusTextColumn();
    }

    // 刷新已获得奖励记录列表。
    private void RefreshRecordList()
    {
        ClearRecordItems();

        if (!RewardCardAcquiredHistory.Instance || !recordRoot || !recordItemPrefab)
        {
            return;
        }

        IReadOnlyList<RewardCardAcquiredRecord> rewardRecordList = RewardCardAcquiredHistory.Instance.GetRecordList();
        foreach (RewardCardAcquiredRecord rewardRecord in rewardRecordList)
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

        string summaryText = RewardRuntimeCoordinator.Instance
            ? RewardRuntimeCoordinator.Instance.DefenseTowerRewards.BuildSummaryText()
            : EMPTY_BONUS_TEXT;

        RefreshTotalBonusColumns(summaryText);
    }

    // 缓存总加成文本列的初始引用。
    private void CacheBonusTextColumn()
    {
        if (!totalBonusText || _bonusColumnTextList.Contains(totalBonusText))
        {
            return;
        }

        _bonusColumnTextList.Add(totalBonusText);
        CacheBonusBaseLayout();
    }

    // 缓存总加成文本原始显示区域。
    private void CacheBonusBaseLayout()
    {
        if (!totalBonusText || _hasBonusBaseLayout)
        {
            return;
        }

        RectTransform totalBonusRect = totalBonusText.rectTransform;
        _bonusBaseAnchoredPosition = totalBonusRect.anchoredPosition;
        _bonusBaseSize = totalBonusRect.sizeDelta;
        _hasBonusBaseLayout = true;
    }

    // 按最大行数把总加成摘要刷新为多列文本。
    private void RefreshTotalBonusColumns(string summaryText)
    {
        string[] summaryLineList = SplitSummaryLines(summaryText);
        int linesPerColumn = Mathf.Max(MIN_BONUS_LINES_PER_COLUMN, maxBonusLinesPerColumn);
        int columnCount = Mathf.Max(1, Mathf.CeilToInt((float)summaryLineList.Length / linesPerColumn));
        EnsureBonusColumnTextCount(columnCount);
        LayoutBonusColumns(columnCount);

        for (int i = 0; i < _bonusColumnTextList.Count; i++)
        {
            TMP_Text columnText = _bonusColumnTextList[i];
            if (!columnText)
            {
                continue;
            }

            bool isActiveColumn = i < columnCount;
            columnText.gameObject.SetActive(isActiveColumn);
            if (!isActiveColumn)
            {
                columnText.text = string.Empty;
                continue;
            }

            columnText.richText = true;
            columnText.text = BuildColumnText(summaryLineList, i * linesPerColumn, linesPerColumn);
        }
    }

    // 将摘要文本拆分为可分栏的行列表。
    private string[] SplitSummaryLines(string summaryText)
    {
        if (string.IsNullOrWhiteSpace(summaryText))
        {
            return new[] { EMPTY_BONUS_TEXT };
        }

        return summaryText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }

    // 确保总加成文本列数量足够。
    private void EnsureBonusColumnTextCount(int columnCount)
    {
        CacheBonusTextColumn();

        while (_bonusColumnTextList.Count < columnCount)
        {
            TMP_Text bonusColumnText = Instantiate(totalBonusText, totalBonusText.transform.parent);
            bonusColumnText.name = $"{totalBonusText.name}_Column{_bonusColumnTextList.Count + 1}";
            _bonusColumnTextList.Add(bonusColumnText);
        }
    }

    // 根据列数量重新摆放总加成文本列。
    private void LayoutBonusColumns(int columnCount)
    {
        CacheBonusBaseLayout();

        RectTransform sourceRect = totalBonusText.rectTransform;
        float totalWidth = _hasBonusBaseLayout ? _bonusBaseSize.x : sourceRect.sizeDelta.x;
        float totalHeight = _hasBonusBaseLayout ? _bonusBaseSize.y : sourceRect.sizeDelta.y;
        float columnWidth = (totalWidth - bonusColumnSpacing * (columnCount - 1)) / columnCount;
        float startX = -totalWidth * 0.5f + columnWidth * 0.5f;

        for (int i = 0; i < columnCount; i++)
        {
            TMP_Text columnText = _bonusColumnTextList[i];
            if (!columnText)
            {
                continue;
            }

            RectTransform columnRect = columnText.rectTransform;
            columnRect.anchorMin = sourceRect.anchorMin;
            columnRect.anchorMax = sourceRect.anchorMax;
            columnRect.pivot = sourceRect.pivot;
            columnRect.anchoredPosition = _bonusBaseAnchoredPosition + Vector2.right * (startX + i * (columnWidth + bonusColumnSpacing));
            columnRect.sizeDelta = new Vector2(columnWidth, totalHeight);
        }
    }

    // 构建单列总加成文本。
    private string BuildColumnText(IReadOnlyList<string> summaryLineList, int startIndex, int maxLineCount)
    {
        StringBuilder columnBuilder = new();
        int endIndex = Mathf.Min(summaryLineList.Count, startIndex + maxLineCount);

        for (int i = startIndex; i < endIndex; i++)
        {
            if (columnBuilder.Length > 0)
            {
                columnBuilder.AppendLine();
            }

            columnBuilder.Append(summaryLineList[i]);
        }

        return columnBuilder.ToString();
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
