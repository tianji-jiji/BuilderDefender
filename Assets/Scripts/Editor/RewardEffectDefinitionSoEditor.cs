using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励效果定义的自定义 Inspector，负责编辑 Handler、描述模板和字符串参数显示规则。
/// </summary>
[CustomEditor(typeof(RewardEffectDefinitionSo))]
public class RewardEffectDefinitionSoEditor : Editor
{
    private SerializedProperty _displayNameProp;
    private SerializedProperty _descriptionTemplateProp;
    private SerializedProperty _handlerProp;
    private SerializedProperty _parameterDisplayDefinitionListProp;

    // 缓存序列化字段引用。
    private void OnEnable()
    {
        _displayNameProp = serializedObject.FindProperty("displayName");
        _descriptionTemplateProp = serializedObject.FindProperty("descriptionTemplate");
        _handlerProp = serializedObject.FindProperty("handler");
        _parameterDisplayDefinitionListProp = serializedObject.FindProperty("parameterDisplayDefinitionList");
    }

    // 绘制奖励效果定义 Inspector。
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicInfo();
        EditorGUILayout.Space(8f);
        DrawDescriptionSettings();
        EditorGUILayout.Space(8f);
        DrawParameterDisplayDefinitions();
        EditorGUILayout.Space(8f);
        DrawValidationMessages();

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制基础信息。
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("显示名称"));
        EditorGUILayout.PropertyField(_handlerProp, new GUIContent("效果 Handler"));
    }

    // 绘制描述模板设置。
    private void DrawDescriptionSettings()
    {
        EditorGUILayout.LabelField("描述模板", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_descriptionTemplateProp, new GUIContent("描述模板"));
    }

    // 绘制参数显示规则列表。
    private void DrawParameterDisplayDefinitions()
    {
        EditorGUILayout.LabelField("参数显示规则", EditorStyles.boldLabel);

        if (GUILayout.Button("新增参数规则"))
        {
            AddParameterDisplayDefinition();
        }

        if (_parameterDisplayDefinitionListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("没有参数规则时，会使用默认 {value} 文案和百分比显示。", MessageType.Info);
            return;
        }

        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            DrawParameterDisplayDefinition(parameterDisplayDefinitionProp, i);
        }
    }

    // 绘制单个参数显示规则。
    private void DrawParameterDisplayDefinition(SerializedProperty parameterDisplayDefinitionProp, int index)
    {
        SerializedProperty parameterIdProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterId");
        SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
        SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
        SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

        string parameterTitle = string.IsNullOrWhiteSpace(parameterIdProp.stringValue) ? "未命名参数" : parameterIdProp.stringValue.Trim();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{index + 1}. {parameterTitle}", EditorStyles.boldLabel);
        if (GUILayout.Button("删除", GUILayout.Width(56f)))
        {
            _parameterDisplayDefinitionListProp.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(parameterIdProp, new GUIContent("参数 ID"));
        EditorGUILayout.PropertyField(displayNameProp, new GUIContent("显示名称"));
        DrawEnumPopup(valueFormatProp, "显示方式", RewardEffectAuthoringPresets.ValueFormatDisplayNameArray);
        DrawEnumPopup(autoImpactRuleProp, "颜色规则", RewardEffectAuthoringPresets.AutoImpactRuleDisplayNameArray);

        if (GUILayout.Button("按参数 ID 填充默认显示设置"))
        {
            ApplyParameterDefaults(parameterIdProp, displayNameProp, valueFormatProp, autoImpactRuleProp);
        }

        EditorGUILayout.EndVertical();
    }

    // 新增一条参数显示规则。
    private void AddParameterDisplayDefinition()
    {
        int index = _parameterDisplayDefinitionListProp.arraySize;
        _parameterDisplayDefinitionListProp.InsertArrayElementAtIndex(index);
        SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(index);

        SerializedProperty parameterIdProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterId");
        SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
        SerializedProperty templateTokenProp = parameterDisplayDefinitionProp.FindPropertyRelative("templateToken");
        SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
        SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

        SetStringIfDifferent(parameterIdProp, GetFirstUnusedParameterId());
        SetStringIfDifferent(templateTokenProp, string.Empty);
        ApplyParameterDefaults(parameterIdProp, displayNameProp, valueFormatProp, autoImpactRuleProp);
    }

    // 按参数 ID 填充默认显示设置。
    private void ApplyParameterDefaults(SerializedProperty parameterIdProp, SerializedProperty displayNameProp, SerializedProperty valueFormatProp, SerializedProperty autoImpactRuleProp)
    {
        string parameterId = string.IsNullOrWhiteSpace(parameterIdProp.stringValue) ? RewardEffectParameterIds.VALUE : parameterIdProp.stringValue.Trim();
        SetStringIfDifferent(parameterIdProp, parameterId);
        SetStringIfDifferent(displayNameProp, RewardEffectAuthoringPresets.GetParameterDisplayName(parameterId));
        SetEnumIndexIfDifferent(valueFormatProp, (int)RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterId));
        SetEnumIndexIfDifferent(autoImpactRuleProp, (int)RewardEffectAuthoringPresets.GetDefaultAutoImpactRule(parameterId));
    }

    // 绘制配置校验信息。
    private void DrawValidationMessages()
    {
        List<string> validationMessageList = BuildValidationMessageList();
        foreach (string validationMessage in validationMessageList)
        {
            EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
        }
    }

    // 生成配置校验信息。
    private List<string> BuildValidationMessageList()
    {
        List<string> validationMessageList = new();

        if (_handlerProp.objectReferenceValue == null)
        {
            validationMessageList.Add("没有绑定效果 Handler，这个效果不会产生实际玩法效果。");
        }

        HashSet<string> parameterIdSet = new();
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterId");
            string parameterId = string.IsNullOrWhiteSpace(parameterIdProp.stringValue) ? RewardEffectParameterIds.VALUE : parameterIdProp.stringValue.Trim();

            if (!parameterIdSet.Add(parameterId))
            {
                validationMessageList.Add($"参数 ID 重复：{parameterId}");
            }
        }

        return validationMessageList;
    }

    // 查找第一个未使用的常用参数 ID。
    private string GetFirstUnusedParameterId()
    {
        string[] candidateArray =
        {
            RewardEffectParameterIds.VALUE,
            RewardEffectParameterIds.TRIGGER_ATTACK_COUNT,
            RewardEffectParameterIds.EXTRA_ATTACK_COUNT,
            RewardEffectParameterIds.ATTACK_SPEED_MULTIPLIER,
            RewardEffectParameterIds.LINK_RADIUS
        };

        foreach (string candidate in candidateArray)
        {
            if (!IsParameterIdUsed(candidate))
            {
                return candidate;
            }
        }

        return "NewParameter";
    }

    // 判断参数 ID 是否已被当前定义使用。
    private bool IsParameterIdUsed(string parameterId)
    {
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterId");
            string usedParameterId = string.IsNullOrWhiteSpace(parameterIdProp.stringValue) ? RewardEffectParameterIds.VALUE : parameterIdProp.stringValue.Trim();
            if (usedParameterId == parameterId)
            {
                return true;
            }
        }

        return false;
    }

    // 绘制带中文显示名的枚举弹窗。
    private void DrawEnumPopup(SerializedProperty enumProp, string label, string[] displayNameArray)
    {
        int selectedIndex = Mathf.Clamp(enumProp.enumValueIndex, 0, displayNameArray.Length - 1);
        int newSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayNameArray);
        SetEnumIndexIfDifferent(enumProp, newSelectedIndex);
    }

    // 设置字符串属性，避免无意义脏标记。
    private void SetStringIfDifferent(SerializedProperty stringProp, string value)
    {
        if (stringProp.stringValue != value)
        {
            stringProp.stringValue = value;
        }
    }

    // 设置枚举索引，避免无意义脏标记。
    private void SetEnumIndexIfDifferent(SerializedProperty enumProp, int value)
    {
        if (enumProp.enumValueIndex != value)
        {
            enumProp.enumValueIndex = value;
        }
    }
}
