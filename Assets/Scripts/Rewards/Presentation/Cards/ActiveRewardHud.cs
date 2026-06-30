using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 已获得奖励 HUD 入口，负责展示当前加成数量并打开奖励详情面板。
/// </summary>
public class ActiveRewardHud : MonoBehaviour
{
    private const string COUNT_FORMAT = "加成 x{0}";
    private const string LATEST_CARD_FORMAT = "最近：{0}";
    private const string EMPTY_COUNT_TEXT = "加成 x0";
    private const string EMPTY_LATEST_CARD_TEXT = "最近：无";

    [SerializeField] private Button openSummaryButton;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text latestCardNameText;
    [SerializeField] private RewardSummaryPanel summaryPanel;
    [SerializeField] private CanvasGroup highlightCanvasGroup;

    private RewardRuntimeCoordinator _coordinator;

    // 缓存 HUD 依赖并刷新初始显示。
    private void Awake()
    {
        CacheReferences();
        RefreshEmptyState();
        HideHighlight();
    }

    // 订阅按钮和奖励生效事件。
    private void OnEnable()
    {
        if (openSummaryButton)
        {
            openSummaryButton.onClick.AddListener(OpenSummaryPanel);
        }

        _coordinator = RewardRuntimeCoordinator.Instance;
        if (_coordinator)
        {
            _coordinator.OnRewardApplied += HandleRewardApplied;
        }
    }

    // 取消订阅按钮和奖励生效事件。
    private void OnDisable()
    {
        if (openSummaryButton)
        {
            openSummaryButton.onClick.RemoveListener(OpenSummaryPanel);
        }

        if (_coordinator)
        {
            _coordinator.OnRewardApplied -= HandleRewardApplied;
            _coordinator = null;
        }
    }

    // 处理奖励生效事件并刷新 HUD。
    private void HandleRewardApplied(RewardAppliedContext context)
    {
        if (context?.AppliedRecord?.RewardCard == null)
        {
            return;
        }

        RefreshHud(context);
        ShowHighlight();
    }

    // 缓存 HUD 依赖的组件引用。
    private void CacheReferences()
    {
        if (!openSummaryButton)
        {
            TryGetComponent(out openSummaryButton);
        }
    }

    // 刷新没有任何奖励时的默认状态。
    private void RefreshEmptyState()
    {
        if (countText)
        {
            countText.text = EMPTY_COUNT_TEXT;
        }

        if (latestCardNameText)
        {
            latestCardNameText.text = EMPTY_LATEST_CARD_TEXT;
        }
    }

    // 根据奖励上下文刷新 HUD 文本。
    private void RefreshHud(RewardAppliedContext context)
    {
        if (countText)
        {
            countText.text = string.Format(COUNT_FORMAT, context.TotalCardCount);
        }

        if (latestCardNameText)
        {
            latestCardNameText.text = string.Format(
                LATEST_CARD_FORMAT,
                context.AppliedRecord.RewardCard.CardName);
        }
    }

    // 打开奖励详情面板。
    private void OpenSummaryPanel()
    {
        if (summaryPanel)
        {
            summaryPanel.Open();
        }
    }

    // 显示 HUD 高亮提示。
    private void ShowHighlight()
    {
        if (!highlightCanvasGroup)
        {
            return;
        }

        highlightCanvasGroup.alpha = 1f;
        highlightCanvasGroup.interactable = false;
        highlightCanvasGroup.blocksRaycasts = false;
    }

    // 隐藏 HUD 高亮提示。
    private void HideHighlight()
    {
        if (!highlightCanvasGroup)
        {
            return;
        }

        highlightCanvasGroup.alpha = 0f;
        highlightCanvasGroup.interactable = false;
        highlightCanvasGroup.blocksRaycasts = false;
    }
}
