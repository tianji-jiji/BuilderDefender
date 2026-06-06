using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励效果定义的中文简化检视面板，负责让设计者配置效果文案和卡牌数值输入项。
/// </summary>
[CustomEditor(typeof(RewardEffectDefinitionSo))]
public class RewardEffectDefinitionSoEditor : Editor
{
    private const int BUTTON_WIDTH = 48;
    private const int DESCRIPTION_MIN_HEIGHT = 48;

    private SerializedProperty _effectTypeProp;
    private SerializedProperty _displayNameProp;
    private SerializedProperty _useCustomDescriptionProp;
    private SerializedProperty _descriptionTemplateProp;
    private SerializedProperty _parameterDisplayDefinitionListProp;

    // 缓存需要反复使用的序列化字段。
    private void OnEnable()
    {
        _effectTypeProp = serializedObject.FindProperty("effectType");
        _displayNameProp = serializedObject.FindProperty("displayName");
        _useCustomDescriptionProp = serializedObject.FindProperty("useCustomDescription");
        _descriptionTemplateProp = serializedObject.FindProperty("descriptionTemplate");
        _parameterDisplayDefinitionListProp = serializedObject.FindProperty("parameterDisplayDefinitionList");
    }

    // 绘制奖励效果定义的简化检视面板。
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicInfo();
        ApplyEmptyDefaults();
        DrawDescriptionSettings();
        DrawParameterDisplayRuleList();

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制效果基础信息。
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("效果", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        DrawEnumPopup(_effectTypeProp, "玩法效果", RewardEffectAuthoringPresets.EffectTypeDisplayNameArray);
        if (EditorGUI.EndChangeCheck())
        {
            ApplyEffectTypeDefaults();
        }

        EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("效果名"));
    }

    // 绘制描述设置，默认自动描述，需要时允许中文占位符自定义。
    private void DrawDescriptionSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("描述", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_useCustomDescriptionProp, new GUIContent("自定义描述"));
        if (EditorGUI.EndChangeCheck() && !_useCustomDescriptionProp.boolValue)
        {
            SetStringIfDifferent(_descriptionTemplateProp, RewardEffectAuthoringPresets.GetDefaultDescriptionTemplate(_effectTypeProp.enumValueIndex));
        }

        if (_useCustomDescriptionProp.boolValue)
        {
            DrawCustomDescriptionEditor();
            return;
        }

        SetStringIfDifferent(_descriptionTemplateProp, RewardEffectAuthoringPresets.GetDefaultDescriptionTemplate(_effectTypeProp.enumValueIndex));
        EditorGUILayout.LabelField("自动描述", ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue));
    }

    // 绘制自定义描述文本框和可插入数值按钮。
    private void DrawCustomDescriptionEditor()
    {
        string displayDescription = ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue);

        EditorGUI.BeginChangeCheck();
        string newDisplayDescription = EditorGUILayout.TextArea(displayDescription, GUILayout.MinHeight(DESCRIPTION_MIN_HEIGHT));
        if (EditorGUI.EndChangeCheck())
        {
            _descriptionTemplateProp.stringValue = ConvertDisplayTextToTemplate(newDisplayDescription);
        }

        DrawInsertParameterButtons();
        DrawDescriptionValidation();
    }

    // 绘制可插入的中文参数按钮。
    private void DrawInsertParameterButtons()
    {
        if (_parameterDisplayDefinitionListProp.arraySize <= 0)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("插入数值");

        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            string displayName = GetParameterRuleDisplayName(parameterDisplayDefinitionProp);
            if (GUILayout.Button(displayName))
            {
                InsertParameterPlaceholder(displayName);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // 绘制自定义描述中的错误提示。
    private void DrawDescriptionValidation()
    {
        string displayDescription = ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue);
        string invalidPlaceholder = FindInvalidDisplayPlaceholder(displayDescription);
        if (!string.IsNullOrWhiteSpace(invalidPlaceholder))
        {
            EditorGUILayout.HelpBox($"未知数值：{invalidPlaceholder}", MessageType.Warning);
        }

        if (displayDescription.Contains("{") || displayDescription.Contains("}"))
        {
            EditorGUILayout.HelpBox("不要手写大括号参数，使用“插入数值”按钮。", MessageType.Warning);
        }
    }

    // 绘制参数显示规则列表。
    private void DrawParameterDisplayRuleList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("数值", EditorStyles.boldLabel);

        if (_parameterDisplayDefinitionListProp.arraySize <= 0)
        {
            EditorGUILayout.LabelField("无");
        }

        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            if (DrawParameterDisplayRule(parameterDisplayDefinitionProp, i))
            {
                break;
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加数值"))
        {
            AddParameterDisplayRule();
        }

        if (GUILayout.Button("按玩法填默认数值"))
        {
            ApplyDefaultParameterRules(true);
        }

        EditorGUILayout.EndHorizontal();
    }

    // 绘制单条参数显示规则，返回 true 表示列表结构已经变化。
    private bool DrawParameterDisplayRule(SerializedProperty parameterDisplayDefinitionProp, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"数值 {index + 1}", EditorStyles.boldLabel);

        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
        SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
        SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

        EditorGUI.BeginChangeCheck();
        DrawSuggestedParameterKeyPopup(parameterKeyProp, "数值作用");
        if (EditorGUI.EndChangeCheck())
        {
            RewardEffectParameterKey parameterKey = (RewardEffectParameterKey)parameterKeyProp.enumValueIndex;
            SetStringIfDifferent(displayNameProp, RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyProp.enumValueIndex));
            SetStringIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("templateToken"), string.Empty);
            SetEnumIndexIfDifferent(valueFormatProp, (int)RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterKey));
            SetEnumIndexIfDifferent(autoImpactRuleProp, (int)RewardEffectAuthoringPresets.GetDefaultAutoImpactRule(parameterKey));
        }

        if (string.IsNullOrWhiteSpace(displayNameProp.stringValue))
        {
            SetStringIfDifferent(displayNameProp, RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyProp.enumValueIndex));
        }

        EditorGUILayout.PropertyField(displayNameProp, new GUIContent("显示名"));
        DrawEnumPopup(valueFormatProp, "显示方式", RewardEffectAuthoringPresets.ValueFormatDisplayNameArray);
        DrawEnumPopup(autoImpactRuleProp, "颜色", RewardEffectAuthoringPresets.AutoImpactRuleDisplayNameArray);

        bool listChanged = DrawParameterButtons(index);
        EditorGUILayout.EndVertical();
        return listChanged;
    }

    // 绘制单个数值的排序和删除按钮。
    private bool DrawParameterButtons(int index)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = index > 0;
        if (GUILayout.Button("上移", GUILayout.Width(BUTTON_WIDTH)))
        {
            _parameterDisplayDefinitionListProp.MoveArrayElement(index, index - 1);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            return true;
        }

        GUI.enabled = index < _parameterDisplayDefinitionListProp.arraySize - 1;
        if (GUILayout.Button("下移", GUILayout.Width(BUTTON_WIDTH)))
        {
            _parameterDisplayDefinitionListProp.MoveArrayElement(index, index + 1);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            return true;
        }

        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("删除", GUILayout.Width(BUTTON_WIDTH)))
        {
            _parameterDisplayDefinitionListProp.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            return true;
        }

        EditorGUILayout.EndHorizontal();
        return false;
    }

    // 玩法效果变化时自动填充名字，并只在没有数值时填入默认数值。
    private void ApplyEffectTypeDefaults()
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        SetStringIfDifferent(_displayNameProp, RewardEffectAuthoringPresets.GetEffectTypeDisplayName(effectTypeIndex));

        if (!_useCustomDescriptionProp.boolValue)
        {
            SetStringIfDifferent(_descriptionTemplateProp, RewardEffectAuthoringPresets.GetDefaultDescriptionTemplate(effectTypeIndex));
        }

        if (_parameterDisplayDefinitionListProp.arraySize <= 0)
        {
            ApplyDefaultParameterRules(true);
        }
    }

    // 空字段自动补齐，避免手写程序占位符。
    private void ApplyEmptyDefaults()
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        if (string.IsNullOrWhiteSpace(_displayNameProp.stringValue))
        {
            SetStringIfDifferent(_displayNameProp, RewardEffectAuthoringPresets.GetEffectTypeDisplayName(effectTypeIndex));
        }

        if (string.IsNullOrWhiteSpace(_descriptionTemplateProp.stringValue))
        {
            SetStringIfDifferent(_descriptionTemplateProp, RewardEffectAuthoringPresets.GetDefaultDescriptionTemplate(effectTypeIndex));
        }
    }

    // 添加一条新的数值显示规则。
    private void AddParameterDisplayRule()
    {
        int newIndex = _parameterDisplayDefinitionListProp.arraySize;
        _parameterDisplayDefinitionListProp.InsertArrayElementAtIndex(newIndex);

        RewardEffectParameterKey parameterKey = GetFirstUnusedParameterKey();
        SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(newIndex);
        ApplyParameterRuleValues(
            parameterDisplayDefinitionProp,
            parameterKey,
            RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterKey),
            RewardEffectAuthoringPresets.GetDefaultAutoImpactRule(parameterKey),
            string.Empty,
            true);
    }

    // 按当前玩法效果填入默认数值规则。
    private void ApplyDefaultParameterRules(bool shouldResetDisplaySettings)
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        int parameterRulePresetCount = RewardEffectAuthoringPresets.GetDefaultParameterDisplayPresetCount(effectTypeIndex);
        _parameterDisplayDefinitionListProp.arraySize = parameterRulePresetCount;

        for (int i = 0; i < parameterRulePresetCount; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            ApplyParameterRuleValues(
                parameterDisplayDefinitionProp,
                RewardEffectAuthoringPresets.GetDefaultParameterKey(effectTypeIndex, i),
                RewardEffectAuthoringPresets.GetDefaultParameterValueFormat(effectTypeIndex, i),
                RewardEffectAuthoringPresets.GetDefaultParameterAutoImpactRule(effectTypeIndex, i),
                RewardEffectAuthoringPresets.GetDefaultParameterDisplayName(effectTypeIndex, i),
                shouldResetDisplaySettings);
        }
    }

    // 把预设参数规则写入序列化字段。
    private void ApplyParameterRuleValues(SerializedProperty parameterDisplayDefinitionProp, RewardEffectParameterKey parameterKey, RewardEffectValueFormat valueFormat, RewardEffectAutoImpactRule autoImpactRule, string displayNameOverride, bool shouldResetDisplaySettings)
    {
        int parameterKeyIndex = (int)parameterKey;
        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        bool parameterKeyChanged = parameterKeyProp.enumValueIndex != parameterKeyIndex;

        SetEnumIndexIfDifferent(parameterKeyProp, parameterKeyIndex);
        string displayName = string.IsNullOrWhiteSpace(displayNameOverride) ? RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyIndex) : displayNameOverride;
        SetStringIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("displayName"), displayName);
        SetStringIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("templateToken"), string.Empty);

        if (shouldResetDisplaySettings || parameterKeyChanged)
        {
            SetEnumIndexIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat"), (int)valueFormat);
            SetEnumIndexIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule"), (int)autoImpactRule);
        }
    }

    // 立即插入中文参数占位符并刷新检视面板。
    private void InsertParameterPlaceholder(string displayName)
    {
        string displayDescription = ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue);
        string separator = string.IsNullOrWhiteSpace(displayDescription) || displayDescription.EndsWith(" ") ? string.Empty : " ";
        string newDisplayDescription = $"{displayDescription}{separator}[{displayName}]";

        SetStringIfDifferent(_descriptionTemplateProp, ConvertDisplayTextToTemplate(newDisplayDescription));
        GUI.FocusControl(null);
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    // 把底层模板转换成面板里显示的中文占位符文本。
    private string ConvertTemplateToDisplayText(string template)
    {
        string displayText = template;
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            string displayName = GetParameterRuleDisplayName(parameterDisplayDefinitionProp);
            string token = GetParameterRuleToken(parameterDisplayDefinitionProp);

            displayText = displayText.Replace(token, $"[{displayName}]");
            if (GetParameterRuleKey(parameterDisplayDefinitionProp) == RewardEffectParameterKey.Value)
            {
                displayText = displayText.Replace("{value}", $"[{displayName}]");
            }
        }

        return displayText.Replace("{displayName}", _displayNameProp.stringValue);
    }

    // 把面板里的中文占位符文本转换成底层模板。
    private string ConvertDisplayTextToTemplate(string displayText)
    {
        string template = displayText;
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            string displayName = GetParameterRuleDisplayName(parameterDisplayDefinitionProp);
            string token = GetParameterRuleToken(parameterDisplayDefinitionProp);

            template = template.Replace($"[{displayName}]", token);
        }

        return template;
    }

    // 查找未匹配到当前效果数值的中文占位符。
    private string FindInvalidDisplayPlaceholder(string displayText)
    {
        int searchIndex = 0;
        while (searchIndex < displayText.Length)
        {
            int startIndex = displayText.IndexOf('[', searchIndex);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            int endIndex = displayText.IndexOf(']', startIndex + 1);
            if (endIndex < 0)
            {
                return displayText.Substring(startIndex);
            }

            string placeholder = displayText.Substring(startIndex, endIndex - startIndex + 1);
            if (!IsKnownDisplayPlaceholder(placeholder))
            {
                return placeholder;
            }

            searchIndex = endIndex + 1;
        }

        return string.Empty;
    }

    // 判断中文占位符是否属于当前效果的数值。
    private bool IsKnownDisplayPlaceholder(string placeholder)
    {
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            string displayName = GetParameterRuleDisplayName(parameterDisplayDefinitionProp);
            if (placeholder == $"[{displayName}]")
            {
                return true;
            }
        }

        return false;
    }

    // 获取参数规则的显示名。
    private string GetParameterRuleDisplayName(SerializedProperty parameterDisplayDefinitionProp)
    {
        SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
        if (!string.IsNullOrWhiteSpace(displayNameProp.stringValue))
        {
            return displayNameProp.stringValue;
        }

        return RewardEffectAuthoringPresets.GetParameterDisplayName((int)GetParameterRuleKey(parameterDisplayDefinitionProp));
    }

    // 获取参数规则的底层模板占位符。
    private string GetParameterRuleToken(SerializedProperty parameterDisplayDefinitionProp)
    {
        SerializedProperty templateTokenProp = parameterDisplayDefinitionProp.FindPropertyRelative("templateToken");
        if (!string.IsNullOrWhiteSpace(templateTokenProp.stringValue))
        {
            return templateTokenProp.stringValue;
        }

        return $"{{{GetParameterRuleKey(parameterDisplayDefinitionProp)}}}";
    }

    // 获取参数规则对应的参数作用。
    private RewardEffectParameterKey GetParameterRuleKey(SerializedProperty parameterDisplayDefinitionProp)
    {
        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        return (RewardEffectParameterKey)parameterKeyProp.enumValueIndex;
    }

    // 获取新增数值时优先使用的未占用参数作用。
    private RewardEffectParameterKey GetFirstUnusedParameterKey()
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        int parameterRulePresetCount = RewardEffectAuthoringPresets.GetDefaultParameterDisplayPresetCount(effectTypeIndex);
        for (int i = 0; i < parameterRulePresetCount; i++)
        {
            RewardEffectParameterKey parameterKey = RewardEffectAuthoringPresets.GetDefaultParameterKey(effectTypeIndex, i);
            if (!IsParameterKeyUsed(parameterKey))
            {
                return parameterKey;
            }
        }

        int parameterKeyCount = Enum.GetValues(typeof(RewardEffectParameterKey)).Length;
        for (int i = 0; i < parameterKeyCount; i++)
        {
            RewardEffectParameterKey parameterKey = (RewardEffectParameterKey)i;
            if (!IsParameterKeyUsed(parameterKey))
            {
                return parameterKey;
            }
        }

        return RewardEffectParameterKey.Value;
    }

    // 判断参数作用是否已经在当前效果中使用。
    private bool IsParameterKeyUsed(RewardEffectParameterKey parameterKey)
    {
        for (int i = 0; i < _parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            if (GetParameterRuleKey(parameterDisplayDefinitionProp) == parameterKey)
            {
                return true;
            }
        }

        return false;
    }

    // 绘制只包含当前玩法常用数值作用的下拉框。
    private void DrawSuggestedParameterKeyPopup(SerializedProperty parameterKeyProp, string label)
    {
        int[] parameterKeyIndexArray = BuildSuggestedParameterKeyIndexArray(parameterKeyProp.enumValueIndex);
        string[] displayNameArray = new string[parameterKeyIndexArray.Length];
        int selectedIndex = 0;

        for (int i = 0; i < parameterKeyIndexArray.Length; i++)
        {
            int parameterKeyIndex = parameterKeyIndexArray[i];
            displayNameArray[i] = RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyIndex);
            if (parameterKeyIndex == parameterKeyProp.enumValueIndex)
            {
                selectedIndex = i;
            }
        }

        int newSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayNameArray);
        parameterKeyProp.enumValueIndex = parameterKeyIndexArray[newSelectedIndex];
    }

    // 构建当前玩法效果建议使用的数值作用列表。
    private int[] BuildSuggestedParameterKeyIndexArray(int currentParameterKeyIndex)
    {
        List<int> parameterKeyIndexList = new List<int>();
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        int parameterRulePresetCount = RewardEffectAuthoringPresets.GetDefaultParameterDisplayPresetCount(effectTypeIndex);

        for (int i = 0; i < parameterRulePresetCount; i++)
        {
            AddUniqueParameterKeyIndex(parameterKeyIndexList, (int)RewardEffectAuthoringPresets.GetDefaultParameterKey(effectTypeIndex, i));
        }

        if (parameterKeyIndexList.Count <= 0)
        {
            AddUniqueParameterKeyIndex(parameterKeyIndexList, (int)RewardEffectParameterKey.Value);
        }

        AddUniqueParameterKeyIndex(parameterKeyIndexList, currentParameterKeyIndex);
        return parameterKeyIndexList.ToArray();
    }

    // 向数值作用索引列表添加不重复的项。
    private void AddUniqueParameterKeyIndex(List<int> parameterKeyIndexList, int parameterKeyIndex)
    {
        if (!parameterKeyIndexList.Contains(parameterKeyIndex))
        {
            parameterKeyIndexList.Add(parameterKeyIndex);
        }
    }

    // 绘制中文枚举下拉框。
    private void DrawEnumPopup(SerializedProperty enumProp, string label, string[] displayNameArray)
    {
        string[] popupNameArray = BuildPopupNameArray(enumProp, displayNameArray);
        enumProp.enumValueIndex = EditorGUILayout.Popup(label, enumProp.enumValueIndex, popupNameArray);
    }

    // 根据中文名数组和原始枚举名构建下拉框显示文本。
    private string[] BuildPopupNameArray(SerializedProperty enumProp, string[] displayNameArray)
    {
        string[] popupNameArray = new string[enumProp.enumDisplayNames.Length];
        for (int i = 0; i < popupNameArray.Length; i++)
        {
            popupNameArray[i] = i < displayNameArray.Length ? displayNameArray[i] : enumProp.enumDisplayNames[i];
        }

        return popupNameArray;
    }

    // 仅在字符串不同时写入序列化字段，减少 Inspector 重绘卡顿。
    private void SetStringIfDifferent(SerializedProperty property, string value)
    {
        if (property.stringValue != value)
        {
            property.stringValue = value;
        }
    }

    // 仅在枚举值不同时写入序列化字段，减少 Inspector 重绘卡顿。
    private void SetEnumIndexIfDifferent(SerializedProperty property, int value)
    {
        if (property.enumValueIndex != value)
        {
            property.enumValueIndex = value;
        }
    }
}
