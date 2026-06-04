using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 防御塔战斗系统，负责索敌、发射箭矢、升星战斗属性和攻击型奖励效果。
/// </summary>
public class DefenseSystem : MonoBehaviour
{
    private const int MAX_DETECTED_ENEMIES = 10;
    private const float DETECT_INTERVAL = 0.2f;
    private const float MIN_ARROW_GENERATE_RATE = 0.05f;
    private const int DEFAULT_STAR_LEVEL = 1;
    private const int STAR_LEVEL_TWO = 2;
    private const int STAR_LEVEL_THREE = 3;

    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform[] superArrowSpawnPoints;
    [SerializeField] private Transform detectPoint;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private float arrowGenerateRate = 0.3f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Arrow arrowPrefab;
    [SerializeField] private Material starOneArrowMaterial;
    [SerializeField] private Material starTwoArrowMaterial;
    [SerializeField] private Material starThreeArrowMaterial;

    private readonly Collider2D[] _detectResults = new Collider2D[MAX_DETECTED_ENEMIES];
    private readonly List<int> _extraAttackCounterList = new List<int>();
    private HealthSystem _healthSystem;
    private float _baseDetectRadius;
    private float _baseArrowGenerateRate;
    private float _timer;
    private int _baseAttackDamage;
    private int _detectedEnemyCount;
    private int _currentStarLevel = DEFAULT_STAR_LEVEL;
    private int _killCountSinceUpgrade;
    private float _upgradeAttackDamageMultiplier = 1f;
    private float _upgradeAttackIntervalMultiplier = 1f;
    private float _upgradeDetectRadiusMultiplier = 1f;
    private bool _hasEnemy;
    private bool _superArrowUnlocked;
    private Enemy _currentTargetEnemy;

    public int CurrentStarLevel => _currentStarLevel;

    private enum TargetLane
    {
        Any,
        Upper,
        Lower
    }

    // 缓存防御塔的基础战斗属性和组件引用。
    private void Awake()
    {
        _baseDetectRadius = detectRadius;
        _baseArrowGenerateRate = arrowGenerateRate;
        _baseAttackDamage = attackDamage;
        TryGetComponent(out _healthSystem);
    }

    // 订阅全局奖励变化事件并注册防御塔。
    private void OnEnable()
    {
        RewardBonusManager.OnRewardBonusChanged += RefreshRewardBonuses;
        DefenseTowerRegistry.RegisterDefenseSystem(this);
    }

    // 启动防御塔的周期性敌人侦测。
    private void Start()
    {
        RefreshRewardBonuses();
        InvokeRepeating(nameof(DetectEnemy), 0f, DETECT_INTERVAL);
    }

    // 根据升星配置提升攻击力、射速并解锁超级箭。
    public void ApplyUpgradeLevel(BuildingUpgradeLevel upgradeLevel)
    {
        if (upgradeLevel == null)
        {
            return;
        }

        _upgradeAttackDamageMultiplier = upgradeLevel.AttackDamageMultiplier;
        _upgradeAttackIntervalMultiplier = upgradeLevel.AttackIntervalMultiplier;
        _upgradeDetectRadiusMultiplier = upgradeLevel.DetectRadiusMultiplier;
        _superArrowUnlocked = upgradeLevel.UnlockSuperArrow;
        _currentStarLevel = upgradeLevel.StarLevel;
        RefreshRewardBonuses();
    }

    // 记录箭矢击杀并按奖励规则自动升星。
    public void NotifyEnemyKilled()
    {
        if (!RewardBonusManager.Instance)
        {
            return;
        }

        int killCountToUpgrade = RewardBonusManager.Instance.DefenseKillCountAutoUpgrade;
        if (killCountToUpgrade <= 0)
        {
            return;
        }

        _killCountSinceUpgrade++;
        if (_killCountSinceUpgrade < killCountToUpgrade)
        {
            return;
        }

        _killCountSinceUpgrade = 0;
        BuildingUpgradeButton upgradeButton = DefenseTowerRegistry.GetUpgradeButton(this);
        if (upgradeButton)
        {
            upgradeButton.UpgradeOneLevelWithoutCost();
        }
    }

