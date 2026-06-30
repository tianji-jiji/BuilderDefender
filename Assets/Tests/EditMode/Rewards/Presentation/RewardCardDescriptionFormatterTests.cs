using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 奖励卡牌描述格式化测试，保护数据职责拆分前后的显示结果。
/// </summary>
public class RewardCardDescriptionFormatterTests
{
    // 验证 Value 参数会按百分比和正向颜色生成描述文本。
    [Test]
    public void BuildDescriptionTextFormatsValueParameter()
    {
        ScriptableObject rewardCard = null;
        ScriptableObject effectDefinition = null;

        try
        {
            Type rewardCardType = GetRequiredType("RewardCardSo");
            Type effectDefinitionType = GetRequiredType("RewardEffectDefinitionSo");
            Type parameterConfigType = GetRequiredType("RewardEffectParameterConfig");
            Type cardEffectConfigType = GetRequiredType("RewardCardEffectConfig");
            Type formatterType = GetRequiredType("RewardCardDescriptionFormatter");

            effectDefinition = ScriptableObject.CreateInstance(effectDefinitionType);
            SetField(effectDefinitionType, effectDefinition, "displayName", "攻击");
            SetField(effectDefinitionType, effectDefinition, "descriptionTemplate", "{displayName} {value}");

            object parameterConfig = Activator.CreateInstance(parameterConfigType);
            SetField(parameterConfigType, parameterConfig, "parameterId", "Value");
            SetField(parameterConfigType, parameterConfig, "value", 0.1f);

            object cardEffectConfig = Activator.CreateInstance(cardEffectConfigType);
            SetField(cardEffectConfigType, cardEffectConfig, "effectDefinition", effectDefinition);
            SetListField(cardEffectConfigType, cardEffectConfig, "parameterConfigList", parameterConfig);

            rewardCard = ScriptableObject.CreateInstance(rewardCardType);
            SetListField(rewardCardType, rewardCard, "effectConfigList", cardEffectConfig);

            MethodInfo buildDescriptionMethod = formatterType.GetMethod(
                "BuildDescriptionText",
                BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(buildDescriptionMethod);

            string description = buildDescriptionMethod.Invoke(null, new object[] { rewardCard }) as string;

            Assert.That(description, Does.Contain("攻击"));
            Assert.That(description, Does.Contain("+10%"));
            Assert.That(description, Does.Contain("#55FF77"));
        }
        finally
        {
            DestroyImmediate(rewardCard);
            DestroyImmediate(effectDefinition);
        }
    }

    // 创建与字段声明类型一致的列表并写入测试元素。
    private static void SetListField(Type ownerType, object target, string fieldName, object element)
    {
        FieldInfo field = GetRequiredField(ownerType, fieldName);
        IList valueList = Activator.CreateInstance(field.FieldType) as IList;
        Assert.NotNull(valueList);
        valueList.Add(element);
        field.SetValue(target, valueList);
    }

    // 设置指定对象的私有实例字段。
    private static void SetField(Type type, object target, string fieldName, object value)
    {
        GetRequiredField(type, fieldName).SetValue(target, value);
    }

    // 获取指定类型的私有实例字段。
    private static FieldInfo GetRequiredField(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"未找到字段：{type.Name}.{fieldName}");
        return field;
    }

    // 从生产程序集取得指定类型。
    private static Type GetRequiredType(string typeName)
    {
        Type type = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.NotNull(type, $"未找到生产类型：{typeName}");
        return type;
    }

    // 立即销毁测试期间创建的 Unity 对象。
    private static void DestroyImmediate(Object unityObject)
    {
        if (unityObject)
        {
            Object.DestroyImmediate(unityObject);
        }
    }
}
