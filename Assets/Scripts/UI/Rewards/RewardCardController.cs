using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励卡牌选择控制器，负责协调波次结束、奖励抽取、卡牌生成和奖励应用。
/// </summary>
[RequireComponent(typeof(RewardCardSelectionPanel))]
public class RewardCardController : MonoBehaviour
{
    [SerializeField] private RewardCardDrawPoolSo rewardCardDrawPool;
    [SerializeField] private Transform cardOptionRoot;
    [SerializeField] private bool pauseGameDuringSelection = true;

    private readonly List<RewardCardOption> _spawnedCardOptionList = new();
    private RewardCardSelectionPanel _selectionPanel;
    private EnemyWaveManager _waveManager;
    private float _previousTimeScale = 1f;
    private bool _isShowing;
    private bool _canSelectCard;

    // 初始化奖励选卡界面的基础状态。
    private void Awake()
    {
        CacheReferences();
        _selectionPanel.Hide();
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
        CloseRewardChoices();
    }

    // 缓存奖励选卡控制器依赖的组件。
    private void CacheReferences()
    {
        _selectionPanel = GetComponent<RewardCardSelectionPanel>();
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
        if (_isShowing || !rewardCardDrawPool || !cardOptionRoot)
        {
            return;
        }

        ClearOptions();
        RewardCardDrawContext drawContext = RewardCardAcquiredHistory.Instance
            ? RewardCardAcquiredHistory.Instance.BuildRewardCardOfferContext()
            : RewardCardDrawContext.Default(_waveManager ? _waveManager.waveIndex : 0);
        List<RewardCardSo> rewardCardList = rewardCardDrawPool.DrawCards(drawContext);
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
        ShowSelectionPanel();
    }

    // 实例化一张奖励卡选项。
    private void SpawnCardOption(RewardCardSo rewardCard)
    {
        if (!rewardCard || !rewardCard.CardPrefab)
        {
            return;
        }

        GameObject cardObject = Instantiate(rewardCard.CardPrefab, cardOptionRoot);

        if (cardObject.TryGetComponent(out RewardCardOption rewardCardOption))
        {
            rewardCardOption.Init(rewardCard);
            rewardCardOption.SetInteractable(_canSelectCard);
            rewardCardOption.OnSelected += HandleCardSelected;
            _spawnedCardOptionList.Add(rewardCardOption);
            return;
        }

        Destroy(cardObject);
    }

    // 处理玩家点击选择奖励卡牌。
    private void HandleCardSelected(RewardCardOption rewardCardOption, RewardCardSo rewardCard)
    {
        if (!_isShowing || !_canSelectCard || !rewardCardOption || !rewardCard)
        {
            return;
        }

        _canSelectCard = false;
        _selectionPanel.SetInteraction(false, true);
        SetCardOptionsInteractable(false);
        rewardCardOption.PlaySelectFeedback(() => CompleteCardSelection(rewardCard));
    }

    // 完成卡牌选择流程并关闭选卡界面。
    private void CompleteCardSelection(RewardCardSo rewardCard)
    {
        ApplyReward(rewardCard);
        CloseRewardChoices();
    }

    // 应用玩家选择的奖励卡效果。
    private void ApplyReward(RewardCardSo rewardCard)
    {
        if (!RewardRuntimeCoordinator.Instance)
        {
            return;
        }

        RewardRuntimeCoordinator.Instance.ApplyReward(rewardCard);
    }

    // 关闭选卡界面并清理本次生成的卡牌。
    private void CloseRewardChoices()
    {
        bool wasShowing = _isShowing;
        ClearOptions();
        _selectionPanel.Hide();
        if (wasShowing)
        {
            ResumeGameTime();
        }

        _isShowing = false;
        _canSelectCard = false;
    }

    // 清理选卡根节点下的所有卡牌实例。
    private void ClearOptions()
    {
        foreach (RewardCardOption rewardCardOption in _spawnedCardOptionList)
        {
            if (!rewardCardOption)
            {
                continue;
            }

            rewardCardOption.OnSelected -= HandleCardSelected;
            Destroy(rewardCardOption.gameObject);
        }

        _spawnedCardOptionList.Clear();
    }

    // 显示奖励选择面板。
    private void ShowSelectionPanel()
    {
        _canSelectCard = false;
        SetCardOptionsInteractable(false);
        _selectionPanel.Show(EnableCardSelection);
    }

    // 允许玩家正式点击奖励卡牌。
    private void EnableCardSelection()
    {
        if (!_isShowing)
        {
            return;
        }

        _canSelectCard = true;
        _selectionPanel.SetInteraction(true, true);
        SetCardOptionsInteractable(true);
    }

    // 设置所有奖励卡选项的点击交互状态。
    private void SetCardOptionsInteractable(bool isInteractable)
    {
        foreach (RewardCardOption rewardCardOption in _spawnedCardOptionList)
        {
            if (!rewardCardOption)
            {
                continue;
            }

            rewardCardOption.SetInteractable(isInteractable);
        }
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
