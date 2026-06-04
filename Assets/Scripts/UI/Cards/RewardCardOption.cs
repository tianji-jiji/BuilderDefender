using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCardOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string POSITIVE_COLOR_HEX = "#55FF77";
    private const string NEGATIVE_COLOR_HEX = "#FF5A5A";
    private const string NEUTRAL_COLOR_HEX = "#FFFFFF";

    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private MMF_Player pointerEnterFeedbacks;
    [SerializeField] private MMF_Player pointerExitFeedbacks;

    private RewardCardSo _rewardCard;
    private CardRewardController _controller;

    // 缓存卡牌根节点上的按钮引用。
    private void Awake()
    {
        CacheReferences();
    }

    // 注册卡牌点击事件。
    private void OnEnable()
    {
        CacheReferences();

        if (selectButton)
        {
            selectButton.onClick.AddListener(HandleSelectClicked);
        }
    }

    // 取消注册卡牌点击事件。
    private void OnDisable()
    {
        if (selectButton)
        {
            selectButton.onClick.RemoveListener(HandleSelectClicked);
        }
    }

    // 初始化本次实例对应的奖励卡数据和控制器。
    public void Init(RewardCardSo rewardCard, CardRewardController controller)
    {
        _rewardCard = rewardCard;
        _controller = controller;

        RefreshCardView();
    }

    // 处理鼠标进入卡牌区域时的悬浮反馈。
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pointerEnterFeedbacks)
        {
            pointerEnterFeedbacks.PlayFeedbacks();
        }
    }

    // 处理鼠标离开卡牌区域时的恢复反馈。
    public void OnPointerExit(PointerEventData eventData)
    {
        if (pointerExitFeedbacks)
        {
            pointerExitFeedbacks.PlayFeedbacks();
        }
    }

    // 缓存卡牌选项依赖的组件。
    private void CacheReferences()
    {
        if (!selectButton)
        {
            TryGetComponent(out selectButton);
        }
    }

    // 根据奖励数据刷新卡牌显示内容。
    private void RefreshCardView()
    {
        if (descriptionText && _rewardCard)
        {
            descriptionText.richText = true;
            descriptionText.text = BuildDescriptionText(_rewardCard);
        }
    }

    // 构建奖励卡牌描述文本。
    private string BuildDescriptionText(RewardCardSo rewardCard)
    {
        if (rewardCard.EffectConfigList == null || rewardCard.EffectConfigList.Count <= 0)
        {
            return string.Empty;
        }

        string description = string.Empty;
        for (int i = 0; i < rewardCard.EffectConfigList.Count; i++)
        {
            RewardEffectConfig effectConfig = rewardCard.EffectConfigList[i];
            if (effectConfig == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(description))
            {
                description += "\n";
            }

            description += BuildEffectDescription(effectConfig);
        }

        return description;
    }

    // 构建单个奖励效果的描述文本。
    private string BuildEffectDescription(RewardEffectConfig effectConfig)
    {
        string valueText = BuildColoredValueText(effectConfig);
        switch (effectConfig.EffectType)
        {
            case RewardEffectType.DefenseAttackDamageMultiplier:
                return $"所有防御塔攻击力 {valueText}";
            case RewardEffectType.DefenseAttackSpeedMultiplier:
                return $"所有防御塔攻击速度 {valueText}";
            case RewardEffectType.DefenseDetectRadiusMultiplier:
                return $"所有防御塔攻击范围 {valueText}";
            case RewardEffectType.DefenseMaxHealthMultiplier:
                return $"所有防御塔生命值 {valueText}";
            case RewardEffectType.DefenseBuildCostMultiplier:
                return $"所有防御塔建造成本 {valueText}";
            default:
                return valueText;
        }
    }

    // 构建带颜色的奖励数值文本。
    private string BuildColoredValueText(RewardEffectConfig effectConfig)
    {
        string valueText = FormatPercent(effectConfig.Value);
        string colorHex = GetImpactColorHex(GetEffectImpact(effectConfig));
        return $"<color={colorHex}>{valueText}</color>";
    }

    // 获取奖励效果对玩家的显示倾向。
    private RewardEffectDisplayImpact GetEffectImpact(RewardEffectConfig effectConfig)
    {
        if (effectConfig.DisplayImpact != RewardEffectDisplayImpact.Auto)
        {
            return effectConfig.DisplayImpact;
        }

        return GetAutoEffectImpact(effectConfig);
    }

    // 根据效果类型和值自动判断收益、惩罚或中性显示。
    private RewardEffectDisplayImpact GetAutoEffectImpact(RewardEffectConfig effectConfig)
    {
        if (Mathf.Approximately(effectConfig.Value, 0f))
        {
            return RewardEffectDisplayImpact.Neutral;
        }

        switch (effectConfig.EffectType)
        {
            case RewardEffectType.DefenseBuildCostMultiplier:
                return effectConfig.Value < 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            case RewardEffectType.DefenseAttackDamageMultiplier:
            case RewardEffectType.DefenseAttackSpeedMultiplier:
            case RewardEffectType.DefenseDetectRadiusMultiplier:
            case RewardEffectType.DefenseMaxHealthMultiplier:
                return effectConfig.Value > 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            default:
                return effectConfig.Value > 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
        }
    }

    // 获取显示倾向对应的富文本颜色。
    private string GetImpactColorHex(RewardEffectDisplayImpact displayImpact)
    {
        switch (displayImpact)
        {
            case RewardEffectDisplayImpact.Positive:
                return POSITIVE_COLOR_HEX;
            case RewardEffectDisplayImpact.Negative:
                return NEGATIVE_COLOR_HEX;
            case RewardEffectDisplayImpact.Neutral:
                return NEUTRAL_COLOR_HEX;
            default:
                return NEUTRAL_COLOR_HEX;
        }
    }

    // 把倍率增量格式化成百分比文本。
    private string FormatPercent(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        return percent > 0 ? $"+{percent}%" : $"{percent}%";
    }

    // 处理玩家选择当前卡牌的点击行为。
    private void HandleSelectClicked()
    {
        if (!_controller || !_rewardCard)
        {
            return;
        }

        _controller.SelectCard(_rewardCard);
    }
}
