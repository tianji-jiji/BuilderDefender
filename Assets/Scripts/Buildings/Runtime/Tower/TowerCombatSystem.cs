using UnityEngine;

/// <summary>
/// 防御塔战斗门面，负责协调索敌、发射、属性计算和攻击型奖励效果。
/// </summary>
public class TowerCombatSystem : MonoBehaviour
{
    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private float arrowGenerateRate = 0.3f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private TowerTargetSelector targetSelector;
    [SerializeField] private TowerArrowLauncher arrowLauncher;

    private TowerStatCalculator _statCalculator;
    private TowerCombatStats _currentStats;
    private HealthSystem _healthSystem;
    private BuildingRuntimeRegistry _buildingRegistry;
    private TowerRewardRuntime _rewardRuntime;
    private RewardRuntimeCoordinator _rewardCoordinator;
    private bool _isSubscribedToRewardBonuses;

    private float _timer;
    private bool _hasEnemy;
    public int CurrentStarLevel => _statCalculator?.CurrentStarLevel ?? 1;
    private TowerRewardTriggerDispatcher ActiveTowerRewardTriggerDispatcher
        => _rewardRuntime?.TriggerDispatcher;

    private void Awake()
    {
        TryGetComponent(out _healthSystem);
        CacheCombatComponents();
    }

    private void OnEnable()
    {
        TrySubscribeRewardBonuses();
        _buildingRegistry?.RegisterTower(this);
    }

    private void OnDisable()
    {
        TryUnsubscribeRewardBonuses();
        ActiveTowerRewardTriggerDispatcher?.ClearSource(this);
        _buildingRegistry?.UnregisterTower(this);
        CancelInvoke();
        _hasEnemy = false;
        targetSelector?.ClearTarget();
    }

    private void Start()
    {
        CacheRuntimeDependencies();
        _statCalculator = new TowerStatCalculator(
            this,
            attackDamage,
            arrowGenerateRate,
            detectRadius,
            _rewardRuntime);
        _currentStats = _statCalculator.RefreshStats();
        _buildingRegistry?.RegisterTower(this);
        TrySubscribeRewardBonuses();
        RefreshRewardBonuses();
        InvokeRepeating(nameof(DetectEnemy), 0f, TowerTargetSelector.DETECT_INTERVAL);
    }

    private void Update()
    {
        if (!_hasEnemy)
        {
            return;
        }

        _hasEnemy = targetSelector && targetSelector.RefreshCurrentTarget(_currentStats.DetectRadius);
        if (!_hasEnemy)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            ExecuteAttack();
            _timer = _currentStats.ArrowGenerateRate;
        }
    }

    // 根据升星配置提升攻击力、射速并解锁超级箭。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        _statCalculator?.ApplyUpgradeLevel(upgradeLevel);
        RefreshRewardBonuses();
    }

    // 通知运行时卡牌效果本防御塔击杀了敌人。
    public void NotifyEnemyKilled()
    {
        ActiveTowerRewardTriggerDispatcher?.OnEnemyKilled(new TowerEnemyKilledContext(this));
    }

    // 缓存防御塔战斗子组件。
    private void CacheCombatComponents()
    {
        if (!targetSelector)
        {
            TryGetComponent(out targetSelector);
        }

        if (!arrowLauncher)
        {
            TryGetComponent(out arrowLauncher);
        }
    }

    // 缓存奖励运行时和场景建筑注册表。
    private void CacheRuntimeDependencies()
    {
        _rewardCoordinator = RewardRuntimeCoordinator.Instance;
        _rewardRuntime = _rewardCoordinator
            ? _rewardCoordinator.TowerRewards
            : null;
        _buildingRegistry = BuildingPlacementManager.Instance
            ? BuildingPlacementManager.Instance.BuildingRegistry
            : null;
    }

    // 订阅奖励状态变化事件。
    private void TrySubscribeRewardBonuses()
    {
        if (_isSubscribedToRewardBonuses)
        {
            return;
        }

        _rewardCoordinator = _rewardCoordinator
            ? _rewardCoordinator
            : RewardRuntimeCoordinator.Instance;
        if (!_rewardCoordinator)
        {
            return;
        }

        _rewardCoordinator.OnActiveRewardsChanged += RefreshRewardBonuses;
        _isSubscribedToRewardBonuses = true;
    }

    // 取消订阅奖励状态变化事件。
    private void TryUnsubscribeRewardBonuses()
    {
        if (!_isSubscribedToRewardBonuses)
        {
            return;
        }

        if (_rewardCoordinator)
        {
            _rewardCoordinator.OnActiveRewardsChanged -= RefreshRewardBonuses;
        }

        _isSubscribedToRewardBonuses = false;
    }

    // 侦测范围内最近的有效敌人并设置为当前攻击目标。
    private void DetectEnemy()
    {
        _hasEnemy = targetSelector && targetSelector.HasTargetInRadius(_currentStats.DetectRadius);
    }

    // 执行一次主动攻击并处理攻击触发型奖励。
    private void ExecuteAttack()
    {
        if (!arrowLauncher || !targetSelector || _statCalculator == null)
        {
            _hasEnemy = false;
            return;
        }

        bool hasFired = arrowLauncher.FirePrimaryShotGroup(this, targetSelector, _statCalculator, ActiveTowerRewardTriggerDispatcher);
        if (!hasFired && TryRefreshTargetsForImmediateShot())
        {
            hasFired = arrowLauncher.FirePrimaryShotGroup(this, targetSelector, _statCalculator, ActiveTowerRewardTriggerDispatcher);
        }

        if (!hasFired)
        {
            _hasEnemy = false;
            targetSelector.ClearTarget();
            return;
        }

        HandleAttackTriggeredRewards();
    }

    // 发射缓存过期时立即重新检测一次
    private bool TryRefreshTargetsForImmediateShot()
    {
        DetectEnemy();
        return _hasEnemy;
    }

    // 处理一次主动攻击触发后的奖励效果。
    private void HandleAttackTriggeredRewards()
    {
        TowerAttackCompletedContext attackContext = new(this, _healthSystem);
        ActiveTowerRewardTriggerDispatcher?.OnAttackCompleted(attackContext);
        arrowLauncher.FireExtraAttackArrows(attackContext.ExtraAttackCount, this, targetSelector, _statCalculator, ActiveTowerRewardTriggerDispatcher);
    }

    // 根据升星倍率和全局奖励倍率刷新防御塔战斗属性。
    private void RefreshRewardBonuses()
    {
        if (_statCalculator == null)
        {
            return;
        }

        _currentStats = _statCalculator.RefreshStats();
        attackDamage = _currentStats.AttackDamage;
        arrowGenerateRate = _currentStats.ArrowGenerateRate;
        detectRadius = _currentStats.DetectRadius;
    }
}
