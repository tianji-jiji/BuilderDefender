# Rewards Layered Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 Rewards 重构为 Data、Domain、Application、Integration、Presentation 和 Editor 分层，同时保留现有奖励资产与游戏行为。

**Architecture:** ScriptableObject 继续保存卡牌配置和效果策略引用；纯 C# Domain 负责抽卡、历史和 Tower 奖励规则；Application 执行效果并分发运行时触发；Integration 连接 Unity 场景、建筑、波次和随机数；Presentation 只负责奖励 UI。Tower 场景操作通过 `ITowerRewardWorld` 隔离，Rewards 不再依赖 `BuildingUpgradeButton`。

**Tech Stack:** Unity 6.3 LTS (`6000.3.14f1`)、C#、UGUI、NUnit、Unity Test Framework 1.6.0。

---

## Execution Constraints

- 不执行 `git add`、`git commit`、`git mv` 或任何其他 Git 状态变更。用户已明确拒绝 Git 操作。
- 所有 Unity 脚本移动必须同时移动对应 `.meta` 文件。
- 每个任务结束时保持项目可编译；不要累计跨任务编译错误。
- 重命名序列化字段时使用 `FormerlySerializedAs`。
- 不创建 Home 或 Economy 占位代码。
- 不添加 asmdef 或命名空间。
- 不保留旧 API 包装器。
- `BuilderDefender.EditModeTests` 无法直接引用预定义 `Assembly-CSharp`；本计划中的 EditMode 测试代码示例若直接写出生产类型，实施时必须改为通过 `Type.GetType("类型名, Assembly-CSharp")` 反射访问，除非用户另行批准生产程序集拆分。

## Verification Commands

项目关闭其他 Unity Editor 实例后，从 PowerShell 执行：

```powershell
$unity = 'D:\UnityEditor\6000.3.14f1\Editor\Unity.exe'
$project = 'D:\UnityProject\Unity_Demo\BuilderDefender'
& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode `
  -testResults "$project\Temp\EditModeResults.xml" `
  -logFile "$project\Temp\EditModeTests.log" -quit
```

预期：进程退出码为 `0`，`Temp/EditModeResults.xml` 中 `failed="0"`。

定向测试在命令中增加：

```powershell
-testFilter '完整测试类名或测试方法名'
```

只检查编译时执行：

```powershell
$unity = 'D:\UnityEditor\6000.3.14f1\Editor\Unity.exe'
$project = 'D:\UnityProject\Unity_Demo\BuilderDefender'
& $unity -batchmode -nographics -projectPath $project `
  -logFile "$project\Temp\Compile.log" -quit
