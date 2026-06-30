# Rewards 分层重构设计

日期：2026-06-29  
状态：已批准

## 1. 目标

将 `Assets/Scripts/Rewards` 整理为职责清晰的分层结构，同时修正当前代码中数据、规则、执行、场景访问和 UI 相互混杂的问题。

本次重构优先保证：

- 目录能够反映代码职责。
- 领域状态和规则不直接依赖场景对象、单例或 UI。
- 奖励执行不直接控制 UI 组件。
- ScriptableObject 继续只承担可配置资产和效果策略入口。
- 现有奖励资产、Prefab 和场景引用在迁移后保持有效。
- 不保留旧类型包装器、兼容转发类或空的未来模块。

## 2. 范围

本次处理整个奖励模块以及与其存在直接耦合的少量建筑和 UI 代码。

包含：

- 奖励卡牌数据与抽取。
- 奖励获得历史。
- 通用效果应用流程。
- Tower 奖励状态、规则、效果和运行时触发。
- Tower 奖励访问场景对象的方式。
- 奖励卡牌、HUD、提示和摘要 UI。
- 奖励自定义 Inspector 与编辑器预设。
- 相关 EditMode 和 PlayMode 测试。

不包含：

- Home 奖励模块。
- Economy 奖励模块。
- 新增奖励玩法。
- 引入 asmdef 或命名空间。
- 重做现有 ScriptableObject Applier 机制。
- 与奖励无关的建筑、战斗或 UI 重构。

## 3. 目标目录

```text
Assets/Scripts/Rewards/
├── Data/
├── Domain/
│   ├── Cards/
│   └── Tower/
├── Application/
│   ├── Effects/
│   └── Tower/
├── Integration/
│   └── Tower/
├── Presentation/
│   ├── Cards/
│   └── Tower/
└── Editor/
```

只创建有实际代码的目录，不创建 Home、Economy 或其他空占位目录。

## 4. 分层职责

### 4.1 Data

保存由设计人员在 Inspector 中配置的静态数据：

- 奖励卡牌资产。
- 奖励卡池资产。
- 效果定义资产。
- 效果参数配置和显示配置。
- 稀有度、分类、参数 ID 等配置类型。

Data 不负责抽卡、执行奖励、访问场景或生成最终 UI 文本。

### 4.2 Domain

保存运行时状态和纯规则：

- 卡牌抽取规则与抽取上下文。
- 奖励获得记录和历史叠加。
- Tower 奖励累计状态。
- Tower 成本、战力和倍率计算。
- 不依赖场景对象的 Tower 奖励规则值。

Domain 不访问 `MonoBehaviour` 单例、场景对象、管理器或 UI。

### 4.3 Application

负责执行用例和协调 Domain：

- 应用一张奖励卡的全部效果。
- 调用 ScriptableObject Applier。
- 注册并分发 Tower 运行时触发效果。
- 保存单个触发效果的计数状态。
- 通过抽象接口请求外部游戏操作。

Application 不直接查找场景对象，也不直接控制 UI。

### 4.4 Integration

负责把奖励应用层连接到实际 Unity 游戏系统：

- 绑定 `WaveManager` 生命周期事件。
- 提供 Tower、建筑、基地生命和升级能力。
- 组装奖励运行时、历史、抽卡服务和外部依赖。
- 将外部游戏事件转发给 Application。

Integration 可以依赖 Buildings、Resources 和 Wave 等模块，但不会包含奖励计算规则。

### 4.5 Presentation

负责玩家能够看到和交互的奖励内容：

- 奖励卡牌选择界面。
- 奖励卡牌描述。
- 已获得奖励 HUD。
- 奖励获得提示。
- 奖励摘要面板。
- Tower 奖励摘要格式化。

Presentation 可以读取 Domain 状态并调用 Application 入口，但不能直接修改奖励状态。

### 4.6 Editor

只包含编辑器代码和编辑器预设。该目录必须是 Unity 识别的 `Editor` 目录。

## 5. 依赖规则

- Domain 不依赖 Application、Integration 或 Presentation。
- Application 可以依赖 Data 和 Domain。
- Application 通过 `ITowerRewardWorld` 请求场景操作。
- Integration 实现 `ITowerRewardWorld` 并访问实际游戏对象。
- Presentation 通过 `RewardRuntimeCoordinator` 提供的公开入口和事件与奖励系统通信。
- Data 中允许保存 `RewardEffectApplierSo` 资产引用，保留现有可配置效果策略，不引入效果 ID 注册表。
- Reward 专属 UI 统一位于 `Rewards/Presentation`，不再分散在 `Assets/Scripts/UI/Rewards`。

