using System;
using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 奖励卡牌选择面板，负责面板显示、隐藏、交互开关和展示 Feel 反馈。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(MMF_Player))]
[DefaultExecutionOrder(-100)]
public class RewardCardSelectionPanel : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private MMF_Player _showFeedback;
    private Coroutine _showCoroutine;
    private Action _onShowComplete;

    // 初始化面板依赖和默认隐藏状态。
    private void Awake()
    {
        CacheReferences();
        Hide();
    }

    // 停止未完成的展示流程。
    private void OnDisable()
    {
        StopShowCoroutine();
        StopShowFeedback();
        _onShowComplete = null;
    }

    // 显示面板，并在展示反馈完成后执行回调。
    public void Show(Action onShowComplete)
    {
        StopShowCoroutine();
        StopShowFeedback();
        _onShowComplete = onShowComplete;
        _canvasGroup.transform.localScale = Vector3.one;
        SetInteraction(false, true);
        _canvasGroup.alpha = 0f;
        _showFeedback.PlayFeedbacks();
        _showCoroutine = StartCoroutine(WaitForShowFeedbackComplete());
    }

    // 隐藏面板并关闭交互。
    public void Hide()
    {
        StopShowCoroutine();
        StopShowFeedback();
        _onShowComplete = null;
        _canvasGroup.alpha = 0f;
        SetInteraction(false, false);
    }

    // 设置面板交互和射线拦截状态。
    public void SetInteraction(bool isInteractable, bool blocksRaycasts)
    {
        _canvasGroup.interactable = isInteractable;
        _canvasGroup.blocksRaycasts = blocksRaycasts;
    }

    // 缓存面板依赖的组件。
    private void CacheReferences()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _showFeedback = GetComponent<MMF_Player>();
    }

    // 等待展示反馈结束。
    private IEnumerator WaitForShowFeedbackComplete()
    {
        while (_showFeedback.IsPlaying)
        {
            yield return null;
        }

        _showCoroutine = null;
        CompleteShow();
    }

    // 完成面板展示流程。
    private void CompleteShow()
    {
        _canvasGroup.alpha = 1f;
        SetInteraction(true, true);
        _onShowComplete?.Invoke();
        _onShowComplete = null;
    }

    // 停止正在等待的展示完成协程。
    private void StopShowCoroutine()
    {
        if (_showCoroutine == null)
        {
            return;
        }

        StopCoroutine(_showCoroutine);
        _showCoroutine = null;
    }

    // 停止尚未完成的奖励面板展示反馈。
    private void StopShowFeedback()
    {
        if (!_showFeedback.IsPlaying)
        {
            return;
        }

        _showFeedback.StopFeedbacks();
    }
}
