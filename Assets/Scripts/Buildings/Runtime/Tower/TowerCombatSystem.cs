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

    private float _timer;
    private bool _hasEnemy;
    public int CurrentStarLevel => _statCalculator?.CurrentStarLevel ?? 1;
    private TowerRewardTriggerDispatcher ActiveTowerRewardTriggerDispatcher => RewardRuntimeCoordinator.Instance
        ? RewardRuntimeCoordinator.Instance.TowerRewards.TriggerDispatcher
        : null;

    private void Awake()
    {
        TryGetComponent(out _healthSystem);
        CacheCombatComponents();
        _statCalculator = new TowerStatCalculator(this, attackDamage, arrowGenerateRate, detectRadius);
        _currentStats = _statCalculator.RefreshStats();
    }

    private void OnEnable()
    {
        RewardRuntimeCoordinator.OnActiveRewardsChanged += RefreshRewardBonuses;
        TowerRegistry.RegisterDefenseTowerSystem(this);
    }

    private void OnDisable()
    {
        RewardRuntimeCoordinator.OnActiveRewardsChanged -= RefreshRewardBonuses;
        ActiveTowerRewardTriggerDispatcher?.ClearSource(this);
        TowerRegistry.UnregisterDefenseTowerSystem(this);
        CancelInvoke();
        _hasEnemy = false;
        targetSelector?.ClearTarget();
    }

    private void Start()
    {
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
