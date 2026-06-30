using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励卡牌选择控制器，负责展示候选卡牌并转发玩家选择。
/// </summary>
[RequireComponent(typeof(RewardCardSelectionPanel))]
public class RewardCardSelectionController : MonoBehaviour
{
    [SerializeField] private RewardCardPoolSo rewardCardDrawPool;
    [SerializeField] private Transform cardOptionRoot;
    [SerializeField] private bool pauseGameDuringSelection = true;

    private readonly List<RewardCardOption> _spawnedCardOptionList = new();
    private RewardCardSelectionPanel _selectionPanel;
    private RewardRuntimeCoordinator _coordinator;
    private float _previousTimeScale = 1f;
    private bool _isShowing;
    private bool _canSelectCard;

    // 初始化奖励选卡界面的基础状态。
    private void Awake()
    {
        CacheReferences();
        _selectionPanel.Hide();
    }

    private void OnEnable()
    {
        BindCoordinator();
    }

    // 解绑波次结束事件并恢复时间流速。
    private void OnDisable()
    {
        UnbindCoordinator();
        CloseRewardChoices();
    }

    // 启动时配置卡池并绑定协调器事件。
    private void Start()
    {
        BindCoordinator();
    }

    // 缓存奖励选卡控制器依赖的组件。
    private void CacheReferences()
    {
        _selectionPanel = GetComponent<RewardCardSelectionPanel>();
    }

    // 绑定奖励运行时协调器并配置卡池。
    private void BindCoordinator()
    {
        if (_coordinator)
        {
            return;
        }

        _coordinator = RewardRuntimeCoordinator.Instance;
        if (!_coordinator)
        {
            return;
        }

        _coordinator.ConfigureCardPool(rewardCardDrawPool);
        _coordinator.OnRewardChoicesReady += HandleRewardChoicesReady;
    }

    // 取消订阅奖励候选事件。
    private void UnbindCoordinator()
    {
        if (!_coordinator)
        {
            return;
        }

        _coordinator.OnRewardChoicesReady -= HandleRewardChoicesReady;
        _coordinator = null;
    }

    // 展示协调器生成的奖励候选卡牌。
    private void HandleRewardChoicesReady(IReadOnlyList<RewardCardSo> candidateCardList)
    {
        if (_isShowing
            || candidateCardList == null
            || candidateCardList.Count <= 0
            || !cardOptionRoot)
        {
            return;
        }

        ClearOptions();
        foreach (RewardCardSo rewardCard in candidateCardList)
        {
            SpawnCardOption(rewardCard);
        }

        if (_spawnedCardOptionList.Count <= 0)
        {
            return;
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
        _coordinator?.ApplyReward(rewardCard);
        CloseRewardChoices();
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
