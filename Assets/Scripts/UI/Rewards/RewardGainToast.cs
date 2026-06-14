using System.Collections;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励获得提示，负责在玩家选卡后展示本次获得的卡牌名称、描述和即时反馈。
/// </summary>
public class RewardGainToast : MonoBehaviour
{
    private const string CARD_NAME_PREFIX = "获得：";

    [SerializeField] private CanvasGroup toastCanvasGroup;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image rarityFrameImage;
    [SerializeField] private MMF_Player showFeedbacks;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float displayDuration = 2.4f;
    [SerializeField] private Color normalRarityColor = Color.white;
    [SerializeField] private Color rareRarityColor = new(0.3f, 0.65f, 1f, 1f);
    [SerializeField] private Color epicRarityColor = new(0.75f, 0.35f, 1f, 1f);
    [SerializeField] private Color legendaryRarityColor = new(1f, 0.7f, 0.2f, 1f);

    private Coroutine _hideCoroutine;

    // 缓存奖励提示依赖并隐藏初始状态。
    private void Awake()
    {
        CacheReferences();
        HideImmediate();
    }

    // 订阅奖励生效事件。
    private void OnEnable()
    {
        RewardCardSelectionHistory.OnRewardCardApplied += HandleRewardCardApplied;
    }

    // 取消订阅奖励生效事件。
    private void OnDisable()
    {
        RewardCardSelectionHistory.OnRewardCardApplied -= HandleRewardCardApplied;
        StopHideCoroutine();
    }

    // 处理奖励生效事件并刷新提示内容。
    private void HandleRewardCardApplied(RewardCardAppliedContext context)
    {
        if (context == null || context.SelectionRecord == null)
        {
            return;
        }

        RefreshText(context.SelectionRecord);
        RefreshRarityColor(context.SelectionRecord.Rarity);
        PlayFeedback();
        RestartAutoHide();
    }

    // 缓存奖励提示依赖的组件引用。
    private void CacheReferences()
    {
        if (!toastCanvasGroup)
        {
            TryGetComponent(out toastCanvasGroup);
        }

        if (!showFeedbacks)
        {
            TryGetComponent(out showFeedbacks);
        }

        if (!audioSource)
        {
            TryGetComponent(out audioSource);
        }
    }

    // 刷新本次获得奖励的文本。
    private void RefreshText(RewardCardSelectionRecord selectionRecord)
    {
        if (cardNameText)
        {
            cardNameText.richText = true;
            cardNameText.text = $"{CARD_NAME_PREFIX}{selectionRecord.CardName}";
        }

        if (descriptionText)
        {
            descriptionText.richText = true;
            descriptionText.text = selectionRecord.DescriptionText;
        }
    }

    // 根据奖励稀有度刷新边框颜色。
    private void RefreshRarityColor(RewardCardRarity rarity)
    {
        if (!rarityFrameImage)
        {
            return;
        }

        rarityFrameImage.color = GetRarityColor(rarity);
    }

    // 播放奖励获得反馈。
    private void PlayFeedback()
    {
        ShowImmediate();

        if (showFeedbacks)
        {
            showFeedbacks.PlayFeedbacks();
        }

        if (audioSource)
        {
            audioSource.Play();
        }
    }

    // 重新开始自动隐藏计时。
    private void RestartAutoHide()
    {
        StopHideCoroutine();
        _hideCoroutine = StartCoroutine(AutoHideCoroutine());
    }

    // 停止当前自动隐藏计时。
    private void StopHideCoroutine()
    {
        if (_hideCoroutine == null)
        {
            return;
        }

        StopCoroutine(_hideCoroutine);
        _hideCoroutine = null;
    }

    // 等待一段真实时间后隐藏奖励提示。
    private IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, displayDuration));
        HideImmediate();
        _hideCoroutine = null;
    }

    // 立即显示奖励提示。
    private void ShowImmediate()
    {
        if (!toastCanvasGroup)
        {
            return;
        }

        toastCanvasGroup.alpha = 1f;
        toastCanvasGroup.interactable = false;
        toastCanvasGroup.blocksRaycasts = false;
    }

    // 立即隐藏奖励提示。
    private void HideImmediate()
    {
        if (!toastCanvasGroup)
        {
            return;
        }

        toastCanvasGroup.alpha = 0f;
        toastCanvasGroup.interactable = false;
        toastCanvasGroup.blocksRaycasts = false;
    }

    // 获取奖励稀有度对应的显示颜色。
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
            default:
                return normalRarityColor;
        }
    }
}
