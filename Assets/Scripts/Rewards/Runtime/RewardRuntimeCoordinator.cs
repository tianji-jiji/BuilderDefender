using System;
using UnityEngine;

/// <summary>
/// 奖励运行时协调器，负责应用奖励卡牌并协调各奖励运行时状态。
/// </summary>
public class RewardRuntimeCoordinator : MonoBehaviour
{
    public static RewardRuntimeCoordinator Instance;

    // 当前生效奖励发生变化时通知所有需要刷新属性的对象。
    public static event Action OnActiveRewardsChanged;

    private readonly TowerRewardRuntime towerRewards = new();

    private WaveManager _waveManager;

    public TowerRewardRuntime TowerRewards => towerRewards;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BindWaveManager();
    }

    private void OnDisable()
    {
        UnbindWaveManager();
    }

    // 应用一整张奖励卡
    public void ApplyReward(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return;
        }

        // 创建上下文
        RewardEffectApplyContext applyContext = new(
            this,
            towerRewards,
            ResourceManager.Instance,
            WaveManager.Instance,
            BuildingPlacementManager.Instance);
        // 应用卡牌效果
        RewardEffectApplicationService.ApplyEffects(rewardCard.EffectConfigList, applyContext);
        // 记录这张卡被选择过
        RecordRewardCardSelection(rewardCard);
        // 通知奖励发生变化
        OnActiveRewardsChanged?.Invoke();
    }

    // 记录本次奖励选择
    private void RecordRewardCardSelection(RewardCardSo rewardCard)
    {
        if (RewardCardAcquiredHistory.Instance)
        {
            RewardCardAcquiredHistory.Instance.RecordRewardCard(rewardCard);
        }
    }

    // 绑定当前场景中的波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !WaveManager.Instance)
        {
            return;
        }

        _waveManager = WaveManager.Instance;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
    }

    // 解绑当前场景中的波次管理器。
    private void UnbindWaveManager()
    {
        if (!_waveManager)
        {
            return;
        }

        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
        _waveManager = null;
    }

    // 波次结束时应用波次结算型奖励。
    private void HandleWaveCompleted(int waveIndex)
    {
        towerRewards.OnWaveCompleted();
    }
}
