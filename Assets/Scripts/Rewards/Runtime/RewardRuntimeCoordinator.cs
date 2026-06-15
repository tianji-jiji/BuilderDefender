using System;
using UnityEngine;

/// <summary>
/// 奖励运行时协调器，负责应用奖励卡牌并协调各奖励模块的运行时状态。
/// </summary>
public class RewardRuntimeCoordinator : MonoBehaviour
{
    public static RewardRuntimeCoordinator Instance;

    // 当前生效奖励发生变化时通知所有需要刷新属性的对象。
    public static event Action OnActiveRewardsChanged;

    private readonly DefenseTowerRewardModule _defenseTowerRewards = new();
    private readonly ResourceRewardModule _resourceRewards = new();
    private readonly HomeRewardModule _homeRewards = new();

    private EnemyWaveManager _waveManager;

    public DefenseTowerRewardModule DefenseTowerRewards => _defenseTowerRewards;
    public ResourceRewardModule ResourceRewards => _resourceRewards;
    public HomeRewardModule HomeRewards => _homeRewards;

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

    // 应用一张奖励卡中配置的全部效果。
    public void ApplyReward(RewardCardSo rewardCard)
    {
        if (!rewardCard)
        {
            return;
        }

        RewardEffectApplyContext applyContext = new(
            this,
            _defenseTowerRewards,
            _resourceRewards,
            _homeRewards,
            ResourceManager.Instance,
            EnemyWaveManager.Instance,
            BuildingPlacementManager.Instance);

        RewardEffectApplicationRouter.ApplyEffects(rewardCard.EffectConfigList, applyContext);
        RecordRewardCardSelection(rewardCard);
        OnActiveRewardsChanged?.Invoke();
    }

    // 记录本次奖励选择，缺少历史组件时只输出警告而不阻断数值生效。
    private void RecordRewardCardSelection(RewardCardSo rewardCard)
    {
        if (RewardCardAcquisitionHistory.Instance)
        {
            RewardCardAcquisitionHistory.Instance.RecordRewardCard(rewardCard);
            return;
        }

        Debug.LogWarning("RewardCardSelectionHistory is missing in scene. Reward bonus applied, but visible reward history was not recorded.");
    }

    // 绑定当前场景中的波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !EnemyWaveManager.Instance)
        {
            return;
        }

        _waveManager = EnemyWaveManager.Instance;
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
        _defenseTowerRewards.OnWaveCompleted(waveIndex);
    }
}
