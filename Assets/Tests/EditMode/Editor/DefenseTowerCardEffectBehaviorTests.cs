using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class DefenseTowerCardEffectBehaviorTests
{
    private readonly List<ScriptableObject> _createdScriptableObjectList = new();

    // 清理测试创建的对象和资产实例。
    [TearDown]
    public void TearDown()
    {
        foreach (ScriptableObject scriptableObject in _createdScriptableObjectList)
        {
            if (scriptableObject)
            {
                Object.DestroyImmediate(scriptableObject);
            }
        }

        _createdScriptableObjectList.Clear();

        foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.name.StartsWith("DefenseTowerCardEffectTest_"))
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    // 验证额外箭效果按原触发次数请求额外发射。
    [Test]
    public void ExtraArrowEffect_RequestsExtraAttackOnTriggerCount()
    {
        DefenseTowerRuntimeEffectDispatcher dispatcher = CreateDispatcherWithHandler(
            CreateEffect<DefenseTowerExtraArrowApplierSo>(),
            new Dictionary<string, float>
            {
                { RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 2f },
                { RewardEffectParameterIds.EXTRA_ATTACK_COUNT, 3f }
            });

        DefenseTowerAttackContext firstAttackContext = new(null, null);
        DefenseTowerAttackContext secondAttackContext = new(null, null);

        dispatcher.OnAfterAttack(firstAttackContext);
        dispatcher.OnAfterAttack(secondAttackContext);

        Assert.AreEqual(0, firstAttackContext.ExtraAttackCount);
        Assert.AreEqual(3, secondAttackContext.ExtraAttackCount);
    }

    // 验证攻击扣血效果按原触发次数扣除防御塔生命。
    [Test]
    public void AttackHealthCostEffect_LosesHealthOnTriggerCount()
    {
        HealthSystem healthSystem = CreateHealthSystem(10);
        DefenseTowerRuntimeEffectDispatcher dispatcher = CreateDispatcherWithHandler(
            CreateEffect<DefenseTowerAttackHealthCostApplierSo>(),
            new Dictionary<string, float>
            {
                { RewardEffectParameterIds.TRIGGER_ATTACK_COUNT, 2f },
                { RewardEffectParameterIds.ATTACK_HEALTH_COST, 3f }
            });

        dispatcher.OnAfterAttack(new DefenseTowerAttackContext(null, healthSystem));
        dispatcher.OnAfterAttack(new DefenseTowerAttackContext(null, healthSystem));

        Assert.AreEqual(7, healthSystem.CurrentHealth);
    }

    // 验证波末回血效果仍然治疗已注册的防御塔。
    [Test]
    public void WaveEndHealEffect_HealsRegisteredDefenseTower()
    {
        HealthSystem healthSystem = CreateRegisteredDefenseBuilding(10);
        healthSystem.LoseHealth(5);
        DefenseTowerRuntimeEffectDispatcher dispatcher = CreateDispatcherWithHandler(
            CreateEffect<DefenseTowerWaveEndHealApplierSo>(),
            new Dictionary<string, float>
            {
                { RewardEffectParameterIds.WAVE_END_HEAL_PERCENT, 0.2f }
            });

        dispatcher.OnWaveCompleted(new DefenseTowerWaveContext(1));

        Assert.AreEqual(7, healthSystem.CurrentHealth);
    }

    // 创建并注册指定 Handler 到运行时。
    private DefenseTowerRuntimeEffectDispatcher CreateDispatcherWithHandler(DefenseTowerRewardApplierSo applier, Dictionary<string, float> parameterDic)
    {
        DefenseTowerRewardState defenseTowerRewardState = new();
        DefenseTowerRuntimeEffectDispatcher dispatcher = new();
        RewardEffectDefinitionSo definition = CreateEffect<RewardEffectDefinitionSo>();
        RewardCardEffectConfig config = CreateConfig(definition, parameterDic);
        SetField(definition, "handler", applier);
        RewardEffectApplyContext applyContext = new(null, defenseTowerRewardState, null, null, null, dispatcher);

        DefenseTowerRewardEffectApplier.ApplyEffects(new[] { config }, defenseTowerRewardState, applyContext);
        return dispatcher;
    }

    // 创建测试用效果实例。
    private T CreateEffect<T>() where T : ScriptableObject
    {
        T effect = ScriptableObject.CreateInstance<T>();
        _createdScriptableObjectList.Add(effect);
        return effect;
    }

    // 创建测试用奖励配置。
    private RewardCardEffectConfig CreateConfig(RewardEffectDefinitionSo definition, Dictionary<string, float> parameterDic)
    {
        RewardCardEffectConfig config = new();
        List<RewardEffectParameterConfig> parameterConfigList = new();
        SetField(config, "effectDefinition", definition);

        foreach (KeyValuePair<string, float> parameterPair in parameterDic)
        {
            RewardEffectParameterConfig parameterConfig = new();
            SetField(parameterConfig, "parameterId", parameterPair.Key);
            SetField(parameterConfig, "value", parameterPair.Value);
            parameterConfigList.Add(parameterConfig);
        }

        SetField(config, "parameterConfigList", parameterConfigList);
        return config;
    }

    // 创建指定生命值的 HealthSystem。
    private HealthSystem CreateHealthSystem(int maxHealth)
    {
        GameObject gameObject = new("DefenseTowerCardEffectTest_Health");
        HealthSystem healthSystem = gameObject.AddComponent<HealthSystem>();
        healthSystem.Init(maxHealth);
        return healthSystem;
    }

    // 创建并注册防御建筑用于测试波末回血。
    private HealthSystem CreateRegisteredDefenseBuilding(int maxHealth)
    {
        GameObject gameObject = new("DefenseTowerCardEffectTest_Building");
        HealthSystem healthSystem = gameObject.AddComponent<HealthSystem>();
        Building building = gameObject.AddComponent<Building>();
        BuildingSo buildingSo = ScriptableObject.CreateInstance<BuildingSo>();
        _createdScriptableObjectList.Add(buildingSo);
        buildingSo.maxHealth = maxHealth;
        buildingSo.buildingType = BuildingSo.BuildingType.Defense;
        SetField(building, "buildingSo", buildingSo);
        InvokePrivateMethod(building, "Awake");
        InvokePrivateMethod(building, "OnEnable");
        return healthSystem;
    }

    // 设置私有序列化字段。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    // 调用私有生命周期方法。
    private void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo methodInfo = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(methodInfo, $"Missing method {methodName}");
        methodInfo.Invoke(target, null);
    }
}