    // 根据攻击间隔持续向当前目标发射箭矢。
    private void Update()
    {
        if (!_hasEnemy)
        {
            return;
        }

        RefreshCurrentTargetIfNeeded();
        if (!_hasEnemy)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            GenerateArrow();
            _timer = GetCurrentArrowGenerateRate();
        }
    }

    // 生成本次主动攻击的箭矢，并处理攻击触发型奖励。
    private void GenerateArrow()
    {
        if (!arrowPrefab)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        bool hasFired = FirePrimaryShotGroup();
        if (!hasFired && TryRefreshTargetsForImmediateShot())
        {
            hasFired = FirePrimaryShotGroup();
        }

        if (!hasFired)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        HandleAttackTriggeredRewards();
    }

    // 发射当前星级对应的一组主动攻击箭矢。
    private bool FirePrimaryShotGroup()
    {
        bool hasFired = FireFromSpawnPoint(arrowSpawnPoint, TargetLane.Any);

        if (_superArrowUnlocked)
        {
            hasFired |= FireSuperArrowSpawnPoint(0, TargetLane.Upper);
            hasFired |= FireSuperArrowSpawnPoint(1, TargetLane.Lower);
        }

        return hasFired;
    }

    // 处理一次主动攻击触发后的奖励效果。
    private void HandleAttackTriggeredRewards()
    {
        ApplyAttackHealthCost();
        FireRewardExtraAttacks();
    }

    // 根据额外攻击规则发射额外箭矢。
    private void FireRewardExtraAttacks()
    {
        if (!RewardBonusManager.Instance)
        {
            return;
        }

        IReadOnlyList<DefenseExtraAttackRule> extraAttackRuleList = RewardBonusManager.Instance.DefenseExtraAttackRuleList;
        EnsureExtraAttackCounterCount(extraAttackRuleList.Count);

        for (int i = 0; i < extraAttackRuleList.Count; i++)
        {
            DefenseExtraAttackRule extraAttackRule = extraAttackRuleList[i];
            _extraAttackCounterList[i]++;

            if (_extraAttackCounterList[i] < extraAttackRule.TriggerAttackCount)
            {
                continue;
            }

            _extraAttackCounterList[i] = 0;
            FireExtraAttackArrows(extraAttackRule.ExtraAttackCount);
        }
    }

    // 确保额外攻击计数器数量和规则数量一致。
    private void EnsureExtraAttackCounterCount(int targetCount)
    {
        while (_extraAttackCounterList.Count < targetCount)
        {
            _extraAttackCounterList.Add(0);
        }

        while (_extraAttackCounterList.Count > targetCount)
        {
            _extraAttackCounterList.RemoveAt(_extraAttackCounterList.Count - 1);
        }
    }

    // 发射指定数量的额外普通箭。
    private void FireExtraAttackArrows(int extraAttackCount)
    {
        for (int i = 0; i < extraAttackCount; i++)
        {
            FireFromSpawnPoint(arrowSpawnPoint, TargetLane.Any);
        }
    }

    // 应用超载核心的攻击损血。
    private void ApplyAttackHealthCost()
    {
        if (!_healthSystem || !RewardBonusManager.Instance)
        {
            return;
        }

        int healthCost = RewardBonusManager.Instance.DefenseAttackHealthCost;
        if (healthCost > 0)
        {
            _healthSystem.LoseHealth(healthCost);
        }
    }

    // 侦测范围内最近的有效敌人并设置为当前攻击目标。
    private void DetectEnemy()
    {
        Transform detectionPoint = detectPoint ? detectPoint : transform;
        _detectedEnemyCount = Physics2D.OverlapCircleNonAlloc(detectionPoint.position, detectRadius, _detectResults, detectLayer);

        if (_detectedEnemyCount == 0)
        {
            _hasEnemy = false;
            _currentTargetEnemy = null;
            return;
        }

        _currentTargetEnemy = FindClosestTarget(TargetLane.Any);
        _hasEnemy = _currentTargetEnemy;
    }

    // 使用指定发射点向目标区域内的最近敌人射箭。
    private bool FireFromSpawnPoint(Transform spawnPoint, TargetLane targetLane)
    {
        if (!spawnPoint)
        {
            return false;
        }

        Enemy target = FindPreferredTarget(targetLane);
        if (!IsTargetValid(target))
        {
            return false;
        }

        Arrow arrow = PoolManager.Instance
            ? PoolManager.Instance.Spawn(arrowPrefab, spawnPoint.position, Quaternion.identity)
            : Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity);

        if (!arrow)
        {
            return false;
        }

        arrow.SetVisualEffect(GetArrowMaterialForCurrentStarLevel(), ShouldEnableArrowTrail());
        arrow.SetDamage(GetCurrentAttackDamage());
        arrow.SetAttackContext(this, GetArmorIgnorePercent(), ShouldUseExplosiveArrow(), GetExplosionRadius(), GetExplosionDamageMultiplier());
        arrow.SetTarget(target);
        return true;
    }

    // 发射缓存过期时立即重新检测一次，避免本次攻击 tick 因旧目标失效而空等。
    private bool TryRefreshTargetsForImmediateShot()
    {
        DetectEnemy();
        return _hasEnemy;
    }

    // 使用三星解锁的额外发射点进行分区射击。
    private bool FireSuperArrowSpawnPoint(int index, TargetLane targetLane)
    {
        if (superArrowSpawnPoints == null || index < 0 || index >= superArrowSpawnPoints.Length)
        {
            return false;
        }

        return FireFromSpawnPoint(superArrowSpawnPoints[index], targetLane);
    }

    // 优先查找指定区域目标，区域内没有敌人时回退到全局索敌。
    private Enemy FindPreferredTarget(TargetLane targetLane)
    {
        Enemy target = FindClosestTarget(targetLane);
        if (IsTargetValid(target) || targetLane == TargetLane.Any)
        {
            return target;
        }

        return FindClosestTarget(TargetLane.Any);
    }

    // 当前攻击目标失效时立即从缓存中重锁，缓存无效时再做一次实时检测。
    private void RefreshCurrentTargetIfNeeded()
    {
        if (IsTargetValid(_currentTargetEnemy))
        {
            return;
        }

        _currentTargetEnemy = FindClosestTarget(TargetLane.Any);
        if (IsTargetValid(_currentTargetEnemy))
        {
            _hasEnemy = true;
            return;
        }

        DetectEnemy();
    }

    // 根据当前星级获取箭头应该使用的视觉材质。
    private Material GetArrowMaterialForCurrentStarLevel()
    {
        if (_currentStarLevel >= STAR_LEVEL_THREE && starThreeArrowMaterial)
        {
            return starThreeArrowMaterial;
        }

        if (_currentStarLevel >= STAR_LEVEL_TWO && starTwoArrowMaterial)
        {
            return starTwoArrowMaterial;
        }

        return starOneArrowMaterial;
    }

    // 判断当前星级是否启用箭头拖尾效果。
    private bool ShouldEnableArrowTrail()
    {
        return _currentStarLevel >= STAR_LEVEL_TWO;
    }

    // 查找指定区域内距离防御塔最近的有效敌人。
    private Enemy FindClosestTarget(TargetLane targetLane)
    {
        Enemy closestTarget = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < _detectedEnemyCount; i++)
        {
            Collider2D result = _detectResults[i];
            if (!result || !result.TryGetComponent(out Enemy enemy) || !IsTargetValid(enemy) || !IsEnemyInTargetLane(enemy, targetLane))
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude(enemy.transform.position - transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = enemy;
            }
        }

        return closestTarget;
    }

    // 判断敌人是否属于指定的上下半区目标范围。
    private bool IsEnemyInTargetLane(Enemy enemy, TargetLane targetLane)
    {
        switch (targetLane)
        {
            case TargetLane.Upper:
                return enemy.transform.position.y >= transform.position.y;
            case TargetLane.Lower:
                return enemy.transform.position.y < transform.position.y;
            default:
                return true;
        }
    }

    // 根据升星倍率和全局奖励倍率刷新防御塔战斗属性。
    private void RefreshRewardBonuses()
    {
        attackDamage = GetCurrentAttackDamage(false);
        arrowGenerateRate = GetCurrentArrowGenerateRate();

        float rewardDetectRadiusMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseDetectRadiusMultiplier
            : 1f;
        detectRadius = Mathf.Max(0.01f, _baseDetectRadius * _upgradeDetectRadiusMultiplier * rewardDetectRadiusMultiplier);
    }

    // 计算当前攻击伤害。
    private int GetCurrentAttackDamage(bool includeRandomDoubleDamage = true)
    {
        float rewardAttackDamageMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseAttackDamageMultiplier
            : 1f;
        float threeStarResonanceMultiplier = GetThreeStarResonanceMultiplier();
        float finalDefenseMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.GetFinalDefenseAttackDamageMultiplier()
            : 1f;
        float damage = _baseAttackDamage * _upgradeAttackDamageMultiplier * rewardAttackDamageMultiplier * threeStarResonanceMultiplier * finalDefenseMultiplier;

        if (includeRandomDoubleDamage && ShouldApplyDoubleDamage())
        {
            damage *= 2f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    // 计算当前攻击间隔。
    private float GetCurrentArrowGenerateRate()
    {
        float rewardAttackIntervalMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseAttackIntervalMultiplier
            : 1f;
        float overloadAttackIntervalMultiplier = RewardBonusManager.Instance
            ? RewardBonusManager.Instance.DefenseOverloadAttackIntervalMultiplier
            : 1f;
        float linkedAttackIntervalMultiplier = ShouldApplyLinkedAttackSpeed()
            ? RewardBonusManager.Instance.DefenseLinkedAttackIntervalMultiplier
            : 1f;

        return Mathf.Max(MIN_ARROW_GENERATE_RATE, _baseArrowGenerateRate * _upgradeAttackIntervalMultiplier * rewardAttackIntervalMultiplier * overloadAttackIntervalMultiplier * linkedAttackIntervalMultiplier);
    }

    // 计算星级共鸣提供的攻击力倍率。
    private float GetThreeStarResonanceMultiplier()
    {
        if (!RewardBonusManager.Instance)
        {
            return 1f;
        }

        int threeStarCount = DefenseTowerRegistry.GetThreeStarDefenseCount();
        return 1f + threeStarCount * RewardBonusManager.Instance.DefenseAttackDamagePerThreeStarTower;
    }

    // 判断防线联动攻速是否生效。
    private bool ShouldApplyLinkedAttackSpeed()
    {
        return RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseLinkRadius > 0f
               && DefenseTowerRegistry.HasNearbyDefenseTower(this, RewardBonusManager.Instance.DefenseLinkRadius);
    }

    // 判断本次攻击是否触发双倍伤害。
    private bool ShouldApplyDoubleDamage()
    {
        return RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseDoubleDamageChance > 0f
               && Random.value < RewardBonusManager.Instance.DefenseDoubleDamageChance;
    }

    // 获取本次箭矢护甲穿透比例。
    private float GetArmorIgnorePercent()
    {
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseArmorIgnorePercent : 0f;
    }

    // 判断本次箭矢是否启用三星爆裂箭。
    private bool ShouldUseExplosiveArrow()
    {
        return _currentStarLevel >= STAR_LEVEL_THREE
               && RewardBonusManager.Instance
               && RewardBonusManager.Instance.DefenseThreeStarExplosiveArrowEnabled;
    }

    // 获取爆裂箭爆炸半径。
    private float GetExplosionRadius()
    {
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseExplosionRadius : 0f;
    }

    // 获取爆裂箭范围伤害倍率。
    private float GetExplosionDamageMultiplier()
    {
        return RewardBonusManager.Instance ? RewardBonusManager.Instance.DefenseExplosionDamageMultiplier : 0f;
    }

    // 禁用防御塔时停止周期侦测并取消注册。
    private void OnDisable()
    {
        RewardBonusManager.OnRewardBonusChanged -= RefreshRewardBonuses;
        DefenseTowerRegistry.UnregisterDefenseSystem(this);
        CancelInvoke();
        _hasEnemy = false;
        _currentTargetEnemy = null;
    }

    // 判断敌人是否仍然可以被防御塔攻击。
    private bool IsTargetValid(Enemy enemy)
    {
        return enemy && enemy.gameObject.activeInHierarchy && enemy.IsAlive;
    }
}