```

预期：退出码为 `0`，日志中没有 `error CS` 和 Missing Script。

## Target File Map

### Data

- `Assets/Scripts/Rewards/Data/RewardCardSo.cs`
- `Assets/Scripts/Rewards/Data/RewardCardPoolSo.cs`
- `Assets/Scripts/Rewards/Data/RewardCardEffectConfig.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectParameterConfig.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectParameterIds.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectDefinitionSo.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectParameterDisplayDefinition.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectDisplayImpact.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectValueFormat.cs`
- `Assets/Scripts/Rewards/Data/RewardEffectAutoImpactRule.cs`
- `Assets/Scripts/Rewards/Data/RewardCardRarity.cs`
- `Assets/Scripts/Rewards/Data/RewardCardCategory.cs`

### Domain

- `Assets/Scripts/Rewards/Domain/Cards/IRewardRandom.cs`
- `Assets/Scripts/Rewards/Domain/Cards/RewardCardDrawContext.cs`
- `Assets/Scripts/Rewards/Domain/Cards/RewardCardDrawService.cs`
- `Assets/Scripts/Rewards/Domain/Cards/RewardCardRecord.cs`
- `Assets/Scripts/Rewards/Domain/Cards/RewardHistory.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerRewardState.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerExtraAttackRule.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerAttackHealthCostRule.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerBuildCostCalculator.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerPowerCalculator.cs`
- `Assets/Scripts/Rewards/Domain/Tower/TowerPowerSnapshot.cs`

### Application

- `Assets/Scripts/Rewards/Application/Effects/RewardEffectApplierSo.cs`
- `Assets/Scripts/Rewards/Application/Effects/RewardApplyContext.cs`
- `Assets/Scripts/Rewards/Application/Effects/RewardEffectApplicationService.cs`
- `Assets/Scripts/Rewards/Application/Effects/RewardEffectParameterReader.cs`
- `Assets/Scripts/Rewards/Application/RewardAppliedContext.cs`
- `Assets/Scripts/Rewards/Application/Tower/ITowerRewardWorld.cs`
- `Assets/Scripts/Rewards/Application/Tower/TowerRewardRuntime.cs`
- `Assets/Scripts/Rewards/Application/Tower/Effects/Base/TowerRewardApplierSo.cs`
- `Assets/Scripts/Rewards/Application/Tower/Effects/*.cs`
- `Assets/Scripts/Rewards/Application/Tower/Runtime/*.cs`

### Integration

- `Assets/Scripts/Rewards/Integration/RewardRuntimeCoordinator.cs`
- `Assets/Scripts/Rewards/Integration/UnityRewardRandom.cs`
- `Assets/Scripts/Rewards/Integration/Tower/TowerRewardWorld.cs`
- `Assets/Scripts/Buildings/Runtime/BuildingRuntimeRegistry.cs`
- `Assets/Scripts/Buildings/Runtime/Tower/TowerUpgradeController.cs`

### Presentation And Editor

- `Assets/Scripts/Rewards/Presentation/Cards/*.cs`
- `Assets/Scripts/Rewards/Presentation/Tower/TowerRewardSummaryFormatter.cs`
- `Assets/Scripts/Rewards/Editor/RewardCardPoolSoEditor.cs`
- `Assets/Scripts/Rewards/Editor/RewardCardIdAutoGenerator.cs`
- `Assets/Scripts/Rewards/Editor/RewardCardSoEditor.cs`
- `Assets/Scripts/Rewards/Editor/RewardEffectDefinitionSoEditor.cs`
- `Assets/Scripts/Rewards/Editor/RewardEffectAuthoringPresets.cs`

## Task 1: Establish Current-Type Characterization Tests

**Files:**

- Create: `Assets/Tests/EditMode/Rewards/Application/TowerRewardTriggerCharacterizationTests.cs`
- Create: `Assets/Tests/EditMode/Rewards/TestGameObjectScope.cs`
- Keep temporarily: `Assets/Tests/EditMode/DefenseTowerRewardTriggerTests.cs`

- [x] **Step 1: Create a deterministic GameObject cleanup helper**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 测试 GameObject 生命周期范围，确保测试结束时销毁创建的对象。
/// </summary>
public sealed class TestGameObjectScope : IDisposable
{
    private readonly List<GameObject> _gameObjectList = new();

    // 创建默认禁用的测试对象，避免组件生命周期自动访问场景单例。
    public GameObject Create(string objectName)
    {
        GameObject gameObject = new(objectName);
        gameObject.SetActive(false);
        _gameObjectList.Add(gameObject);
        return gameObject;
    }

    // 销毁当前范围创建的全部测试对象。
    public void Dispose()
    {
        foreach (GameObject gameObject in _gameObjectList)
        {
            if (gameObject)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
```

- [x] **Step 2: Add current-type characterization tests through the existing reflection boundary**

```csharp
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tower 奖励触发器现有行为测试，为后续重命名和移动提供保护。
/// </summary>
public class TowerRewardTriggerCharacterizationTests
{
    // 验证单个效果的计数按来源 Tower 隔离。
    [Test]
    public void RuntimeStateTracksCounterPerTower()
    {
        using TestGameObjectScope scope = new();
        TowerCombatSystem firstTower = scope.Create("FirstTower").AddComponent<TowerCombatSystem>();
        TowerCombatSystem secondTower = scope.Create("SecondTower").AddComponent<TowerCombatSystem>();
        TowerRewardRuntimeState state = new(null);

        Assert.AreEqual(1, state.IncrementCounter(firstTower, "Attack"));
        Assert.AreEqual(2, state.IncrementCounter(firstTower, "Attack"));
        Assert.AreEqual(1, state.IncrementCounter(secondTower, "Attack"));
    }

    // 验证攻击完成上下文累计额外攻击次数。
    [Test]
    public void AttackCompletedContextAccumulatesExtraAttacks()
    {
        TowerAttackCompletedContext context = new(null, null);

        context.RequestExtraAttack(1);
        context.RequestExtraAttack(2);

        Assert.AreEqual(3, context.ExtraAttackCount);
    }

    // 验证清理指定 Tower 不会影响其他 Tower 的计数。
    [Test]
    public void RuntimeStateClearCountersDoesNotAffectOtherTower()
    {
        using TestGameObjectScope scope = new();
        TowerCombatSystem firstTower = scope.Create("FirstTower").AddComponent<TowerCombatSystem>();
        TowerCombatSystem secondTower = scope.Create("SecondTower").AddComponent<TowerCombatSystem>();
        TowerRewardRuntimeState state = new(null);
        state.IncrementCounter(firstTower, "Kill");
        state.IncrementCounter(secondTower, "Kill");

        state.ClearCounters(firstTower);

        Assert.AreEqual(1, state.IncrementCounter(firstTower, "Kill"));
        Assert.AreEqual(2, state.IncrementCounter(secondTower, "Kill"));
    }
}
```

- [x] **Step 3: Run only the new characterization tests**

Run the EditMode command with:

```powershell
-testFilter 'TowerRewardTriggerCharacterizationTests'
```

Expected: three tests pass. The obsolete reflection test file is intentionally excluded from this run.

- [x] **Step 4: Record the existing public behavior that must remain stable**

Confirm from test names and production call sites that the migration preserves:

- per-Tower counters;
- extra attack accumulation;
- arrow modifier composition;
- kill-growth upgrades;
- wave-end healing;
- source cleanup on disable.

## Task 2: Split Card Pool Data From Draw Logic

**Files:**

- Rename with `.meta`: `Assets/Scripts/Rewards/Data/RewardCardDrawPoolSo.cs` → `Assets/Scripts/Rewards/Data/RewardCardPoolSo.cs`
- Create: `Assets/Scripts/Rewards/Domain/Cards/IRewardRandom.cs`
- Create: `Assets/Scripts/Rewards/Domain/Cards/RewardCardDrawService.cs`
- Move with `.meta`: `Assets/Scripts/Rewards/Runtime/RewardCardDrawContext.cs` → `Assets/Scripts/Rewards/Domain/Cards/RewardCardDrawContext.cs`
- Create: `Assets/Scripts/Rewards/Integration/UnityRewardRandom.cs`
- Rename with `.meta`: `Assets/Scripts/Editor/RewardCardDrawPoolSoEditor.cs` → `Assets/Scripts/Rewards/Editor/RewardCardPoolSoEditor.cs`
- Create: `Assets/Tests/EditMode/Rewards/Domain/RewardCardDrawServiceTests.cs`

- [x] **Step 1: Write draw-service tests before moving the algorithm**

```csharp
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 奖励卡抽取规则测试。
/// </summary>
public class RewardCardDrawServiceTests
{
    // 验证抽卡过滤未到开放波次的卡牌。
    [Test]
    public void DrawFiltersCardsAboveCurrentWave()
    {
        RewardCardSo availableCard = CreateCard("Available", 1, 1, 0);
        RewardCardSo lockedCard = CreateCard("Locked", 1, 5, 0);
        RewardCardPoolSo pool = CreatePool(new[] { availableCard, lockedCard }, 1, false);
        RewardCardDrawService service = new(new FirstIndexRandom());

        IReadOnlyList<RewardCardSo> resultList = service.Draw(pool, RewardCardDrawContext.Default(1));

        Assert.AreEqual(1, resultList.Count);
        Assert.AreSame(availableCard, resultList[0]);
        Object.DestroyImmediate(pool);
        Object.DestroyImmediate(availableCard.CardPrefab);
        Object.DestroyImmediate(lockedCard.CardPrefab);
        Object.DestroyImmediate(availableCard);
        Object.DestroyImmediate(lockedCard);
    }

    // 创建指定抽取配置的测试卡牌。
    private static RewardCardSo CreateCard(string cardName, int weight, int minWaveIndex, int maxPickCount)
    {
        RewardCardSo card = ScriptableObject.CreateInstance<RewardCardSo>();
        SetField(card, "cardName", cardName);
        SetField(card, "weight", weight);
        SetField(card, "minWaveIndex", minWaveIndex);
        SetField(card, "maxPickCount", maxPickCount);
        SetField(card, "cardPrefab", new GameObject($"{cardName}Prefab"));
        return card;
    }

    // 创建测试卡池。
    private static RewardCardPoolSo CreatePool(IReadOnlyList<RewardCardSo> cardList, int choiceCount, bool allowDuplicate)
    {
        RewardCardPoolSo pool = ScriptableObject.CreateInstance<RewardCardPoolSo>();
        SetField(pool, "rewardCardList", new List<RewardCardSo>(cardList));
        SetField(pool, "choiceCount", choiceCount);
        SetField(pool, "allowDuplicate", allowDuplicate);
        return pool;
    }

    // 设置测试资产的私有序列化字段。
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    /// <summary>
    /// 始终选择首个权重区间的随机数实现。
    /// </summary>
    private sealed class FirstIndexRandom : IRewardRandom
    {
        // 返回最小值以稳定选择首个候选项。
        public int Range(int minInclusive, int maxExclusive)
        {
            return minInclusive;
        }
    }
}
```

- [x] **Step 2: Run the test and verify it fails for missing new types**

Run with:

```powershell
-testFilter 'RewardCardDrawServiceTests'
```

Expected: compile fails because `RewardCardPoolSo`, `RewardCardDrawService` and `IRewardRandom` do not exist.

- [x] **Step 3: Rename the pool and expose read-only configuration**

`RewardCardPoolSo` must contain this public surface and no draw loop:

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 奖励卡池数据资产，保存候选卡和抽取配置。
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/RewardCard/RewardCardPoolSo")]
public class RewardCardPoolSo : ScriptableObject
{
    private const int DEFAULT_CHOICE_COUNT = 3;

    [SerializeField] private List<RewardCardSo> rewardCardList = new();
    [SerializeField] private int choiceCount = DEFAULT_CHOICE_COUNT;
    [SerializeField] private bool allowDuplicate;

    public IReadOnlyList<RewardCardSo> RewardCardList => rewardCardList;
    public int ChoiceCount => Mathf.Max(1, choiceCount);
    public bool AllowDuplicate => allowDuplicate;
}
```

- [x] **Step 4: Implement deterministic draw dependencies**

```csharp
/// <summary>
/// 奖励抽取随机数来源。
/// </summary>
public interface IRewardRandom
{
    int Range(int minInclusive, int maxExclusive);
}
```

```csharp
using UnityEngine;

/// <summary>
/// 使用 Unity 随机数生成奖励抽取结果。
/// </summary>
public sealed class UnityRewardRandom : IRewardRandom
{
    // 返回指定左闭右开区间内的随机整数。
    public int Range(int minInclusive, int maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
}
```

- [x] **Step 5: Move the existing filter and weighted-pick algorithm into RewardCardDrawService**

Implement the complete service:

```csharp
using System.Collections.Generic;

/// <summary>
/// 奖励卡抽取服务，负责候选过滤、权重抽取和重复控制。
/// </summary>
public sealed class RewardCardDrawService
{
    private readonly IRewardRandom _random;

    public RewardCardDrawService(IRewardRandom random)
    {
        _random = random;
    }

    // 根据卡池和当前抽取上下文生成候选卡。
    public IReadOnlyList<RewardCardSo> Draw(RewardCardPoolSo pool, RewardCardDrawContext context)
    {
        if (!pool || _random == null)
        {
            return System.Array.Empty<RewardCardSo>();
        }

        return Draw(pool.RewardCardList, pool.ChoiceCount, pool.AllowDuplicate, context);
    }

    // 执行候选过滤和权重抽取。
    private IReadOnlyList<RewardCardSo> Draw(
        IReadOnlyList<RewardCardSo> sourceCardList,
        int choiceCount,
        bool allowDuplicate,
        RewardCardDrawContext context)
    {
        List<RewardCardSo> resultList = new();
        List<RewardCardSo> availableCardList = BuildAvailableCardList(sourceCardList, context);

        while (resultList.Count < choiceCount && availableCardList.Count > 0)
        {
            RewardCardSo selectedCard = PickWeightedCard(availableCardList);
            if (!selectedCard)
            {
                break;
            }

            resultList.Add(selectedCard);
            if (!allowDuplicate)
            {
                availableCardList.Remove(selectedCard);
            }
        }

        return resultList;
    }

    // 收集当前上下文允许参与抽取的卡牌。
    private static List<RewardCardSo> BuildAvailableCardList(
        IReadOnlyList<RewardCardSo> sourceCardList,
        RewardCardDrawContext context)
    {
        List<RewardCardSo> availableCardList = new();
        if (sourceCardList == null)
        {
            return availableCardList;
        }

        foreach (RewardCardSo rewardCard in sourceCardList)
        {
            if (IsCardAvailable(rewardCard, context))
            {
                availableCardList.Add(rewardCard);
            }
        }

        return availableCardList;
    }

    // 按权重选择一张候选卡。
    private RewardCardSo PickWeightedCard(IReadOnlyList<RewardCardSo> availableCardList)
    {
        int totalWeight = 0;
        foreach (RewardCardSo rewardCard in availableCardList)
        {
            totalWeight += GetDrawWeight(rewardCard);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = _random.Range(0, totalWeight);
        int accumulatedWeight = 0;
        foreach (RewardCardSo rewardCard in availableCardList)
        {
            accumulatedWeight += GetDrawWeight(rewardCard);
            if (roll < accumulatedWeight)
            {
                return rewardCard;
            }
        }

        return null;
    }

    // 判断卡牌是否满足当前抽取条件。
    private static bool IsCardAvailable(RewardCardSo rewardCard, RewardCardDrawContext context)
    {
        return rewardCard
               && rewardCard.Weight > 0
               && rewardCard.CardPrefab
               && context.CurrentWaveIndex >= rewardCard.MinWaveIndex
               && context.GetSelectedCount(rewardCard) < rewardCard.MaxPickCount;
    }

    // 返回用于抽取的非负权重。
    private static int GetDrawWeight(RewardCardSo rewardCard)
    {
        return rewardCard ? rewardCard.Weight : 0;
    }
}
```

Do not retain `DrawCards` on `RewardCardPoolSo`.

- [x] **Step 6: Update the custom inspector type and serialized references**

Change:

```csharp
[CustomEditor(typeof(RewardCardPoolSo))]
public class RewardCardPoolSoEditor : Editor
```

Update serialized field types from `RewardCardDrawPoolSo` to `RewardCardPoolSo` without renaming the field itself.

- [x] **Step 7: Run draw tests and compile**

Expected: `RewardCardDrawServiceTests` passes and Unity compile exits `0`.

## Task 3: Replace MonoBehaviour Reward History With Domain State

**Files:**

- Create: `Assets/Scripts/Rewards/Domain/Cards/RewardCardRecord.cs`
- Create: `Assets/Scripts/Rewards/Domain/Cards/RewardHistory.cs`
- Create: `Assets/Scripts/Rewards/Application/RewardAppliedContext.cs`
- Keep temporarily until Task 10 migrates all callers: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredContext.cs`
- Keep temporarily until Task 10 migrates all callers: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredHistory.cs`
- Keep temporarily until Task 10 migrates all callers: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredRecord.cs`
- Create: `Assets/Tests/EditMode/Rewards/Domain/RewardHistoryTests.cs`

- [x] **Step 1: Write history tests**

```csharp
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 奖励获得历史领域测试。
/// </summary>
public class RewardHistoryTests
{
    // 验证同一卡牌重复获得时叠加现有记录。
    [Test]
    public void RecordSameCardIncrementsStackCount()
    {
        RewardCardSo card = ScriptableObject.CreateInstance<RewardCardSo>();
        RewardHistory history = new();

        history.Record(card, 3);
        RewardCardRecord record = history.Record(card, 4);

        Assert.AreEqual(1, history.RecordList.Count);
        Assert.AreEqual(2, record.StackCount);
        Assert.AreEqual(3, record.AcquiredWaveIndex);
        Object.DestroyImmediate(card);
    }

    // 验证清空历史后记录和总次数归零。
    [Test]
    public void ClearRemovesAllRecords()
    {
        RewardCardSo card = ScriptableObject.CreateInstance<RewardCardSo>();
        RewardHistory history = new();
        history.Record(card, 1);

        history.Clear();

        Assert.AreEqual(0, history.RecordList.Count);
        Assert.AreEqual(0, history.TotalCardCount);
        Object.DestroyImmediate(card);
    }
}
```

- [x] **Step 2: Verify the tests fail for missing domain types**

Run with `-testFilter 'RewardHistoryTests'`.

Expected: compile fails because `RewardHistory` and `RewardCardRecord` do not exist.

- [x] **Step 3: Implement RewardCardRecord**

```csharp
/// <summary>
/// 单张奖励卡在本局中的获得记录。
/// </summary>
public sealed class RewardCardRecord
{
    public RewardCardSo RewardCard { get; }
    public int AcquiredWaveIndex { get; }
    public int StackCount { get; private set; } = 1;

    public RewardCardRecord(RewardCardSo rewardCard, int acquiredWaveIndex)
    {
        RewardCard = rewardCard;
        AcquiredWaveIndex = acquiredWaveIndex;
    }

    // 增加当前卡牌的叠加次数。
    public void AddStack()
    {
        StackCount++;
    }
}
```

- [x] **Step 4: Implement RewardHistory without Unity lifecycle or formatting**

```csharp
using System.Collections.Generic;

/// <summary>
/// 本局奖励卡获得历史，负责记录、叠加和抽卡计数查询。
/// </summary>
public sealed class RewardHistory
{
    private readonly List<RewardCardRecord> _recordList = new();
    private readonly Dictionary<string, RewardCardRecord> _recordByCardIdDic = new();

    public IReadOnlyList<RewardCardRecord> RecordList => _recordList;

    public int TotalCardCount
    {
        get
        {
            int totalCardCount = 0;
            foreach (RewardCardRecord record in _recordList)
            {
                totalCardCount += record.StackCount;
            }

            return totalCardCount;
        }
    }

    // 记录一张奖励卡并返回对应的累计记录。
    public RewardCardRecord Record(RewardCardSo rewardCard, int waveIndex)
    {
        if (!rewardCard)
        {
            return null;
        }

        string cardId = GetStableCardId(rewardCard);
        if (_recordByCardIdDic.TryGetValue(cardId, out RewardCardRecord existingRecord))
        {
            existingRecord.AddStack();
            return existingRecord;
        }

        RewardCardRecord newRecord = new(rewardCard, waveIndex);
        _recordByCardIdDic.Add(cardId, newRecord);
        _recordList.Add(newRecord);
        return newRecord;
    }

    // 根据当前历史构建奖励抽取上下文。
    public RewardCardDrawContext BuildDrawContext(int currentWaveIndex)
    {
        Dictionary<string, int> selectedCardCountDic = new();
        foreach (KeyValuePair<string, RewardCardRecord> recordPair in _recordByCardIdDic)
        {
            selectedCardCountDic[recordPair.Key] = recordPair.Value.StackCount;
        }

        return new RewardCardDrawContext(currentWaveIndex, selectedCardCountDic);
    }

    // 清空本局全部奖励记录。
    public void Clear()
    {
        _recordList.Clear();
        _recordByCardIdDic.Clear();
    }

    // 获取卡牌稳定 ID，缺失时使用资产名。
    private static string GetStableCardId(RewardCardSo rewardCard)
    {
        return string.IsNullOrWhiteSpace(rewardCard.CardId)
            ? rewardCard.name
            : rewardCard.CardId;
    }
}
```

- [x] **Step 5: Create the replacement application event payload**

```csharp
using System.Collections.Generic;

/// <summary>
/// 奖励卡成功应用后的只读通知数据。
/// </summary>
public sealed class RewardAppliedContext
{
    public RewardCardSo RewardCard { get; }
    public RewardCardRecord AppliedRecord { get; }
    public IReadOnlyList<RewardCardRecord> AllRecordList { get; }
    public int TotalCardCount { get; }

    public RewardAppliedContext(
        RewardCardSo rewardCard,
        RewardCardRecord appliedRecord,
        IReadOnlyList<RewardCardRecord> allRecordList,
        int totalCardCount)
    {
        RewardCard = rewardCard;
        AppliedRecord = appliedRecord;
        AllRecordList = allRecordList;
        TotalCardCount = totalCardCount;
    }
}
```

- [x] **Step 6: Run history tests**

Expected: all `RewardHistoryTests` pass.

## Task 4: Split Reward Data Types And Presentation Formatting

**Files:**

- Modify: `Assets/Scripts/Rewards/Data/RewardCardSo.cs`
- Modify: `Assets/Scripts/Rewards/Data/RewardCardEffectConfig.cs`
- Modify: `Assets/Scripts/Rewards/Data/RewardEffectDefinitionSo.cs`
- Create the split Data files listed in Target File Map.
- Extract: `RewardEffectParameterReader` from `Assets/Scripts/Rewards/Data/RewardCardEffectConfig.cs` into `Assets/Scripts/Rewards/Application/Effects/RewardEffectParameterReader.cs`
- Modify: `Assets/Scripts/UI/Rewards/RewardCardDescriptionFormatter.cs`
- Create: `Assets/Tests/EditMode/Rewards/Presentation/RewardCardDescriptionFormatterTests.cs`

- [x] **Step 1: Add a description-formatting characterization test**

Create a card effect with a definition template and one `Value` parameter using reflection, then assert:

```csharp
string description = RewardCardDescriptionFormatter.BuildDescriptionText(card);
Assert.That(description, Does.Contain("+10%"));
```

Run the test before moving behavior. Expected: pass against current implementation.

- [x] **Step 2: Split enums and serializable types into files matching their main type**

Move declarations without changing enum values or serialized field names:

```text
RewardCardRarity
RewardCardCategory
RewardEffectDisplayImpact
RewardEffectValueFormat
RewardEffectAutoImpactRule
RewardEffectParameterConfig
RewardEffectParameterDisplayDefinition
RewardEffectParameterIds
```

- [x] **Step 3: Make Data expose templates instead of formatting them**

`RewardEffectDefinitionSo` must expose read-only values:

```csharp
public string DisplayName => displayName;
public string DescriptionTemplate => string.IsNullOrWhiteSpace(descriptionTemplate)
    ? DEFAULT_DESCRIPTION_TEMPLATE
    : descriptionTemplate;
public RewardEffectAutoImpactRule AutoImpactRule => autoImpactRule;
public IReadOnlyList<RewardEffectParameterDisplayDefinition> ParameterDisplayDefinitionList
    => parameterDisplayDefinitionList;
public RewardEffectApplierSo Applier => applier;

// 查找指定参数的显示定义。
public RewardEffectParameterDisplayDefinition GetParameterDisplayDefinition(string parameterId)
{
    if (parameterDisplayDefinitionList == null)
    {
        return null;
    }

    foreach (RewardEffectParameterDisplayDefinition definition in parameterDisplayDefinitionList)
    {
        if (definition != null
            && string.Equals(definition.ParameterId, parameterId, StringComparison.Ordinal))
        {
            return definition;
        }
    }

    return null;
}
```

`RewardEffectParameterDisplayDefinition` exposes `ParameterId`, `TemplateToken`, `ValueFormat` and `AutoImpactRule`. Remove `BuildDescription`, display-impact resolution and token replacement from Data classes.

Replace `RewardCardDescriptionFormatter.BuildEffectDescription` and impact resolution with:

```csharp
// 构建单个奖励效果的描述文本。
private static string BuildEffectDescription(RewardCardEffectConfig cardEffectConfig)
{
    Dictionary<string, string> parameterTextDic = BuildParameterTextDic(cardEffectConfig);
    RewardEffectDefinitionSo definition = cardEffectConfig.EffectDefinition;
    if (!definition)
    {
        return parameterTextDic.TryGetValue(RewardEffectParameterIds.VALUE, out string valueText)
            ? valueText
            : string.Empty;
    }

    string description = definition.DescriptionTemplate.Replace("{displayName}", definition.DisplayName);
    foreach (KeyValuePair<string, string> parameterTextPair in parameterTextDic)
    {
        RewardEffectParameterDisplayDefinition displayDefinition =
            definition.GetParameterDisplayDefinition(parameterTextPair.Key);
        string token = displayDefinition != null
            ? displayDefinition.TemplateToken
            : $"{{{parameterTextPair.Key}}}";
        description = description.Replace(token, parameterTextPair.Value);

        if (parameterTextPair.Key == RewardEffectParameterIds.VALUE)
        {
            description = description.Replace("{value}", parameterTextPair.Value);
        }
    }

    return description;
}

// 解析参数显示倾向。
private static RewardEffectDisplayImpact ResolveDisplayImpact(
    RewardEffectParameterDisplayDefinition displayDefinition,
    RewardEffectAutoImpactRule fallbackRule,
    float value,
    RewardEffectDisplayImpact displayImpactOverride)
{
    if (displayImpactOverride != RewardEffectDisplayImpact.Auto)
    {
        return displayImpactOverride;
    }

    if (Mathf.Approximately(value, 0f))
    {
        return RewardEffectDisplayImpact.Neutral;
    }

    RewardEffectAutoImpactRule rule = displayDefinition?.AutoImpactRule ?? fallbackRule;
    return rule switch
    {
        RewardEffectAutoImpactRule.GreaterThanZeroIsPositive => value > 0f
            ? RewardEffectDisplayImpact.Positive
            : RewardEffectDisplayImpact.Negative,
        RewardEffectAutoImpactRule.LessThanZeroIsPositive => value < 0f
            ? RewardEffectDisplayImpact.Positive
            : RewardEffectDisplayImpact.Negative,
        RewardEffectAutoImpactRule.AlwaysPositive => RewardEffectDisplayImpact.Positive,
        RewardEffectAutoImpactRule.AlwaysNegative => RewardEffectDisplayImpact.Negative,
        _ => RewardEffectDisplayImpact.Neutral
    };
}
```

Replace the value overload with:

```csharp
// 构建指定参数 ID 和值的富文本。
private static string BuildColoredParameterText(
    RewardCardEffectConfig cardEffectConfig,
    string parameterId,
    float value,
    RewardEffectDisplayImpact displayImpactOverride)
{
    RewardEffectDefinitionSo definition = cardEffectConfig.EffectDefinition;
    RewardEffectParameterDisplayDefinition displayDefinition =
        definition ? definition.GetParameterDisplayDefinition(parameterId) : null;
    RewardEffectValueFormat valueFormat = displayDefinition?.ValueFormat
        ?? RewardEffectValueFormat.PercentWithSign;
    RewardEffectAutoImpactRule fallbackRule = definition
        ? definition.AutoImpactRule
        : RewardEffectAutoImpactRule.AlwaysNeutral;
    RewardEffectDisplayImpact displayImpact = ResolveDisplayImpact(
        displayDefinition,
        fallbackRule,
        value,
        displayImpactOverride);
    string valueText = FormatValue(value, valueFormat);
    return $"<color={GetImpactColorHex(displayImpact)}>{valueText}</color>";
}
```

Keep the existing color constants and value-formatting methods unchanged.

- [x] **Step 4: Move RewardEffectParameterReader into Application**

Keep public signatures unchanged so Applier migrations remain mechanical:

```csharp
public static float GetFloat(
    RewardCardEffectConfig cardEffectConfig,
    string parameterId,
    float defaultValue,
    bool logMissingWarning = false);

public static int GetInt(
    RewardCardEffectConfig cardEffectConfig,
    string parameterId,
    int defaultValue,
    bool logMissingWarning = false);
```

- [x] **Step 5: Run formatter tests and compile**

Expected: description output remains unchanged and compilation succeeds.

## Task 5: Make Tower Reward Domain Pure

**Files:**

- Rename with `.meta`: `Tower/ActiveRewards/TowerActiveRewards.cs` → `Domain/Tower/TowerRewardState.cs`
- Move with `.meta`: `TowerExtraAttackRule.cs`, `TowerAttackHealthCostRule.cs` → `Domain/Tower/`
- Move with `.meta`: `TowerBuildCostCalculator.cs`, `TowerPowerSnapshot.cs` → `Domain/Tower/`
- Rename with `.meta`: `TowerPowerEvaluator.cs` → `Domain/Tower/TowerPowerCalculator.cs`
- Create: `Assets/Tests/EditMode/Rewards/Domain/TowerRewardStateTests.cs`
- Create: `Assets/Tests/EditMode/Rewards/Domain/TowerPowerCalculatorTests.cs`

- [ ] **Step 1: Add state and calculator tests**

```csharp
using NUnit.Framework;

/// <summary>
/// Tower 奖励状态测试。
/// </summary>
public class TowerRewardStateTests
{
    // 验证攻击力奖励按加法累计并转换为倍率。
    [Test]
    public void AddAttackDamageBonusAccumulatesMultiplier()
    {
        TowerRewardState state = new();

        state.AddAttackDamageBonus(0.1f);
        state.AddAttackDamageBonus(0.2f);

        Assert.AreEqual(1.3f, state.AttackDamageMultiplier, 0.0001f);
    }

    // 验证最终防线计算通过外部基地生命值输入。
    [Test]
    public void GetFinalDefenseMultiplierUsesProvidedHomeHealth()
    {
        TowerRewardState state = new();
        state.AddFinalDefense(0.5f, 0.3f);

        Assert.AreEqual(1.5f, TowerPowerCalculator.GetFinalDefenseMultiplier(state, 0.2f));
        Assert.AreEqual(1f, TowerPowerCalculator.GetFinalDefenseMultiplier(state, 0.8f));
    }
}
```

- [ ] **Step 2: Verify tests fail for missing renamed types**

Run with `-testFilter 'TowerRewardStateTests'`.

- [ ] **Step 3: Rename TowerActiveRewards and remove world queries**

Apply this global type map:

```text
TowerActiveRewards       -> TowerRewardState
TowerPowerEvaluator      -> TowerPowerCalculator
```

Remove `IsFinalDefenseActive` and `GetFinalDefenseTowerAttackDamageMultiplier` from state. Move final-defense evaluation into `TowerPowerCalculator.GetFinalDefenseMultiplier(TowerRewardState state, float homeHealthNormalized)`.

Move extra-attack, critical and explosion power calculations from state into `TowerPowerCalculator` so `TowerRewardState` only stores, validates and aggregates values.

Expose the stored final-defense configuration without reading the world:

```csharp
public float FinalDefenseAttackDamageBonus => Mathf.Max(0f, _finalDefenseAttackDamageBonus);
public float FinalDefenseHomeHealthThreshold => Mathf.Clamp01(_finalDefenseHomeHealthThreshold);
```

- [ ] **Step 4: Update TowerBuildCostCalculator and TowerPowerCalculator signatures**

Use:

```csharp
public static int GetAdjustedAmount(
    BuildingSo buildingSo,
    ResourceCost resourceCost,
    TowerRewardState rewardState);

public static TowerPowerSnapshot CreateSnapshot(TowerRewardState rewardState);

public static float GetFinalDefenseMultiplier(
    TowerRewardState rewardState,
    float homeHealthNormalized);
```

Implement the moved calculations as:

```csharp
// 计算额外攻击规则提供的平均输出倍率。
public static float GetExtraAttackPowerMultiplier(TowerRewardState rewardState)
{
    if (rewardState == null)
    {
        return 1f;
    }

    float extraAttackPower = 0f;
    foreach (TowerExtraAttackRule rule in rewardState.ExtraAttackRuleList)
    {
        extraAttackPower += (float)rule.extraAttackCount / rule.triggerAttackCount;
    }

    return 1f + Mathf.Max(0f, extraAttackPower);
}

// 计算双倍伤害概率提供的平均输出倍率。
public static float GetCriticalPowerMultiplier(TowerRewardState rewardState)
{
    return 1f + (rewardState?.DoubleDamageChance ?? 0f);
}

// 计算爆裂箭提供的粗略范围输出倍率。
public static float GetExplosivePowerMultiplier(TowerRewardState rewardState)
{
    if (rewardState == null
        || !rewardState.ThreeStarExplosiveArrowEnabled
        || rewardState.ExplosionRadius <= 0f
        || rewardState.ExplosionDamageMultiplier <= 0f)
    {
        return 1f;
    }

    return 1f + Mathf.Clamp(
        rewardState.ExplosionRadius * rewardState.ExplosionDamageMultiplier * 0.1f,
        0f,
        1.5f);
}

// 根据基地生命比例计算最终防线倍率。
public static float GetFinalDefenseMultiplier(
    TowerRewardState rewardState,
    float homeHealthNormalized)
{
    if (rewardState == null
        || rewardState.FinalDefenseAttackDamageBonus <= 0f
        || rewardState.FinalDefenseHomeHealthThreshold <= 0f
        || homeHealthNormalized > rewardState.FinalDefenseHomeHealthThreshold)
    {
        return 1f;
    }

    return 1f + rewardState.FinalDefenseAttackDamageBonus;
}
```

- [ ] **Step 5: Update all callers and run tests**

Use `rg -n 'TowerActiveRewards|TowerPowerEvaluator' Assets/Scripts Assets/Tests` and require zero matches after migration.

Expected: Tower state and calculator tests pass.

## Task 6: Rename And Move Reward Application Runtime

**Files:**

- Move with `.meta`: `Rewards/Core/RewardEffectApplierSo.cs` → `Application/Effects/RewardEffectApplierSo.cs`
- Rename with `.meta`: `Rewards/Core/RewardEffectApplyContext.cs` → `Application/Effects/RewardApplyContext.cs`
- Move with `.meta`: `Rewards/Core/RewardEffectApplicationService.cs` → `Application/Effects/RewardEffectApplicationService.cs`
- Move all Tower Appliers with `.meta` → `Application/Tower/Effects/`
- Move `TowerRewardApplierSo.cs` with `.meta` → `Application/Tower/Effects/Base/`
- Move `TowerRewardRuntime.cs` with `.meta` → `Application/Tower/TowerRewardRuntime.cs`
- Move with `.meta`: `ITowerArrowModifier.cs` → `Application/Tower/Runtime/ITowerArrowModifier.cs`
- Move with `.meta`: `ITowerAttackCompletedTrigger.cs` → `Application/Tower/Runtime/ITowerAttackCompletedTrigger.cs`
- Rename with `.meta`: `ITowerEnemyKilledRewardTrigger.cs` → `Application/Tower/Runtime/ITowerEnemyKilledTrigger.cs`
- Move with `.meta`: `ITowerRuntimeReward.cs` → `Application/Tower/Runtime/ITowerRuntimeReward.cs`
- Move with `.meta`: `ITowerWaveCompletedTrigger.cs` → `Application/Tower/Runtime/ITowerWaveCompletedTrigger.cs`
- Move with `.meta`: `TowerArrowContext.cs` → `Application/Tower/Runtime/TowerArrowContext.cs`
- Move with `.meta`: `TowerAttackCompletedContext.cs` → `Application/Tower/Runtime/TowerAttackCompletedContext.cs`
- Move with `.meta`: `TowerEnemyKilledContext.cs` → `Application/Tower/Runtime/TowerEnemyKilledContext.cs`
- Rename with `.meta`: `TowerRewardRuntimeState.cs` → `Application/Tower/Runtime/TowerEffectState.cs`
- Move with `.meta`: `TowerRewardTriggerDispatcher.cs` → `Application/Tower/Runtime/TowerRewardTriggerDispatcher.cs`
- Rename `TowerRewardRuntimeState` → `TowerEffectState`
- Rename file `ITowerEnemyKilledRewardTrigger.cs` → `ITowerEnemyKilledTrigger.cs`
- Create: `Assets/Tests/EditMode/Rewards/Application/RewardEffectApplicationServiceTests.cs`

- [ ] **Step 1: Update characterization tests to final runtime names first**

Change the Task 1 tests:

```text
TowerRewardRuntimeState -> TowerEffectState
```

Update both constructor calls to include the world dependency:

```csharp
TowerEffectState state = new(null, null);
```

Run them and expect compile failure until production types move.

- [ ] **Step 2: Shrink RewardApplyContext**

```csharp
/// <summary>
/// 奖励效果应用上下文，提供效果执行所需的奖励运行时依赖。
/// </summary>
public sealed class RewardApplyContext
{
    public TowerRewardRuntime TowerRewardRuntime { get; }
    public TowerRewardState TowerRewardState => TowerRewardRuntime?.State;
    public ITowerRewardWorld TowerWorld => TowerRewardRuntime?.World;

    public RewardApplyContext(TowerRewardRuntime towerRewardRuntime)
    {
        TowerRewardRuntime = towerRewardRuntime;
    }
}
```

Remove `RewardRuntimeCoordinator`, `ResourceManager`, `WaveManager` and `BuildingPlacementManager` from the context.

- [ ] **Step 3: Rename application and runtime state references mechanically**

Apply:

```text
RewardEffectApplyContext  -> RewardApplyContext
TowerRewardRuntimeState   -> TowerEffectState
ActiveRewards property    -> State
TowerActiveRewards        -> TowerRewardState
```

Collection and method behavior remains unchanged.

The complete renamed effect state is:

```csharp
using System.Collections.Generic;

/// <summary>
/// 单个 Tower 运行时奖励效果的配置、场景入口和分 Tower 计数状态。
/// </summary>
public sealed class TowerEffectState
{
    private readonly Dictionary<TowerCombatSystem, Dictionary<string, int>> _towerCounterDic = new();

    public RewardCardEffectConfig Config { get; }
    public ITowerRewardWorld World { get; }

    public TowerEffectState(RewardCardEffectConfig config, ITowerRewardWorld world)
    {
        Config = config;
        World = world;
    }

    // 增加指定 Tower 的计数并返回新值。
    public int IncrementCounter(TowerCombatSystem sourceTowerCombatSystem, string counterId)
    {
        if (!sourceTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return 0;
        }

        if (!_towerCounterDic.TryGetValue(
                sourceTowerCombatSystem,
                out Dictionary<string, int> counterDic))
        {
            counterDic = new Dictionary<string, int>();
            _towerCounterDic.Add(sourceTowerCombatSystem, counterDic);
        }

        counterDic.TryGetValue(counterId, out int counter);
        counter++;
        counterDic[counterId] = counter;
        return counter;
    }

    // 重置指定 Tower 的一项计数。
    public void ResetCounter(TowerCombatSystem sourceTowerCombatSystem, string counterId)
    {
        if (!sourceTowerCombatSystem || string.IsNullOrWhiteSpace(counterId))
        {
            return;
        }

        if (_towerCounterDic.TryGetValue(
                sourceTowerCombatSystem,
                out Dictionary<string, int> counterDic))
        {
            counterDic[counterId] = 0;
        }
    }

    // 清理指定 Tower 的全部计数。
    public void ClearCounters(TowerCombatSystem sourceTowerCombatSystem)
    {
        if (sourceTowerCombatSystem)
        {
            _towerCounterDic.Remove(sourceTowerCombatSystem);
        }
    }
}
```

- [ ] **Step 4: Define ITowerRewardWorld before converting world-dependent effects**

```csharp
/// <summary>
/// Tower 奖励访问游戏场景所需的查询与命令接口。
/// </summary>
public interface ITowerRewardWorld
{
    float HomeHealthNormalized { get; }
    int GetThreeStarTowerCount();
    bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius);
    bool UpgradeRandomTowerToMaxStar();
    bool UpgradeTowerOneLevel(TowerCombatSystem sourceTowerCombatSystem);
    void HealAllTowers(float healPercent);
}
```

- [ ] **Step 5: Update TowerRewardRuntime constructor and API**

```csharp
public TowerRewardState State { get; }
public TowerRewardTriggerDispatcher TriggerDispatcher { get; }
public ITowerRewardWorld World { get; }

public TowerRewardRuntime(ITowerRewardWorld world)
{
    World = world;
    State = new TowerRewardState();
    TriggerDispatcher = new TowerRewardTriggerDispatcher(world);
}
```

Add world constructor injection to the dispatcher:

```csharp
private readonly ITowerRewardWorld _world;

public TowerRewardTriggerDispatcher(ITowerRewardWorld world)
{
    _world = world;
}
```

In `RegisterEffect`, construct state with:

```csharp
TowerEffectState runtimeState = new(config, _world);
```

`GetAdjustedBuildCostAmount`, `CreatePowerSnapshot` and wave completion continue to delegate to Domain and Runtime collaborators. Remove `BuildSummaryText` in Task 10 so Application does not depend on Presentation.

- [ ] **Step 6: Update all 23 Tower Appliers**

Move these exact files with `.meta`:

```text
TowerArmorIgnoreApplierSo.cs
TowerAttackBonusApplierSo.cs
TowerAttackDamagePerThreeStarApplierSo.cs
TowerAttackHealthCostApplierSo.cs
TowerAttackSpeedApplierSo.cs
TowerAttackSpeedOverloadApplierSo.cs
TowerBuildCostApplierSo.cs
TowerBurnArrowApplierSo.cs
TowerChanceExplosionApplierSo.cs
TowerDamageTakenApplierSo.cs
TowerDetectRadiusApplierSo.cs
TowerDoubleDamageChanceApplierSo.cs
TowerExtraArrowApplierSo.cs
TowerFinalAttackDamageApplierSo.cs
TowerKillGrowthApplierSo.cs
TowerLinkedAttackSpeedApplierSo.cs
TowerMaxHealthApplierSo.cs
TowerNewInitialStarApplierSo.cs
TowerPiercingArrowApplierSo.cs
TowerPoisonArrowApplierSo.cs
TowerRandomMaxStarApplierSo.cs
TowerThreeStarExplosiveArrowApplierSo.cs
TowerWaveEndHealApplierSo.cs
```

Replace state access with `applyContext.TowerRewardState`. Replace the three world-dependent calls:

```csharp
applyContext.TowerWorld?.UpgradeRandomTowerToMaxStar();
runtimeState.World?.UpgradeTowerOneLevel(context.SourceTowerCombatSystem);
runtimeState.World?.HealAllTowers(healPercent);
```

Store `ITowerRewardWorld` on `TowerEffectState` when registering each runtime effect so trigger callbacks do not use a global singleton.

- [ ] **Step 7: Run characterization tests and compile**

Expected: tests pass with final names; `rg` reports zero references to `RewardEffectApplyContext` and `TowerRewardRuntimeState`.

- [ ] **Step 8: Add explicit invalid-config warnings**

Add the test:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 通用奖励效果应用服务测试。
/// </summary>
public class RewardEffectApplicationServiceTests
{
    // 验证缺少效果定义时跳过配置并输出明确警告。
    [Test]
    public void ApplyEffectsMissingDefinitionLogsWarningAndSkips()
    {
        RewardCardEffectConfig config = new();
        LogAssert.Expect(LogType.Warning, "奖励效果缺少 EffectDefinition，已跳过。");

        RewardEffectApplicationService.ApplyEffects(
            new List<RewardCardEffectConfig> { config },
            new RewardApplyContext(null));
    }
}
```

Add `using UnityEngine;` at the file header, then replace the private application method with:

```csharp
// 应用单个效果并注册其运行时能力。
private static void ApplyEffect(
    RewardCardEffectConfig effectConfig,
    RewardApplyContext applyContext)
{
    if (effectConfig == null)
    {
        return;
    }

    RewardEffectDefinitionSo definition = effectConfig.EffectDefinition;
    if (!definition)
    {
        Debug.LogWarning("奖励效果缺少 EffectDefinition，已跳过。");
        return;
    }

    RewardEffectApplierSo applier = definition.Applier;
    if (!applier)
    {
        Debug.LogWarning($"奖励效果 {definition.name} 缺少 Applier，已跳过。", definition);
        return;
    }

    applier.Apply(applyContext, effectConfig);
    if (applier is ITowerRuntimeReward towerRuntimeReward)
    {
        applyContext?.TowerRewardRuntime?.TriggerDispatcher.RegisterEffect(
            towerRuntimeReward,
            effectConfig);
    }
}
```

In `RewardEffectParameterReader.GetFloat` and `GetInt`, emit a warning only when `logMissingWarning` is true:

```csharp
if (logMissingWarning)
{
    Debug.LogWarning($"奖励参数缺失：{parameterId}");
}
```

Run `RewardEffectApplicationServiceTests`. Expected: pass with no unexpected logs.

## Task 7: Extract TowerUpgradeController From UI

**Files:**

- Create: `Assets/Scripts/Buildings/Runtime/Tower/TowerUpgradeController.cs`
- Modify: `Assets/Scripts/UI/Buildings/BuildingUpgradeButton.cs`
- Create: `Assets/Tests/EditMode/Buildings/TowerUpgradeControllerTests.cs`

- [ ] **Step 1: Write controller behavior tests**

```csharp
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tower 升级运行时组件测试。
/// </summary>
public class TowerUpgradeControllerTests
{
    // 验证免费升级可以提升一级并升到满级。
    [Test]
    public void FreeUpgradeAdvancesAndReachesMaxStar()
    {
        using TestGameObjectScope scope = new();
        BuildingUpgradeConfigSo config = CreateConfig();
        TowerUpgradeController controller = scope.Create("TowerUpgrade").AddComponent<TowerUpgradeController>();
        SetField(controller, "upgradeConfig", config);

        Assert.IsTrue(controller.UpgradeOneLevelWithoutCost());
        Assert.AreEqual(2, controller.CurrentStarLevel);
        Assert.IsTrue(controller.UpgradeToMaxStarWithoutCost());
        Assert.AreEqual(3, controller.CurrentStarLevel);
        Assert.IsTrue(controller.IsMaxStar);
        Object.DestroyImmediate(config);
    }

    // 创建包含二星和三星配置的测试资产。
    private static BuildingUpgradeConfigSo CreateConfig()
    {
        BuildingUpgradeLevel levelTwo = new();
        BuildingUpgradeLevel levelThree = new();
        SetField(levelTwo, "starLevel", 2);
        SetField(levelThree, "starLevel", 3);

        BuildingUpgradeConfigSo config = ScriptableObject.CreateInstance<BuildingUpgradeConfigSo>();
        SetField(config, "upgradeLevels", new List<BuildingUpgradeLevel> { levelTwo, levelThree });
        return config;
    }

    // 设置测试对象的私有序列化字段。
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }
}
```

- [ ] **Step 2: Verify tests fail because TowerUpgradeController is missing**

Run with `-testFilter 'TowerUpgradeControllerTests'`.

- [ ] **Step 3: Extract gameplay upgrade state and commands**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tower 升级控制器，负责星级状态、升级消耗和升级属性应用。
/// </summary>
[DisallowMultipleComponent]
public class TowerUpgradeController : MonoBehaviour
{
    private const int MIN_STAR_LEVEL = 1;

    [SerializeField] private BuildingUpgradeConfigSo upgradeConfig;
    [SerializeField] private Building building;
    [SerializeField] private TowerCombatSystem towerCombatSystem;
    [SerializeField] private int currentStarLevel = MIN_STAR_LEVEL;

    public event Action<int> OnStarLevelChanged;

    public int CurrentStarLevel => currentStarLevel;
    public int MaxStarLevel => upgradeConfig ? upgradeConfig.MaxStarLevel : currentStarLevel;
    public bool IsMaxStar => currentStarLevel >= MaxStarLevel;
    public TowerCombatSystem TowerCombatSystem => towerCombatSystem;
    public IReadOnlyList<ResourceCost> NextUpgradeCostList => TryGetNextUpgradeLevel(out BuildingUpgradeLevel level)
        ? level.UpgradeCost
        : Array.Empty<ResourceCost>();

    private void Awake()
    {
        if (!building)
        {
            building = GetComponentInParent<Building>();
        }

        if (!towerCombatSystem)
        {
            towerCombatSystem = GetComponentInParent<TowerCombatSystem>();
        }
    }

    // 尝试消耗资源提升一级。
    public bool TryUpgradeWithCost(ResourceManager resourceManager)
    {
        if (!resourceManager || !TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel))
        {
            return false;
        }

        if (!resourceManager.CanAfford(upgradeLevel.UpgradeCost))
        {
            return false;
        }

        resourceManager.Spend(upgradeLevel.UpgradeCost);
        return ApplyStarLevel(currentStarLevel + 1);
    }

    // 无消耗提升一级。
    public bool UpgradeOneLevelWithoutCost()
    {
        return !IsMaxStar && ApplyStarLevel(currentStarLevel + 1);
    }

    // 无消耗提升到最高星级。
    public bool UpgradeToMaxStarWithoutCost()
    {
        return !IsMaxStar && ApplyStarLevel(MaxStarLevel);
    }

    // 应用奖励提供的初始星级加成。
    public bool ApplyInitialStarBonus(int initialStarBonus)
    {
        if (initialStarBonus <= 0 || IsMaxStar)
        {
            return false;
        }

        int targetStarLevel = Mathf.Clamp(
            currentStarLevel + initialStarBonus,
            MIN_STAR_LEVEL,
            MaxStarLevel);
        return ApplyStarLevel(targetStarLevel);
    }

    // 应用指定星级配置。
    private bool ApplyStarLevel(int targetStarLevel)
    {
        if (!upgradeConfig)
        {
            return false;
        }

        targetStarLevel = Mathf.Clamp(targetStarLevel, MIN_STAR_LEVEL, MaxStarLevel);
        if (targetStarLevel <= currentStarLevel)
        {
            return false;
        }

        BuildingUpgradeLevel upgradeLevel = upgradeConfig.GetUpgradeLevel(targetStarLevel);
        if (upgradeLevel == null)
        {
            return false;
        }

        currentStarLevel = targetStarLevel;
        if (building)
        {
            building.ApplyUpgradeLevel(upgradeLevel);
        }

        if (towerCombatSystem)
        {
            towerCombatSystem.ApplyUpgradeLevel(upgradeLevel);
        }

        OnStarLevelChanged?.Invoke(currentStarLevel);
        return true;
    }

    // 获取下一星级配置。
    private bool TryGetNextUpgradeLevel(out BuildingUpgradeLevel upgradeLevel)
    {
        upgradeLevel = upgradeConfig
            ? upgradeConfig.GetUpgradeLevel(currentStarLevel + 1)
            : null;
        return upgradeLevel != null;
    }
}
```

- [ ] **Step 4: Reduce BuildingUpgradeButton to UI responsibilities**

Keep only:

- serialized `Button`, `TowerUpgradeController`, star visuals and visual options;
- click handling;
- resource warning popup;
- button interactable state;
- star visual refresh;
- event subscription cleanup.

The click handler calls:

```csharp
if (!upgradeController.TryUpgradeWithCost(ResourceManager.Instance))
{
    ShowUpgradeResourceWarning();
}
```

Remove free-upgrade methods and Tower reward registration from `BuildingUpgradeButton`.

- [ ] **Step 5: Preserve serialized scene references**

Add `TowerUpgradeController` to each prefab or scene object that currently has `BuildingUpgradeButton`, assign the existing upgrade config, building and Tower combat references, then point the button at the controller. Verify no Missing Script and no null controller in loaded scenes.

- [ ] **Step 6: Run controller tests and compile**

Expected: controller tests pass and `rg -n 'UpgradeOneLevelWithoutCost|UpgradeToMaxStarWithoutCost' Assets/Scripts/UI` returns no matches.

## Task 8: Move Scene Registration Into Buildings

**Files:**

- Create: `Assets/Scripts/Buildings/Runtime/BuildingRuntimeRegistry.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/BuildingPlacementManager.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/Building.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/Tower/TowerCombatSystem.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/Tower/TowerUpgradeController.cs`
- Delete after migration: `Assets/Scripts/Rewards/Tower/Registry/TowerRegistry.cs`
- Create: `Assets/Tests/EditMode/Buildings/BuildingRuntimeRegistryTests.cs`

- [ ] **Step 1: Write registry tests**

```csharp
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 建筑运行时注册表测试。
/// </summary>
public class BuildingRuntimeRegistryTests
{
    // 验证附近查询排除来源 Tower 并识别半径内另一座 Tower。
    [Test]
    public void HasNearbyTowerExcludesSourceAndFindsNeighbor()
    {
        using TestGameObjectScope scope = new();
        TowerCombatSystem sourceTower = scope.Create("Source").AddComponent<TowerCombatSystem>();
        TowerCombatSystem nearbyTower = scope.Create("Nearby").AddComponent<TowerCombatSystem>();
        sourceTower.transform.position = Vector3.zero;
        nearbyTower.transform.position = Vector3.right * 2f;
        BuildingRuntimeRegistry registry = new();
        registry.RegisterTower(sourceTower);
        registry.RegisterTower(sourceTower);
        registry.RegisterTower(nearbyTower);

        Assert.IsTrue(registry.HasNearbyTower(sourceTower, 3f));
        registry.UnregisterTower(nearbyTower);
        Assert.IsFalse(registry.HasNearbyTower(sourceTower, 3f));
    }

    // 验证升级组件可以按 Tower 查询。
    [Test]
    public void GetUpgradeControllerReturnsMatchingController()
    {
        using TestGameObjectScope scope = new();
        GameObject towerObject = scope.Create("Tower");
        TowerCombatSystem tower = towerObject.AddComponent<TowerCombatSystem>();
        TowerUpgradeController controller = towerObject.AddComponent<TowerUpgradeController>();
        BuildingRuntimeRegistry registry = new();
        registry.RegisterUpgradeController(controller);

        Assert.AreSame(controller, registry.GetUpgradeController(tower));
    }

    // 验证没有基地生命组件时返回完整生命比例。
    [Test]
    public void HomeHealthNormalizedWithoutHomeReturnsOne()
    {
        BuildingRuntimeRegistry registry = new();

        Assert.AreEqual(1f, registry.HomeHealthNormalized);
    }
}
```

- [ ] **Step 2: Implement BuildingRuntimeRegistry as a scene-owned registry**

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 建筑运行时注册表，保存当前场景中的建筑、Tower、升级组件和基地生命。
/// </summary>
public sealed class BuildingRuntimeRegistry
{
    private const int THREE_STAR_LEVEL = 3;

    private readonly List<Building> _defenseBuildingList = new();
    private readonly List<TowerCombatSystem> _towerList = new();
    private readonly List<TowerUpgradeController> _upgradeControllerList = new();
    private HealthSystem _homeHealthSystem;

    public IReadOnlyList<Building> DefenseBuildingList => _defenseBuildingList;
    public IReadOnlyList<TowerUpgradeController> UpgradeControllerList => _upgradeControllerList;
    public float HomeHealthNormalized => _homeHealthSystem
        ? _homeHealthSystem.CurrentHealthNormalized
        : 1f;

    // 注册防御建筑。
    public void RegisterBuilding(Building building)
    {
        AddUnique(_defenseBuildingList, building);
    }

    // 注销防御建筑。
    public void UnregisterBuilding(Building building)
    {
        _defenseBuildingList.Remove(building);
    }

    // 注册 Tower 战斗组件。
    public void RegisterTower(TowerCombatSystem towerCombatSystem)
    {
        AddUnique(_towerList, towerCombatSystem);
    }

    // 注销 Tower 战斗组件。
    public void UnregisterTower(TowerCombatSystem towerCombatSystem)
    {
        _towerList.Remove(towerCombatSystem);
    }

    // 注册 Tower 升级组件。
    public void RegisterUpgradeController(TowerUpgradeController upgradeController)
    {
        AddUnique(_upgradeControllerList, upgradeController);
    }

    // 注销 Tower 升级组件。
    public void UnregisterUpgradeController(TowerUpgradeController upgradeController)
    {
        _upgradeControllerList.Remove(upgradeController);
    }

    // 注册基地生命组件。
    public void RegisterHomeHealth(HealthSystem healthSystem)
    {
        if (healthSystem)
        {
            _homeHealthSystem = healthSystem;
        }
    }

    // 注销基地生命组件。
    public void UnregisterHomeHealth(HealthSystem healthSystem)
    {
        if (_homeHealthSystem == healthSystem)
        {
            _homeHealthSystem = null;
        }
    }

    // 返回当前三星 Tower 数量。
    public int GetThreeStarTowerCount()
    {
        int count = 0;
        foreach (TowerCombatSystem tower in _towerList)
        {
            if (tower && tower.CurrentStarLevel >= THREE_STAR_LEVEL)
            {
                count++;
            }
        }

        return count;
    }

    // 判断来源 Tower 附近是否存在其他 Tower。
    public bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius)
    {
        if (!sourceTowerCombatSystem || radius <= 0f)
        {
            return false;
        }

        float radiusSqr = radius * radius;
        Vector3 sourcePosition = sourceTowerCombatSystem.transform.position;
        foreach (TowerCombatSystem tower in _towerList)
        {
            if (!tower || tower == sourceTowerCombatSystem)
            {
                continue;
            }

            if ((tower.transform.position - sourcePosition).sqrMagnitude <= radiusSqr)
            {
                return true;
            }
        }

        return false;
    }

    // 返回指定 Tower 对应的升级组件。
    public TowerUpgradeController GetUpgradeController(TowerCombatSystem towerCombatSystem)
    {
        if (!towerCombatSystem)
        {
            return null;
        }

        foreach (TowerUpgradeController controller in _upgradeControllerList)
        {
            if (controller && controller.TowerCombatSystem == towerCombatSystem)
            {
                return controller;
            }
        }

        return null;
    }

    // 向列表中添加唯一且有效的 Unity 对象。
    private static void AddUnique<T>(List<T> targetList, T item) where T : Object
    {
        if (item && !targetList.Contains(item))
        {
            targetList.Add(item);
        }
    }
}
```

- [ ] **Step 3: Make scene composition provide one registry reference**

Make `BuildingPlacementManager` the scene owner:

```csharp
private readonly BuildingRuntimeRegistry _buildingRegistry = new();

public BuildingRuntimeRegistry BuildingRegistry => _buildingRegistry;
```

`Building`, `TowerCombatSystem`, `TowerUpgradeController` and `RewardRuntimeCoordinator` cache `BuildingPlacementManager.Instance.BuildingRegistry` once in `Start`. Each component unregisters from the cached registry in `OnDisable` or `OnDestroy`. Do not call `FindObjectOfType` in Update or combat paths.

- [ ] **Step 4: Replace TowerRegistry registrations**

Apply this map:

```text
TowerRegistry.RegisterDefenseBuilding      -> BuildingRuntimeRegistry.RegisterBuilding
TowerRegistry.UnregisterDefenseBuilding    -> BuildingRuntimeRegistry.UnregisterBuilding
TowerRegistry.RegisterDefenseTowerSystem   -> BuildingRuntimeRegistry.RegisterTower
TowerRegistry.UnregisterDefenseTowerSystem -> BuildingRuntimeRegistry.UnregisterTower
TowerRegistry.RegisterHomeHealthSystem     -> BuildingRuntimeRegistry.RegisterHomeHealth
TowerRegistry.UnregisterHomeHealthSystem   -> BuildingRuntimeRegistry.UnregisterHomeHealth
```

Register `TowerUpgradeController` from its own lifecycle; the UI button does not register.

- [ ] **Step 5: Update TowerStatCalculator world queries**

Pass `TowerRewardRuntime` to `TowerStatCalculator` when `TowerCombatSystem.Start` constructs it:

```csharp
private readonly TowerRewardRuntime _rewardRuntime;

public TowerStatCalculator(
    TowerCombatSystem sourceTowerCombatSystem,
    int baseAttackDamage,
    float baseArrowGenerateRate,
    float baseDetectRadius,
    TowerRewardRuntime rewardRuntime)
{
    this.sourceTowerCombatSystem = sourceTowerCombatSystem;
    _baseAttackDamage = Mathf.Max(1, baseAttackDamage);
    _baseArrowGenerateRate = Mathf.Max(MIN_ARROW_GENERATE_RATE, baseArrowGenerateRate);
    _baseDetectRadius = Mathf.Max(0.01f, baseDetectRadius);
    _rewardRuntime = rewardRuntime;
}
```

Read `_rewardRuntime?.State` for reward values and `_rewardRuntime?.World` for three-star count, nearby-Tower queries and home health. Remove all `RewardRuntimeCoordinator.Instance` and static `TowerRegistry` lookups from `TowerStatCalculator`.

Move `_statCalculator` construction from `TowerCombatSystem.Awake` to `Start`, after all scene `Awake` methods have initialized the coordinator. Keep component caching in `Awake`.

- [ ] **Step 6: Run registry tests and compile**

Expected: tests pass and all production `TowerRegistry.` references are gone before deleting the old file and `.meta`.

## Task 9: Implement TowerRewardWorld Integration

**Files:**

- Create: `Assets/Scripts/Rewards/Integration/Tower/TowerRewardWorld.cs`
- Modify: `Assets/Scripts/Rewards/Application/Tower/TowerRewardRuntime.cs`
- Modify: world-dependent Tower Appliers.
- Delete with `.meta`: `TowerImmediateExecutor.cs`, `TowerWaveEndExecutor.cs`
- Create: `Assets/Tests/EditMode/Rewards/Integration/TowerRewardWorldTests.cs`

- [ ] **Step 1: Write integration tests using an in-memory BuildingRuntimeRegistry**

```csharp
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tower 奖励场景适配器测试。
/// </summary>
public class TowerRewardWorldTests
{
    // 验证指定 Tower 可以通过对应升级组件免费提升一级。
    [Test]
    public void UpgradeTowerOneLevelUsesMatchingController()
    {
        using TestGameObjectScope scope = new();
        BuildingUpgradeConfigSo config = CreateConfig();
        GameObject towerObject = scope.Create("Tower");
        TowerCombatSystem tower = towerObject.AddComponent<TowerCombatSystem>();
        TowerUpgradeController controller = towerObject.AddComponent<TowerUpgradeController>();
        SetField(controller, "upgradeConfig", config);
        BuildingRuntimeRegistry registry = new();
        registry.RegisterUpgradeController(controller);
        TowerRewardWorld world = new(registry, new FirstIndexRandom());

        Assert.IsTrue(world.UpgradeTowerOneLevel(tower));
        Assert.AreEqual(2, controller.CurrentStarLevel);
        Object.DestroyImmediate(config);
    }

    // 验证没有基地生命组件时返回完整生命比例。
    [Test]
    public void HomeHealthNormalizedWithoutHomeReturnsOne()
    {
        TowerRewardWorld world = new(new BuildingRuntimeRegistry(), new FirstIndexRandom());

        Assert.AreEqual(1f, world.HomeHealthNormalized);
    }

    // 创建测试升级配置。
    private static BuildingUpgradeConfigSo CreateConfig()
    {
        BuildingUpgradeLevel level = new();
        SetField(level, "starLevel", 2);
        BuildingUpgradeConfigSo config = ScriptableObject.CreateInstance<BuildingUpgradeConfigSo>();
        SetField(config, "upgradeLevels", new List<BuildingUpgradeLevel> { level });
        return config;
    }

    // 设置测试对象私有字段。
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    /// <summary>
    /// 始终选择首个候选项的随机数实现。
    /// </summary>
    private sealed class FirstIndexRandom : IRewardRandom
    {
        // 返回区间最小值。
        public int Range(int minInclusive, int maxExclusive)
        {
            return minInclusive;
        }
    }
}
```

- [ ] **Step 2: Implement TowerRewardWorld**

```csharp
using System.Collections.Generic;

/// <summary>
/// Tower 奖励与实际建筑场景之间的适配器。
/// </summary>
public sealed class TowerRewardWorld : ITowerRewardWorld
{
    private readonly BuildingRuntimeRegistry _registry;
    private readonly IRewardRandom _random;

    public float HomeHealthNormalized => _registry?.HomeHealthNormalized ?? 1f;

    public TowerRewardWorld(BuildingRuntimeRegistry registry, IRewardRandom random)
    {
        _registry = registry;
        _random = random;
    }

    // 返回当前三星 Tower 数量。
    public int GetThreeStarTowerCount()
    {
        return _registry?.GetThreeStarTowerCount() ?? 0;
    }

    // 判断来源 Tower 附近是否存在其他 Tower。
    public bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius)
    {
        return _registry != null && _registry.HasNearbyTower(sourceTowerCombatSystem, radius);
    }

    // 随机选择一座可升级 Tower 并提升到满级。
    public bool UpgradeRandomTowerToMaxStar()
    {
        if (_registry == null || _random == null)
        {
            return false;
        }

        TowerUpgradeController selectedController = null;
        int eligibleCount = 0;
        foreach (TowerUpgradeController controller in _registry.UpgradeControllerList)
        {
            if (!controller || controller.IsMaxStar)
            {
                continue;
            }

            eligibleCount++;
            if (_random.Range(0, eligibleCount) == 0)
            {
                selectedController = controller;
            }
        }

        return selectedController && selectedController.UpgradeToMaxStarWithoutCost();
    }

    // 将指定 Tower 免费提升一级。
    public bool UpgradeTowerOneLevel(TowerCombatSystem sourceTowerCombatSystem)
    {
        TowerUpgradeController controller = _registry?.GetUpgradeController(sourceTowerCombatSystem);
        return controller && controller.UpgradeOneLevelWithoutCost();
    }

    // 按最大生命比例治疗全部防御建筑。
    public void HealAllTowers(float healPercent)
    {
        if (_registry == null || healPercent <= 0f)
        {
            return;
        }

        IReadOnlyList<Building> buildingList = _registry.DefenseBuildingList;
        foreach (Building building in buildingList)
        {
            if (building)
            {
                building.HealByMaxHealthPercent(healPercent);
            }
        }
    }
}
```

- [ ] **Step 3: Replace world-dependent Applier calls**

Use `TowerEffectState.World` in kill-growth and wave-end callbacks. Use `RewardApplyContext.TowerWorld` for immediate random max-star effects.

- [ ] **Step 4: Remove executor indirection**

Delete `TowerImmediateExecutor` and `TowerWaveEndExecutor` after `rg` confirms no references.

- [ ] **Step 5: Run integration and trigger tests**

Expected: `TowerRewardWorldTests` and `TowerRewardTriggerCharacterizationTests` pass.

## Task 10: Rebuild Coordinator And Presentation Event Flow

**Files:**

- Move with `.meta`: `Rewards/Runtime/RewardRuntimeCoordinator.cs` → `Rewards/Integration/RewardRuntimeCoordinator.cs`
- Move and rename with `.meta`: `UI/Rewards/RewardCardController.cs` → `Rewards/Presentation/Cards/RewardCardSelectionController.cs`
- Move with `.meta`: `UI/Rewards/ActiveRewardHud.cs` → `Rewards/Presentation/Cards/ActiveRewardHud.cs`
- Move with `.meta`: `UI/Rewards/RewardCardDescriptionFormatter.cs` → `Rewards/Presentation/Cards/RewardCardDescriptionFormatter.cs`
- Move with `.meta`: `UI/Rewards/RewardCardOption.cs` → `Rewards/Presentation/Cards/RewardCardOption.cs`
- Move with `.meta`: `UI/Rewards/RewardCardSelectionPanel.cs` → `Rewards/Presentation/Cards/RewardCardSelectionPanel.cs`
- Move with `.meta`: `UI/Rewards/RewardCardView.cs` → `Rewards/Presentation/Cards/RewardCardView.cs`
- Move with `.meta`: `UI/Rewards/RewardGainToast.cs` → `Rewards/Presentation/Cards/RewardGainToast.cs`
- Move with `.meta`: `UI/Rewards/RewardSummaryPanel.cs` → `Rewards/Presentation/Cards/RewardSummaryPanel.cs`
- Move with `.meta`: `UI/Rewards/RewardSummaryRecordItem.cs` → `Rewards/Presentation/Cards/RewardSummaryRecordItem.cs`
- Move with `.meta`: `Tower/Evaluation/TowerRewardSummaryFormatter.cs` → `Rewards/Presentation/Tower/`
- Delete after all callers migrate: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredContext.cs`
- Delete after all callers migrate: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredHistory.cs`
- Delete after all callers migrate: `Assets/Scripts/Rewards/Runtime/RewardCardAcquiredRecord.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/Building.cs`
- Modify: `Assets/Scripts/Buildings/Runtime/Tower/TowerCombatSystem.cs`
- Modify: `Assets/Scripts/UI/Common/TooltipManager.cs`
- Create: `Assets/Tests/EditMode/Rewards/Integration/RewardRuntimeCoordinatorTests.cs`

- [ ] **Step 1: Add coordinator event-order tests**

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 奖励运行时协调器流程测试。
/// </summary>
public class RewardRuntimeCoordinatorTests
{
    // 验证奖励应用后先发送应用事件，再发送状态变化事件。
    [Test]
    public void ApplyRewardRaisesEventsInDocumentedOrder()
    {
        using TestGameObjectScope scope = new();
        RewardRuntimeCoordinator coordinator = scope.Create("Coordinator")
            .AddComponent<RewardRuntimeCoordinator>();
        SetField(coordinator, "_history", new RewardHistory());
        SetField(coordinator, "_towerRewards", new TowerRewardRuntime(new FakeTowerRewardWorld()));
        RewardCardSo card = ScriptableObject.CreateInstance<RewardCardSo>();
        List<string> eventNameList = new();
        coordinator.OnRewardApplied += _ => eventNameList.Add("Applied");
        coordinator.OnActiveRewardsChanged += () => eventNameList.Add("Changed");

        coordinator.ApplyReward(card);

        CollectionAssert.AreEqual(new[] { "Applied", "Changed" }, eventNameList);
        Assert.AreEqual(1, coordinator.History.TotalCardCount);
        Object.DestroyImmediate(card);
    }

    // 验证空卡池不会发送候选卡事件。
    [Test]
    public void HandleWaveCompletedEmptyPoolDoesNotRaiseChoices()
    {
        using TestGameObjectScope scope = new();
        RewardRuntimeCoordinator coordinator = scope.Create("Coordinator")
            .AddComponent<RewardRuntimeCoordinator>();
        RewardCardPoolSo pool = ScriptableObject.CreateInstance<RewardCardPoolSo>();
        SetField(coordinator, "_rewardCardPool", pool);
        SetField(coordinator, "_history", new RewardHistory());
        SetField(coordinator, "_drawService", new RewardCardDrawService(new FirstIndexRandom()));
        SetField(coordinator, "_towerRewards", new TowerRewardRuntime(new FakeTowerRewardWorld()));
        bool choicesRaised = false;
        coordinator.OnRewardChoicesReady += _ => choicesRaised = true;

        Invoke(coordinator, "HandleWaveCompleted", 1);

        Assert.IsFalse(choicesRaised);
        Object.DestroyImmediate(pool);
    }

    // 设置私有字段。
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);
    }

    // 调用私有实例方法。
    private static void Invoke(object target, string methodName, params object[] argumentArray)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(target, argumentArray);
    }

    /// <summary>
    /// 固定返回首项的随机数实现。
    /// </summary>
    private sealed class FirstIndexRandom : IRewardRandom
    {
        // 返回区间最小值。
        public int Range(int minInclusive, int maxExclusive)
        {
            return minInclusive;
        }
    }

    /// <summary>
    /// 不访问场景的 Tower 奖励世界实现。
    /// </summary>
    private sealed class FakeTowerRewardWorld : ITowerRewardWorld
    {
        public float HomeHealthNormalized => 1f;

        // 返回零座三星 Tower。
        public int GetThreeStarTowerCount() => 0;

        // 返回附近没有其他 Tower。
        public bool HasNearbyTower(TowerCombatSystem sourceTowerCombatSystem, float radius) => false;

        // 返回没有可升满的 Tower。
        public bool UpgradeRandomTowerToMaxStar() => false;

        // 返回指定 Tower 无法升级。
        public bool UpgradeTowerOneLevel(TowerCombatSystem sourceTowerCombatSystem) => false;

        // 忽略治疗请求。
        public void HealAllTowers(float healPercent)
        {
        }
    }
}
```

- [ ] **Step 2: Make RewardRuntimeCoordinator own runtime state**

```csharp
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

    public event Action<IReadOnlyList<RewardCardSo>> OnRewardChoicesReady;
    public event Action<RewardAppliedContext> OnRewardApplied;
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
        InitializeRuntime();
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
    private void InitializeRuntime()
    {
        buildingPlacementManager = buildingPlacementManager
            ? buildingPlacementManager
            : BuildingPlacementManager.Instance;
        if (!buildingPlacementManager)
        {
            Debug.LogError("RewardRuntimeCoordinator 缺少 BuildingPlacementManager。", this);
            return;
        }

        IRewardRandom random = new UnityRewardRandom();
        _history = new RewardHistory();
        _drawService = new RewardCardDrawService(random);
        TowerRewardWorld towerWorld = new(buildingPlacementManager.BuildingRegistry, random);
        _towerRewards = new TowerRewardRuntime(towerWorld);
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
```

- [ ] **Step 3: Convert RewardCardSelectionController to presentation only**

Keep the existing serialized pool field to preserve scene data. In `Start`, register it once:

```csharp
_coordinator = RewardRuntimeCoordinator.Instance;
_coordinator.ConfigureCardPool(rewardCardPool);
_coordinator.OnRewardChoicesReady += HandleRewardChoicesReady;
```

`HandleRewardChoicesReady` receives candidates, creates views and pauses time only when the list is non-empty. Selection calls `_coordinator.ApplyReward(rewardCard)`. Remove direct access to `WaveManager`, draw service and history.

- [ ] **Step 4: Update HUD, Toast and Summary subscriptions**

Replace static history events with coordinator instance events:

```csharp
_coordinator.OnRewardApplied += HandleRewardApplied;
```

Read history from `_coordinator.History.RecordList`. Format descriptions at display time using `RewardCardDescriptionFormatter`.

Apply this Presentation type map:

```text
RewardCardAcquiredContext -> RewardAppliedContext
RewardCardAcquiredRecord  -> RewardCardRecord
context.AcquiredRecord    -> context.AppliedRecord
record.DescriptionText    -> RewardCardDescriptionFormatter.BuildDescriptionText(record.RewardCard)
```

Update `RewardSummaryRecordItem.SetRecord` and all private refresh methods to accept `RewardCardRecord`. Card name, rarity and category are read from `record.RewardCard`; wave and stack count remain on the record.

`RewardSummaryPanel` builds the Tower section directly through the Presentation formatter:

```csharp
string summaryText = _coordinator?.TowerRewards?.State != null
    ? TowerRewardSummaryFormatter.BuildSummaryText(_coordinator.TowerRewards.State)
    : string.Empty;
```

Delete `TowerRewardRuntime.BuildSummaryText` after this caller migrates.

- [ ] **Step 5: Update gameplay subscribers**

`Building` and `TowerCombatSystem` subscribe to the coordinator instance `OnActiveRewardsChanged` event in `OnEnable` and unsubscribe in `OnDisable`. They read `TowerRewards.State` instead of `ActiveRewards`.

`TooltipManager` continues to call the coordinator build-cost API but handles a missing coordinator by using the original cost.

- [ ] **Step 6: Preserve component and asset references during moves**

Move each UI script with `.meta`. Rename `RewardCardController` class and file together while retaining the original `.meta` GUID. Verify all reward prefabs and scenes have valid scripts after Unity imports.

- [ ] **Step 7: Run coordinator tests and compile**

Expected: event-order tests pass; `rg -n 'RewardCardAcquiredHistory|RewardCardController|OnRewardCardApplied' Assets/Scripts Assets/Tests` returns zero matches.

## Task 11: Move Editor Code And Remove Old Structure

**Files:**

- Move with `.meta`: `Assets/Scripts/Rewards/Authoring/RewardEffectAuthoringPresets.cs` → `Assets/Scripts/Rewards/Editor/RewardEffectAuthoringPresets.cs`
- Move with `.meta`: `Assets/Scripts/Editor/RewardCardIdAutoGenerator.cs` → `Assets/Scripts/Rewards/Editor/RewardCardIdAutoGenerator.cs`
- Move with `.meta`: `Assets/Scripts/Editor/RewardCardSoEditor.cs` → `Assets/Scripts/Rewards/Editor/RewardCardSoEditor.cs`
- Move with `.meta`: `Assets/Scripts/Editor/RewardEffectDefinitionSoEditor.cs` → `Assets/Scripts/Rewards/Editor/RewardEffectDefinitionSoEditor.cs`
- Ensure `RewardCardPoolSoEditor.cs` already resides in this directory from Task 2.
- Delete empty old Rewards directories and their `.meta` files.

- [ ] **Step 1: Move editor scripts and update renamed pool references**

Replace all `RewardCardDrawPoolSo` symbols with `RewardCardPoolSo`. Keep serialized property names unchanged when the underlying fields are unchanged.

- [ ] **Step 2: Remove obsolete test file after coverage replacement**

Delete with `.meta`:

```text
Assets/Tests/EditMode/DefenseTowerRewardTriggerTests.cs
Assets/Tests/EditMode/DefenseTowerRewardTriggerTests.cs.meta
```

Only delete after Tasks 1, 6 and 9 cover every still-supported trigger behavior.

- [ ] **Step 3: Rename remaining DefenseTower test terminology that describes current Tower types**

Rename `DefenseTowerArrowAbilityTests` to `TowerArrowAbilityTests` only if the class tests the renamed `Tower*` APIs. Preserve its `.meta`. Do not rename domain concepts that still intentionally use “Defense” as a building category.

- [ ] **Step 4: Remove empty old directories**

The following must no longer exist after all files move:

```text
Assets/Scripts/Rewards/Authoring
Assets/Scripts/Rewards/Core
Assets/Scripts/Rewards/Runtime
Assets/Scripts/Rewards/Tower/ActiveRewards
Assets/Scripts/Rewards/Tower/Appliers
Assets/Scripts/Rewards/Tower/EffectExecutor
Assets/Scripts/Rewards/Tower/Evaluation
Assets/Scripts/Rewards/Tower/Registry
Assets/Scripts/Rewards/Tower/Triggers
Assets/Scripts/UI/Rewards
```

- [ ] **Step 5: Verify final directory shape**

Run:

```powershell
Get-ChildItem -LiteralPath 'Assets\Scripts\Rewards' -Directory -Recurse |
  ForEach-Object { $_.FullName.Replace((Get-Location).Path + '\', '') }
```

Expected top-level directories: `Data`, `Domain`, `Application`, `Integration`, `Presentation`, `Editor` only.

## Task 12: Final Validation And Asset Safety Audit

**Files:**

- Modify any failing test or direct caller discovered by the checks below.
- Create: `Assets/Tests/PlayMode/BuilderDefender.PlayModeTests.asmdef`
- Create: `Assets/Tests/PlayMode/Rewards/RewardCardSelectionControllerTests.cs`
- Do not add compatibility wrappers to satisfy obsolete tests.

- [ ] **Step 1: Scan for obsolete symbols**

Run:

```powershell
rg -n 'TowerActiveRewards|RewardCardDrawPoolSo|RewardCardAcquiredHistory|RewardCardAcquiredRecord|RewardEffectApplyContext|TowerRewardRuntimeState|TowerPowerEvaluator|TowerRegistry|TowerImmediateExecutor|TowerWaveEndExecutor|DefenseTowerReward' Assets/Scripts Assets/Tests
```

Expected: zero matches, except serialized migration attributes containing an intentional former field name.

- [ ] **Step 2: Verify file/type name alignment**

For every public top-level class, interface, struct and enum under `Assets/Scripts/Rewards`, confirm the filename matches the primary type. The known `ITowerEnemyKilledRewardTrigger.cs` mismatch must be gone.

- [ ] **Step 3: Run all EditMode tests**

Run the full EditMode command from Verification Commands.

Expected: exit code `0`, no failed tests.

- [ ] **Step 4: Run targeted PlayMode checks in Unity Test Runner**

Create the PlayMode test assembly:

```json
{
  "name": "BuilderDefender.PlayModeTests",
  "rootNamespace": "",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false
}
```

Add the pause/restore test:

```csharp
using System.Collections;
using System.Reflection;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 奖励卡选择界面 PlayMode 测试。
/// </summary>
public class RewardCardSelectionControllerTests
{
    // 验证选择界面暂停后可以恢复进入界面前的时间倍率。
    [UnityTest]
    public IEnumerator PauseAndResume_RestoresPreviousTimeScale()
    {
        float originalTimeScale = Time.timeScale;
        GameObject gameObject = new("RewardSelection");
        gameObject.SetActive(false);
        gameObject.AddComponent<CanvasGroup>();
        gameObject.AddComponent<MMF_Player>();
        gameObject.AddComponent<RewardCardSelectionPanel>();
        RewardCardSelectionController controller =
            gameObject.AddComponent<RewardCardSelectionController>();

        try
        {
            Time.timeScale = 0.75f;
            Invoke(controller, "PauseGameTime");
            Assert.AreEqual(0f, Time.timeScale);

            Invoke(controller, "ResumeGameTime");
            Assert.AreEqual(0.75f, Time.timeScale);
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            Object.Destroy(gameObject);
        }

        yield return null;
    }

    // 调用控制器私有方法。
    private static void Invoke(object target, string methodName)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(target, null);
    }
}
```

Run:

```powershell
$unity = 'D:\UnityEditor\6000.3.14f1\Editor\Unity.exe'
$project = 'D:\UnityProject\Unity_Demo\BuilderDefender'
& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode `
  -testFilter 'RewardCardSelectionControllerTests' `
  -testResults "$project\Temp\PlayModeResults.xml" `
  -logFile "$project\Temp\PlayModeTests.log" -quit
```

Expected: the PlayMode test passes and restores `Time.timeScale` even when the assertion fails.

Manually verify the remaining presentation flow in Step 5: choices open only for non-empty candidates, one card is confirmed per offer, and `OnRewardApplied` updates HUD, Toast and Summary.

- [ ] **Step 5: Inspect scenes, prefabs and ScriptableObject assets**

In Unity Editor:

1. Open each gameplay scene under `Assets/Scenes`.
2. Check Console for Missing Script and serialization warnings.
3. Inspect reward card assets and verify every effect definition still has its Applier reference.
4. Inspect reward UI prefabs and verify moved MonoBehaviour scripts remain attached.
5. Inspect upgrade UI objects and verify `BuildingUpgradeButton` references `TowerUpgradeController`.
6. Enter Play Mode, complete a wave, select a reward, attack with a Tower and finish the next wave.

Expected: existing card draw, stat bonuses, arrow effects, kill growth, random max-star and wave-end healing behave as before.

- [ ] **Step 6: Check final working tree without changing Git state**

Run:

```powershell
git status --short
```

Expected: only the intended design document, implementation plan, moved scripts, `.meta` files, tests and direct caller changes appear. Do not stage or commit them without a new explicit user instruction.
