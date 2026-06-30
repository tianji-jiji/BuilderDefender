using System;
using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 奖励卡牌选项组件，负责单张奖励卡牌的数据显示、指针交互和 Feel 反馈触发。
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RewardCardView))]
public class RewardCardOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MMF_Player hoverEnterFeedback;
    [SerializeField] private MMF_Player hoverExitFeedback;
    [SerializeField] private MMF_Player selectFeedback;
    [SerializeField] private ParticleSystem hoverParticles;

    private Button _selectButton;
    private RewardCardView _cardView;
    private RewardCardSo _rewardCard;
    private Coroutine _selectFeedbackCoroutine;
    private bool _isInteractable = true;
    private bool _isSelected;

    // 当玩家点击选择这张奖励卡时触发。
    public event Action<RewardCardOption, RewardCardSo> OnSelected;

    
    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();

        _selectButton.onClick.AddListener(HandleSelectClicked);
    }

    private void OnDisable()
    {
        _selectButton.onClick.RemoveListener(HandleSelectClicked);

        StopSelectFeedbackCoroutine();
    }

    // 初始化本次实例对应的奖励卡数据和控制器。
    public void Init(RewardCardSo rewardCard)
    {
        _rewardCard = rewardCard;
        _isSelected = false;
        SetInteractable(true);

        _cardView.SetCard(rewardCard);
    }

    // 设置卡牌是否允许继续响应点击。
    public void SetInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;

        _selectButton.interactable = isInteractable;
    }

    // 播放选中反馈，并在反馈结束后执行完成回调。
    public void PlaySelectFeedback(Action onComplete)
    {
        _isSelected = true;
        SetInteractable(false);
        StopHoverFeedbacks();
        StopSelectFeedbackCoroutine();

        selectFeedback.PlayFeedbacks();
        _selectFeedbackCoroutine = StartCoroutine(WaitForSelectFeedbackComplete(onComplete));
    }

    // 处理指针进入卡牌区域。
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanPlayPointerFeedback())
        {
            return;
        }

        hoverExitFeedback.StopFeedbacks();
        hoverEnterFeedback.PlayFeedbacks();
        hoverParticles.Play();
    }

    // 处理指针离开卡牌区域。
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected)
        {
            return;
        }

        hoverEnterFeedback.StopFeedbacks();
        hoverExitFeedback.PlayFeedbacks();
        hoverParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // 处理玩家选择当前卡牌的点击行为。
    private void HandleSelectClicked()
    {
        if (!_isInteractable || _isSelected || !_rewardCard)
        {
            return;
        }

        OnSelected?.Invoke(this, _rewardCard);
    }

    // 缓存卡牌选项依赖的组件。
    private void CacheReferences()
    {
        _selectButton = GetComponent<Button>();
        _cardView = GetComponent<RewardCardView>();
    }

    // 判断当前是否可以播放指针反馈。
    private bool CanPlayPointerFeedback()
    {
        return _isInteractable && !_isSelected;
    }

    // 停止当前悬浮相关反馈。
    private void StopHoverFeedbacks()
    {
        hoverEnterFeedback.StopFeedbacks();
        hoverExitFeedback.StopFeedbacks();
        hoverParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // 等待选择反馈完成。
    private IEnumerator WaitForSelectFeedbackComplete(Action onComplete)
    {
        yield return null;

        while (IsSelectFeedbackPlaying())
        {
            yield return null;
        }

        _selectFeedbackCoroutine = null;
        onComplete?.Invoke();
    }

    // 判断选中反馈播放器或内部任意反馈是否仍在播放。
    private bool IsSelectFeedbackPlaying()
    {
        return selectFeedback && (selectFeedback.IsPlaying || selectFeedback.HasFeedbackStillPlaying());
    }

    // 停止选择反馈等待协程。
    private void StopSelectFeedbackCoroutine()
    {
        if (_selectFeedbackCoroutine == null)
        {
            return;
        }

        StopCoroutine(_selectFeedbackCoroutine);
        _selectFeedbackCoroutine = null;
    }
}
