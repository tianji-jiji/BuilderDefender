using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 奖励卡牌选择控制器，负责在波次结束后展示卡牌并应用玩家选择。
/// </summary>
public class CardRewardController : MonoBehaviour
{
    [SerializeField] private RewardCardPoolSo rewardCardPool;
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;
    [SerializeField] private Transform cardOptionRoot;
    [SerializeField] private MMF_Player showFeedbacks;
    [SerializeField] private bool pauseGameDuringSelection = true;

    private readonly List<GameObject> _spawnedCardObjectList = new();
    private EnemyWaveManager _waveManager;
    private float _previousTimeScale = 1f;
    private Coroutine _showCoroutine;
    private bool _isShowing;
    private bool _canSelectCard;

    // 初始化奖励选卡界面的基础状态。
    private void Awake()
    {
        CacheReferences();
        HideCanvas();
    }

    // 绑定波次结束事件。
    private void Start()
    {
        BindWaveManager();
    }

    // 解绑波次结束事件并恢复时间流速。
    private void OnDisable()
    {
        UnbindWaveManager();

        if (_isShowing && pauseGameDuringSelection)
        {
            Time.timeScale = _previousTimeScale;
        }

        StopShowCoroutine();
        HideCanvas();
        _isShowing = false;
        _canSelectCard = false;
    }

    // 处理玩家选择奖励卡后的效果应用和界面关闭。
    public void SelectCard(RewardCardSo rewardCard)
    {
        if (!_isShowing || !_canSelectCard || !rewardCard)
        {
            return;
        }

        _canSelectCard = false;
        SetCanvasInteraction(false, true);

        if (RewardBonusManager.Instance)
        {
            RewardBonusManager.Instance.ApplyReward(rewardCard);
        }

        CloseRewardChoices();
    }

    // 缓存奖励选卡控制器依赖的组件。
    private void CacheReferences()
    {
        if (!cardRewardCanvasGroup)
        {
            TryGetComponent(out cardRewardCanvasGroup);
        }

        if (!showFeedbacks)
        {
            TryGetComponent(out showFeedbacks);
        }
    }

    // 订阅当前场景中的波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !EnemyWaveManager.Instance)
        {
            return;
        }

        _waveManager = EnemyWaveManager.Instance;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
    }

    // 取消订阅当前场景中的波次管理器。
    private void UnbindWaveManager()
    {
        if (!_waveManager)
        {
            return;
        }

        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
        _waveManager = null;
    }

    // 在波次结束时打开奖励选卡界面。
    private void HandleWaveCompleted(int waveIndex)
    {
        ShowRewardChoices();
    }

    // 抽取并展示本次可选的奖励卡。
    private void ShowRewardChoices()
    {
        if (_isShowing || !rewardCardPool || !cardOptionRoot)
        {
            return;
        }

        ClearOptions();
        List<RewardCardSo> rewardCardList = rewardCardPool.DrawCards();
        if (rewardCardList.Count <= 0)
        {
            return;
        }

        foreach (RewardCardSo rewardCard in rewardCardList)
        {
            SpawnCardOption(rewardCard);
        }

        _isShowing = true;
        PauseGameTime();
        ShowCanvas();
    }

    // 实例化一张奖励卡选项。
    private void SpawnCardOption(RewardCardSo rewardCard)
    {
        if (!rewardCard || !rewardCard.CardPrefab)
        {
            return;
        }

        GameObject cardObject = Instantiate(rewardCard.CardPrefab, cardOptionRoot);
        _spawnedCardObjectList.Add(cardObject);

        if (cardObject.TryGetComponent(out RewardCardOption rewardCardOption))
        {
            rewardCardOption.Init(rewardCard, this);
        }
    }

    // 关闭选卡界面并清理本次生成的卡牌。
    private void CloseRewardChoices()
    {
        StopShowCoroutine();
        ClearOptions();
        HideCanvas();
        ResumeGameTime();
        _isShowing = false;
        _canSelectCard = false;
    }

    // 清理选卡根节点下的所有卡牌实例。
    private void ClearOptions()
    {
        foreach (GameObject cardObject in _spawnedCardObjectList)
        {
            if (cardObject)
            {
                Destroy(cardObject);
            }
        }

        _spawnedCardObjectList.Clear();

        if (!cardOptionRoot)
        {
            return;
        }

        for (int i = cardOptionRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardOptionRoot.GetChild(i).gameObject);
        }
    }

    // 显示奖励选卡画布，并在展示完成前拦截点击。
    private void ShowCanvas()
    {
        if (!cardRewardCanvasGroup)
        {
            return;
        }

        cardRewardCanvasGroup.transform.localScale = Vector3.one;
        _canSelectCard = false;
        SetCanvasInteraction(false, true);

        PlayShowFeedback();
    }

    // 隐藏奖励选卡画布并停止未完成的展示反馈。
    private void HideCanvas()
    {
        if (!cardRewardCanvasGroup)
        {
            return;
        }

        StopShowFeedback();
        _canSelectCard = false;
        cardRewardCanvasGroup.alpha = 0f;
        SetCanvasInteraction(false, false);
    }

    // 播放奖励弹窗显示反馈，没有配置 Feel 时直接显示画布。
    private void PlayShowFeedback()
    {
        StopShowFeedback();

        if (showFeedbacks)
        {
            cardRewardCanvasGroup.alpha = 0f;
            showFeedbacks.PlayFeedbacks();
            _showCoroutine = StartCoroutine(WaitForShowFeedbackComplete());
            return;
        }

        cardRewardCanvasGroup.alpha = 1f;
        EnableCardSelection();
    }

    // 等待奖励面板展示反馈结束后再允许玩家点击卡牌。
    private IEnumerator WaitForShowFeedbackComplete()
    {
        while (showFeedbacks && showFeedbacks.IsPlaying)
        {
            yield return null;
        }

        _showCoroutine = null;

        if (!_isShowing || !cardRewardCanvasGroup)
        {
            yield break;
        }

        cardRewardCanvasGroup.alpha = 1f;
        EnableCardSelection();
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
        if (!showFeedbacks || !showFeedbacks.IsPlaying)
        {
            return;
        }

        showFeedbacks.StopFeedbacks();
    }

    // 允许玩家正式点击奖励卡牌。
    private void EnableCardSelection()
    {
        _canSelectCard = true;
        SetCanvasInteraction(true, true);
    }

    // 设置奖励面板的交互和射线拦截状态。
    private void SetCanvasInteraction(bool interactable, bool blocksRaycasts)
    {
        if (!cardRewardCanvasGroup)
        {
            return;
        }

        cardRewardCanvasGroup.interactable = interactable;
        cardRewardCanvasGroup.blocksRaycasts = blocksRaycasts;
    }

    // 暂停游戏时间以等待玩家选择奖励。
    private void PauseGameTime()
    {
        if (!pauseGameDuringSelection)
        {
            return;
        }

        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    // 恢复选卡前的游戏时间流速。
    private void ResumeGameTime()
    {
        if (!pauseGameDuringSelection)
        {
            return;
        }

        Time.timeScale = _previousTimeScale;
    }
}
