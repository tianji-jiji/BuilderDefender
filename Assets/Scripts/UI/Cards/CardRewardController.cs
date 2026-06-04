using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class CardRewardController : MonoBehaviour
{
    [SerializeField] private RewardCardPoolSo rewardCardPool;
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;
    [SerializeField] private Transform cardOptionRoot;
    [SerializeField] private MMF_Player showFeedbacks;
    [SerializeField] private bool pauseGameDuringSelection = true;

    private readonly List<GameObject> _spawnedCardObjectList = new List<GameObject>();
    private EnemyWaveManager _waveManager;
    private float _previousTimeScale = 1f;
    private bool _isShowing;

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
    }

    // 处理玩家选择奖励卡后的效果应用和界面关闭。
    public void SelectCard(RewardCardSo rewardCard)
    {
        if (!_isShowing || !rewardCard)
        {
            return;
        }

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
        ClearOptions();
        HideCanvas();
        ResumeGameTime();
        _isShowing = false;
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

    // 显示奖励选卡画布。
    private void ShowCanvas()
    {
        if (!cardRewardCanvasGroup)
        {
            return;
        }

        cardRewardCanvasGroup.transform.localScale = Vector3.one;
        cardRewardCanvasGroup.interactable = true;
        cardRewardCanvasGroup.blocksRaycasts = true;

        PlayShowFeedback();
    }

    // 隐藏奖励选卡画布。
    private void HideCanvas()
    {
        if (!cardRewardCanvasGroup)
        {
            return;
        }

        cardRewardCanvasGroup.alpha = 0f;
        cardRewardCanvasGroup.interactable = false;
        cardRewardCanvasGroup.blocksRaycasts = false;
    }

    // 播放奖励弹窗显示反馈，没有配置 Feel 时直接显示画布。
    private void PlayShowFeedback()
    {
        if (showFeedbacks)
        {
            cardRewardCanvasGroup.alpha = 0f;
            showFeedbacks.PlayFeedbacks();
            return;
        }

        cardRewardCanvasGroup.alpha = 1f;
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