`RewardEffectDefinitionSo` 对 `RewardEffectApplierSo` 的序列化引用是本阶段唯一允许的反向类型引用。这是为了保留现有资产编辑方式和脚本 GUID；Data 只保存引用，不调用应用层流程或执行效果。由于本阶段不引入 asmdef，该例外通过代码审查约束。

## 6. 数据层调整

### 6.1 RewardCardSo

保留卡牌基础配置、卡牌效果列表和稳定 ID。继续保留现有 ScriptableObject 资产及其脚本 GUID。

### 6.2 RewardCardPoolSo

`RewardCardDrawPoolSo` 重命名为 `RewardCardPoolSo`，只保存：

- 卡牌列表。
- 每次候选数量。
- 是否允许同次抽取重复。

`DrawCards`、候选过滤和权重抽取逻辑移出该资产。

### 6.3 效果配置

将当前集中在少数文件中的配置类型按主类型拆分为独立文件：

- `RewardCardEffectConfig`
- `RewardEffectParameterConfig`
- `RewardEffectParameterIds`
- `RewardEffectDefinitionSo`
- 显示格式与影响类型

`RewardEffectParameterReader` 移到 `Application/Effects`。描述拼接和富文本颜色解析移到 Presentation。

## 7. 卡牌领域

### 7.1 RewardCardDrawService

新增纯 C# `RewardCardDrawService`，负责：

- 根据波次过滤卡牌。
- 根据最大选择次数过滤卡牌。
- 按权重抽取。
- 处理同次抽取是否允许重复。
- 返回只读候选结果。

抽卡随机数通过小型 `IRewardRandom` 接口提供。Integration 使用 Unity 随机数实现，EditMode 测试使用可控实现。

### 7.2 RewardHistory

`RewardCardAcquiredHistory` 拆分为纯 C# `RewardHistory`，不再继承 `MonoBehaviour`，也不再持有静态单例和静态事件。

`RewardHistory` 负责：

- 保存 `RewardCardRecord` 列表。
- 按卡牌 ID 合并叠加次数。
- 生成抽卡所需的已选择次数索引。
- 清空本局历史。

`RewardCardAcquiredRecord` 重命名为 `RewardCardRecord`。记录保存卡牌资产引用、首次获得波次和叠加次数，不保存格式化描述文本。

`RewardCardAcquiredContext` 重命名为 `RewardAppliedContext`，作为一次奖励应用完成后的只读通知数据。

波次索引由 Integration 在调用时传入，描述文本由 Presentation 根据卡牌数据生成。

## 8. Tower 领域

### 8.1 TowerRewardState

`TowerActiveRewards` 重命名为 `TowerRewardState`，仅负责保存和累计本局 Tower 奖励状态。

保留：

- 数值累计和边界约束。
- `TowerExtraAttackRule`。
- `TowerAttackHealthCostRule`。

移出：

- 对 `TowerRegistry` 或基地生命的查询。
- 战力评估方法。
- 依赖场景状态的最终防线判断。

### 8.2 Tower 规则

以下内容位于 `Domain/Tower`：

- `TowerBuildCostCalculator`
- `TowerPowerCalculator`，由 `TowerPowerEvaluator` 重命名。
- `TowerPowerSnapshot`
- 最终防线、联动和其他需要外部数值输入的计算规则。

规则方法通过参数接收三星塔数量、基地生命比例和附近塔状态，不自行查询场景。

## 9. Application 调整

### 9.1 通用效果应用

以下类型位于 `Application/Effects`：

- `RewardEffectApplierSo`
- `RewardEffectApplicationService`
- `RewardApplyContext`，由 `RewardEffectApplyContext` 重命名。
- `RewardEffectParameterReader`

`RewardApplyContext` 只保留真实使用的奖励运行时和外部接口，不再保存未使用的 `ResourceManager`、`WaveManager`、`BuildingPlacementManager` 或 Coordinator 引用。

### 9.2 Tower 效果

所有 Tower Applier 移动到 `Application/Tower/Effects`，继续保持一个效果一个 ScriptableObject 类型。

