using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class RewardEffectHandlerTests
{
    private readonly List<ScriptableObject> _createdScriptableObjectList = new();

    // 清理测试期间创建的 ScriptableObject。
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

    // 验证配置了 Handler 的效果会使用 Handler 执行。
    [Test]
    public void ApplyEffects_WithHandler_UsesHandler()
    {
        DefenseTowerRewardModifiers defenseTowerRewardModifiers = new();
        RewardEffectDefinitionSo definition = CreateDefinition();
        TestRewardEffectHandlerSo handler = CreateHandler();
        RewardEffectConfig config = CreateConfig(definition, 0.5f);
        RewardEffectContext context = new(null, defenseTowerRewardModifiers, null, null, null);
        SetField(definition, "handler", handler);

        DefenseTowerRewardConfigApplier.ApplyEffects(new[] { config }, defenseTowerRewardModifiers, context);

        Assert.AreEqual(1, handler.ApplyCount);
        Assert.AreSame(context, handler.LastContext);
        Assert.AreSame(config, handler.LastConfig);
        Assert.AreEqual(1f, defenseTowerRewardModifiers.AttackDamageMultiplier);
    }

    // 验证没有 Handler 的效果不会再进入旧 switch fallback。
    [Test]
    public void ApplyEffects_WithoutHandler_DoesNotApplyLegacyFallback()
    {
        DefenseTowerRewardModifiers defenseTowerRewardModifiers = new();
        RewardEffectDefinitionSo definition = CreateDefinition();
        RewardEffectConfig config = CreateConfig(definition, 0.25f);

        DefenseTowerRewardConfigApplier.ApplyEffects(new[] { config }, defenseTowerRewardModifiers);

        Assert.AreEqual(1f, defenseTowerRewardModifiers.AttackDamageMultiplier);
    }

    // 验证旧效果枚举和旧参数枚举已经从程序集里移除。
    [Test]
    public void LegacyEffectEnums_AreRemoved()
    {
        Assembly rewardAssembly = typeof(RewardEffectConfig).Assembly;

        Assert.IsNull(rewardAssembly.GetType("RewardEffectType"));
        Assert.IsNull(rewardAssembly.GetType("RewardEffectParameterKey"));
        Assert.IsNull(typeof(RewardEffectConfig).GetProperty("EffectType"));
        Assert.IsNull(typeof(RewardEffectDefinitionSo).GetProperty("EffectType"));
    }

    // 验证全新参数可以只用字符串 ID，不需要新增枚举。
    [Test]
    public void ParameterReader_CustomParameterId_ReadsWithoutEnumChange()
    {
        RewardEffectConfig config = new();
        RewardEffectParameterConfig parameterConfig = new();
        SetField(parameterConfig, "parameterId", "MaxStack");
        SetField(parameterConfig, "value", 7f);
        SetField(config, "parameterConfigList", new List<RewardEffectParameterConfig> { parameterConfig });

        float value = RewardEffectParameterReader.GetFloat(config, "MaxStack", 0f);

        Assert.AreEqual(7f, value);
    }

    // 验证效果描述可以使用字符串参数 ID 进行替换。
    [Test]
    public void BuildDescription_CustomParameterId_ReplacesCustomToken()
    {
        RewardEffectDefinitionSo definition = CreateDefinition();
        RewardEffectParameterDisplayDefinition displayDefinition = new();
        SetField(displayDefinition, "parameterId", "MaxStack");
        SetField(displayDefinition, "displayName", "最大层数");
        SetField(definition, "displayName", "击杀成长");
        SetField(definition, "descriptionTemplate", "击杀成长最大 {MaxStack} 层");
        SetField(definition, "parameterDisplayDefinitionList", new List<RewardEffectParameterDisplayDefinition> { displayDefinition });

        string description = definition.BuildDescription(new Dictionary<string, string>
        {
            { "MaxStack", "7" }
        });

        Assert.AreEqual("击杀成长最大 7 层", description);
    }

    // 验证项目内已有旧效果定义资产都已经迁移到 Handler。
    [Test]
    public void ExistingEffectDefinitions_AllHaveHandlers()
    {
        string[] definitionGuidArray = AssetDatabase.FindAssets("t:RewardEffectDefinitionSo", new[] { "Assets/ScriptableObjects/Rewards/EffectDefinitions" });
        List<string> missingHandlerPathList = new();

        foreach (string definitionGuid in definitionGuidArray)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(definitionGuid);
            RewardEffectDefinitionSo definition = AssetDatabase.LoadAssetAtPath<RewardEffectDefinitionSo>(assetPath);
            if (definition && !definition.Handler)
            {
                missingHandlerPathList.Add(assetPath);
            }
        }

        Assert.IsNotEmpty(definitionGuidArray, "No RewardEffectDefinitionSo assets found.");
        Assert.IsEmpty(missingHandlerPathList, "Missing RewardEffectHandlerSo on definitions:\n" + string.Join("\n", missingHandlerPathList));
    }

    // 创建测试用效果定义。
    private RewardEffectDefinitionSo CreateDefinition()
    {
        RewardEffectDefinitionSo definition = ScriptableObject.CreateInstance<RewardEffectDefinitionSo>();
        _createdScriptableObjectList.Add(definition);
        return definition;
    }

    // 创建测试用效果配置。
    private RewardEffectConfig CreateConfig(RewardEffectDefinitionSo definition, float value)
    {
        RewardEffectConfig config = new();
        SetField(config, "effectDefinition", definition);
        SetField(config, "value", value);
        return config;
    }

    // 创建测试用 Handler。
    private TestRewardEffectHandlerSo CreateHandler()
    {
        TestRewardEffectHandlerSo handler = ScriptableObject.CreateInstance<TestRewardEffectHandlerSo>();
        _createdScriptableObjectList.Add(handler);
        return handler;
    }

    // 设置私有序列化字段以构建测试数据。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }

    /// <summary>
    /// 测试用 Reward Handler，记录运行时调用参数。
    /// </summary>
    private class TestRewardEffectHandlerSo : RewardEffectHandlerSo
    {
        public int ApplyCount { get; private set; }
        public RewardEffectContext LastContext { get; private set; }
        public RewardEffectConfig LastConfig { get; private set; }

        // 记录 Handler 调用信息。
        public override void Apply(RewardEffectContext context, RewardEffectConfig config)
        {
            ApplyCount++;
            LastContext = context;
            LastConfig = config;
        }
    }
}
