using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class DefenseTowerCardEffectDispatcherTests
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
        DefenseTowerCardEffectDispatcher dispatcher = new();
        OrderedDefenseTowerEffect firstEffect = CreateEffect<OrderedDefenseTowerEffect>();
        OrderedDefenseTowerEffect secondEffect = CreateEffect<OrderedDefenseTowerEffect>();
        List<string> executionOrderList = new();
        firstEffect.Init("First", executionOrderList);
        secondEffect.Init("Second", executionOrderList);

        dispatcher.RegisterEffect(firstEffect, new RewardEffectConfig());
        dispatcher.RegisterEffect(secondEffect, new RewardEffectConfig());
        dispatcher.OnAfterAttack(new DefenseTowerAttackContext(null, null));

        CollectionAssert.AreEqual(new[] { "First", "Second" }, executionOrderList);
    }

    // 验证纯数值 Handler 不会进入运行时效果列表。
    [Test]
    public void ApplyEffects_PureStatHandler_DoesNotRegisterRuntimeEffect()
    {
        DefenseTowerRewardModifiers defenseTowerRewardModifiers = new();
        DefenseTowerCardEffectDispatcher dispatcher = new();
        RewardEffectDefinitionSo definition = CreateDefinition();
        DefenseTowerAttackSpeedHandlerSo handler = CreateEffect<DefenseTowerAttackSpeedHandlerSo>();
        RewardEffectConfig config = CreateConfig(definition, RewardEffectParameterIds.VALUE, 0.25f);
        SetField(definition, "handler", handler);
        RewardEffectContext context = new(null, defenseTowerRewardModifiers, null, null, null, dispatcher);

        DefenseTowerRewardConfigApplier.ApplyEffects(new[] { config }, defenseTowerRewardModifiers, context);

        Assert.AreEqual(0, dispatcher.Count);
        Assert.Less(defenseTowerRewardModifiers.AttackIntervalMultiplier, 1f);
    }

    // 验证同一效果多次获得时会按多条实例堆叠执行。
    [Test]
    public void RuntimeEffect_CanRegisterSameEffectMultipleTimes()
    {
        DefenseTowerCardEffectDispatcher dispatcher = new();
        OrderedDefenseTowerEffect effect = CreateEffect<OrderedDefenseTowerEffect>();
        List<string> executionOrderList = new();
        effect.Init("Stacked", executionOrderList);

        dispatcher.RegisterEffect(effect, new RewardEffectConfig());
        dispatcher.RegisterEffect(effect, new RewardEffectConfig());
        dispatcher.OnAfterAttack(new DefenseTowerAttackContext(null, null));

        Assert.AreEqual(2, dispatcher.Count);
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
    private class OrderedDefenseTowerEffect : DefenseTowerRewardHandlerSo
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
        public override void OnAfterAttack(DefenseTowerRuntimeEffectInstance instance, DefenseTowerAttackContext context)
        {
            _executionOrderList.Add(_label);
        }
    }
}