`TowerRewardApplierSo` 作为 Tower Applier 基类保留。

### 9.3 Tower 运行时

以下类型移动到 `Application/Tower/Runtime`：

- `TowerRewardRuntime`
- `TowerRewardTriggerDispatcher`
- Tower 触发能力接口。
- Tower 攻击、箭矢和击杀上下文。
- `TowerEffectState`，由 `TowerRewardRuntimeState` 重命名。

文件名必须与主类型名一致，`ITowerEnemyKilledRewardTrigger.cs` 重命名为 `ITowerEnemyKilledTrigger.cs`。

## 10. Integration 调整

### 10.1 RewardRuntimeCoordinator

`RewardRuntimeCoordinator` 移动到 Integration 根目录并作为唯一奖励场景入口，负责：

- 创建并持有 `RewardHistory`、`RewardCardDrawService` 和 `TowerRewardRuntime`。
- 绑定和解绑 `WaveManager.OnWaveCompleted`。
- 在波末先执行 Tower 波末奖励，再生成候选卡。
- 暴露 `ApplyReward` 和只读查询入口。
- 发布 `OnRewardChoicesReady`、`OnRewardApplied` 和 `OnActiveRewardsChanged`。

Coordinator 只协调，不包含抽卡、累计、格式化或 Tower 场景查询的具体算法。

### 10.2 ITowerRewardWorld

Application 定义 `ITowerRewardWorld`，Integration 提供 `TowerRewardWorld` 实现。接口只暴露当前奖励真正需要的操作：

- 获取基地生命比例。
- 获取三星 Tower 数量。
- 判断附近是否存在其他 Tower。
- 随机选择并升满一座可升级 Tower。
- 将指定 Tower 免费提升一级。
- 治疗全部防御 Tower。

`TowerImmediateExecutor` 和 `TowerWaveEndExecutor` 删除，Applier 通过 `ITowerRewardWorld` 请求这些操作。

### 10.3 建筑运行时索引

当前 `Rewards/Tower/Registry/TowerRegistry` 从 Rewards 中删除，其场景对象索引职责移动到 Buildings 模块，形成 `BuildingRuntimeRegistry`。

Buildings、Tower 战斗组件、基地生命和升级组件向 `BuildingRuntimeRegistry` 注册。`TowerRewardWorld` 只读取该注册表并转换为奖励需要的查询和命令。

### 10.4 升级职责

从 `BuildingUpgradeButton` 提取 `TowerUpgradeController` 到 `Assets/Scripts/Buildings/Runtime/Tower`：

- 持有升级配置和当前星级。
- 把升级等级应用到 `Building` 和 `TowerCombatSystem`。
- 提供付费升级、免费升一级、免费升满级和初始星级奖励入口。
- 发布星级变化事件供 UI 刷新。

`BuildingUpgradeButton` 只保留按钮监听、资源不足提示、星级视觉和交互状态。Rewards 不再引用 `BuildingUpgradeButton`。

## 11. Presentation 调整

`Assets/Scripts/UI/Rewards` 中的所有奖励专属 UI 脚本移动到 `Rewards/Presentation/Cards`，移动时保留 `.meta`。

主要调整：

- `RewardCardController` 重命名为 `RewardCardSelectionController`。
- Controller 订阅 `OnRewardChoicesReady`，不再自行访问卡池、历史或 `WaveManager`。
- 玩家选卡后仅调用 Coordinator 的 `ApplyReward`。
- `RewardCardDescriptionFormatter` 承担所有卡牌和效果描述生成。
- `TowerRewardSummaryFormatter` 移动到 `Presentation/Tower`。
- HUD、Toast 和 Summary 订阅 `OnRewardApplied`，不再依赖 `RewardCardAcquiredHistory` 静态事件。

只有实际生成候选卡并准备展示后才暂停游戏。空卡池不会打开界面，也不会改变 `Time.timeScale`。

## 12. Editor 调整

以下文件统一移动到 `Rewards/Editor`：

- `RewardEffectAuthoringPresets`
- `RewardCardPoolSoEditor`，由 `RewardCardDrawPoolSoEditor` 重命名。
- `RewardCardIdAutoGenerator`
- `RewardCardSoEditor`
- `RewardEffectDefinitionSoEditor`

编辑器校验继续负责参数范围、整数约束、显示规则和卡牌稳定 ID。运行时仍保留必要的边界保护。

