using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class DefenseCardEffectRuntimeTests
{
    private readonly List<ScriptableObject> _createdScriptableObjectList = new();

    // 清理测试中创建的 ScriptableObject。
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
    }

    // 验证运行时效果会按注册顺序执行。
    [Test]
    public void RuntimeEffect_ExecutesInRegisteredOrder()
    {
        DefenseCardEffectRuntime runtime = new();
        OrderedDefenseEffect firstEffect = CreateEffect<OrderedDefenseEffect>();
        OrderedDefenseEffect secondEffect = CreateEffect<OrderedDefenseEffect>();
        List<string> executionOrderList = new();
        firstEffect.Init("First", executionOrderList);
        secondEffect.Init("Second", executionOrderList);

        runtime.RegisterEffect(firstEffect, new RewardEffectConfig());
        runtime.RegisterEffect(secondEffect, new RewardEffectConfig());
        runtime.OnAfterAttack(new DefenseAttackContext(null, null));

        CollectionAssert.AreEqual(new[] { "First", "Second" }, executionOrderList);
    }

    // 验证纯数值 Handler 不会进入运行时效果列表。
    [Test]
    public void ApplyEffects_PureStatHandler_DoesNotRegisterRuntimeEffect()
    {
        DefenseRewardState defenseRewardState = new();
        DefenseCardEffectRuntime runtime = new();
        RewardEffectDefinitionSo definition = CreateDefinition();
        DefenseAttackSpeedHandlerSo handler = CreateEffect<DefenseAttackSpeedHandlerSo>();
        RewardEffectConfig config = CreateConfig(definition, RewardEffectParameterIds.VALUE, 0.25f);
        SetField(definition, "handler", handler);
        RewardEffectContext context = new(null, defenseRewardState, null, null, null, runtime);

        DefenseRewardEffectApplier.ApplyEffects(new[] { config }, defenseRewardState, context);

        Assert.AreEqual(0, runtime.Count);
        Assert.Less(defenseRewardState.AttackIntervalMultiplier, 1f);
    }

    // 验证同一效果多次获得时会按多条实例堆叠执行。
    [Test]
    public void RuntimeEffect_CanRegisterSameEffectMultipleTimes()
    {
        DefenseCardEffectRuntime runtime = new();
        OrderedDefenseEffect effect = CreateEffect<OrderedDefenseEffect>();
        List<string> executionOrderList = new();
        effect.Init("Stacked", executionOrderList);

        runtime.RegisterEffect(effect, new RewardEffectConfig());
        runtime.RegisterEffect(effect, new RewardEffectConfig());
        runtime.OnAfterAttack(new DefenseAttackContext(null, null));

        Assert.AreEqual(2, runtime.Count);
        CollectionAssert.AreEqual(new[] { "Stacked", "Stacked" }, executionOrderList);
    }

    // 创建测试用效果定义。
    private RewardEffectDefinitionSo CreateDefinition()
    {
        RewardEffectDefinitionSo definition = ScriptableObject.CreateInstance<RewardEffectDefinitionSo>();
        _createdScriptableObjectList.Add(definition);
        return definition;
    }

    // 创建测试用效果实例。
    private T CreateEffect<T>() where T : ScriptableObject
    {
        T effect = ScriptableObject.CreateInstance<T>();
        _createdScriptableObjectList.Add(effect);
        return effect;
    }

    // 创建带单个参数的奖励效果配置。
    private RewardEffectConfig CreateConfig(RewardEffectDefinitionSo definition, string parameterId, float value)
    {
        RewardEffectConfig config = new();
        RewardEffectParameterConfig parameterConfig = new();
        SetField(config, "effectDefinition", definition);
        SetField(parameterConfig, "parameterId", parameterId);
        SetField(parameterConfig, "value", value);
        SetField(config, "parameterConfigList", new List<RewardEffectParameterConfig> { parameterConfig });
        return config;
    }

    // 设置私有序列化字段。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    /// <summary>
    /// 测试用防御塔运行时效果，记录攻击后钩子执行顺序。
    /// </summary>
    private class OrderedDefenseEffect : DefenseRewardHandlerSo
    {
        private string _label;
        private List<string> _executionOrderList;

        public override bool ShouldRegisterRuntimeEffect => true;

        // 初始化测试记录目标。
        public void Init(string label, List<string> executionOrderList)
        {
            _label = label;
            _executionOrderList = executionOrderList;
        }

        // 记录攻击后钩子执行顺序。
        public override void OnAfterAttack(DefenseCardEffectInstance instance, DefenseAttackContext context)
        {
            _executionOrderList.Add(_label);
        }
    }
}
