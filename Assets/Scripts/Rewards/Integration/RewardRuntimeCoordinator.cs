using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励运行时协调器，负责组装依赖并协调抽卡、效果应用和通知流程。
/// </summary>
public class RewardRuntimeCoordinator : MonoBehaviour
{
    public static RewardRuntimeCoordinator Instance { get; private set; }

    [SerializeField] private BuildingPlacementManager buildingPlacementManager;

    private RewardCardPoolSo _rewardCardPool;
    private RewardCardDrawService _drawService;
    private RewardHistory _history;
    private TowerRewardRuntime _towerRewards;
    private WaveManager _waveManager;

    // 奖励候选生成完成后触发。
    public event Action<IReadOnlyList<RewardCardSo>> OnRewardChoicesReady;

    // 奖励成功应用并写入历史后触发。
    public event Action<RewardAppliedContext> OnRewardApplied;

    // 当前生效奖励状态发生变化后触发。
    public event Action OnActiveRewardsChanged;

    public TowerRewardRuntime TowerRewards => _towerRewards;
    public RewardHistory History => _history;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        TryInitializeRuntime(false);
    }

    private void OnEnable()
    {
        BindWaveManager();
    }

    private void OnDisable()
    {
        UnbindWaveManager();
    }

    private void Start()
    {
        TryInitializeRuntime(true);
        BindWaveManager();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 配置当前场景使用的奖励卡池。
    public void ConfigureCardPool(RewardCardPoolSo rewardCardPool)
    {
        if (rewardCardPool)
        {
            _rewardCardPool = rewardCardPool;
        }
    }

    // 应用一张奖励卡并广播应用结果。
    public void ApplyReward(RewardCardSo rewardCard)
    {
        TryInitializeRuntime(false);
        if (!rewardCard || _towerRewards == null || _history == null)
        {
            return;
        }

        RewardApplyContext applyContext = new(_towerRewards);
        RewardEffectApplicationService.ApplyEffects(rewardCard.EffectConfigList, applyContext);

        RewardCardRecord record = _history.Record(rewardCard, GetCurrentWaveIndex());
        RewardAppliedContext appliedContext = new(
            rewardCard,
            record,
            _history.RecordList,
            _history.TotalCardCount);
        OnRewardApplied?.Invoke(appliedContext);
        OnActiveRewardsChanged?.Invoke();
    }

    // 创建本局奖励运行时依赖。
    private void TryInitializeRuntime(bool logMissingManager)
    {
        if (_history != null && _drawService != null && _towerRewards != null)
        {
            return;
        }

        buildingPlacementManager = buildingPlacementManager
            ? buildingPlacementManager
            : BuildingPlacementManager.Instance;
        if (!buildingPlacementManager)
        {
            if (logMissingManager)
            {
                Debug.LogError("RewardRuntimeCoordinator 缺少 BuildingPlacementManager。", this);
            }

            return;
        }

        IRewardRandom random = new UnityRewardRandom();
        _history ??= new RewardHistory();
        _drawService ??= new RewardCardDrawService(random);
        _towerRewards ??= new TowerRewardRuntime(
            new TowerRewardWorld(buildingPlacementManager.BuildingRegistry, random));
    }

    // 绑定当前波次管理器。
    private void BindWaveManager()
    {
        if (_waveManager || !WaveManager.Instance)
        {
            return;
        }

        _waveManager = WaveManager.Instance;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
    }

    // 解绑当前波次管理器。
    private void UnbindWaveManager()
    {
        if (!_waveManager)
        {
            return;
        }

        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
        _waveManager = null;
    }

    // 处理波次结束奖励和候选卡生成。
    private void HandleWaveCompleted(int waveIndex)
    {
        _towerRewards?.OnWaveCompleted();
        if (!_rewardCardPool || _drawService == null || _history == null)
        {
            return;
        }

        RewardCardDrawContext drawContext = _history.BuildDrawContext(waveIndex);
        IReadOnlyList<RewardCardSo> candidateCardList = _drawService.Draw(
            _rewardCardPool,
            drawContext);
        if (candidateCardList.Count > 0)
        {
            OnRewardChoicesReady?.Invoke(candidateCardList);
        }
    }

    // 返回当前波次索引。
    private int GetCurrentWaveIndex()
    {
        return _waveManager ? _waveManager.waveIndex : 0;
    }
}