## 13. 运行流程

1. `RewardRuntimeCoordinator` 收到波次结束事件。
2. Coordinator 通知 `TowerRewardRuntime` 执行波末触发效果。
3. Coordinator 使用卡池数据、当前波次和 `RewardHistory` 请求 `RewardCardDrawService` 生成候选卡。
4. Coordinator 发布 `OnRewardChoicesReady`。
5. `RewardCardSelectionController` 生成选项并暂停游戏。
6. 玩家确认卡牌后，Controller 调用 `ApplyReward`。
7. `RewardEffectApplicationService` 依次执行效果配置。
8. 常驻加成写入 `TowerRewardState`，运行时效果注册到 Dispatcher，场景操作通过 `ITowerRewardWorld` 执行。
9. 成功应用后，Coordinator 写入 `RewardHistory` 并发布 `OnRewardApplied` 和 `OnActiveRewardsChanged`。
10. Presentation、Tower 属性和反馈订阅者刷新。

Tower 攻击、箭矢和击杀事件继续调用 `TowerRewardRuntime` 的公开运行时入口，由 Dispatcher 分发到已注册效果。

## 14. 失败处理

- 缺少卡牌、效果定义或 Applier 时跳过对应项，并输出包含资产名的明确警告。
- 卡池没有可用卡牌时不发布展示事件，不暂停游戏。
- `TowerRewardWorld` 或必要场景依赖缺失时在初始化阶段报告错误。
- 非法参数优先由自定义 Inspector 阻止，运行时继续执行范围保护。
- 运行时触发器收到空上下文时安全返回。
- Tower 销毁或禁用时清理其触发计数。
- 不保留旧 API 包装器，全部调用方和测试同步迁移。

## 15. 资产迁移安全

- 所有脚本移动和重命名必须连同 `.meta` 一起完成。
- ScriptableObject Applier 脚本保留原 GUID，避免效果定义资产丢失引用。
- MonoBehaviour 脚本保留原 GUID，避免 Prefab 和场景组件丢失。
- 序列化字段重命名使用 `FormerlySerializedAs`。
- `CreateAssetMenu` 路径可以整理，但不替换现有资产。
- 每个迁移阶段完成后立即触发 Unity 编译并检查 Missing Script。

## 16. 测试设计

### 16.1 Domain EditMode

- 波次和最大选择次数过滤。
- 权重抽取和重复设置。
- 空卡池和无有效卡牌。
- 历史记录创建、叠加、清空和计数索引。
- Tower 奖励累计、边界约束和规则合并。
- 成本、战力、最终防线和联动计算。

### 16.2 Application EditMode

- 多效果依次应用。
- 无效效果安全跳过。
- 运行时能力自动注册。
- 每座 Tower 的触发计数隔离。
- Tower 销毁后的计数清理。
- 使用假的 `ITowerRewardWorld` 验证治疗、升星和场景查询调用。

### 16.3 Integration EditMode/PlayMode

- `BuildingRuntimeRegistry` 注册和注销。
- `TowerRewardWorld` 的查询与命令。
- `TowerUpgradeController` 的付费和免费升级。
- Coordinator 的波次订阅、解绑和事件顺序。

### 16.4 Presentation PlayMode

- 候选卡生成和清理。
- 展示后暂停、关闭后恢复。
- 空候选不暂停。
- 单次展示只能确认一张卡。
- HUD、Toast 和 Summary 响应 `OnRewardApplied`。

现有 `DefenseTower*` 反射字符串测试更新为当前类型的直接 API 测试。旧生产类型和兼容包装器不保留。

## 17. 验收标准

- `Rewards` 目录只包含 Data、Domain、Application、Integration、Presentation 和 Editor 层。
- Home 和 Economy 不存在占位目录或占位代码。
- Domain 不直接引用场景管理器、UI 类型或静态单例。
- Rewards 不再引用 `BuildingUpgradeButton`。
- `RewardCardPoolSo` 不包含抽卡算法。
- `RewardHistory` 是纯 C# 类型，不依赖 WaveManager 或格式化器。
- `TowerRewardState` 不查询场景。
- Reward 专属 UI 全部位于 Presentation。
- 旧目录和旧类型全部删除。
- Unity 编译无错误、无 Missing Script。
- 相关 EditMode 和 PlayMode 测试通过。
