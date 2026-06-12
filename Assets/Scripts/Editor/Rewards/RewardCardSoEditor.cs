using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励卡牌的自定义 Inspector，负责同步效果参数并提供基础配置校验。
/// </summary>
[CustomEditor(typeof(RewardCardSo))]
public class RewardCardSoEditor : Editor
{
    private SerializedProperty _cardNameProp;
    private SerializedProperty _cardPrefabProp;
    private SerializedProperty _weightProp;
    private SerializedProperty _rarityProp;
    private SerializedProperty _categoryProp;
    private SerializedProperty _minWaveIndexProp;
    private SerializedProperty _maxPickCountProp;
    private SerializedProperty _isUniqueProp;
    private SerializedProperty _effectConfigListProp;

    // 缓存序列化字段引用。
    private void OnEnable()
    {
        _cardNameProp = serializedObject.FindProperty("cardName");
        _cardPrefabProp = serializedObject.FindProperty("cardPrefab");
        _weightProp = serializedObject.FindProperty("weight");
        _rarityProp = serializedObject.FindProperty("rarity");
        _categoryProp = serializedObject.FindProperty("category");
        _minWaveIndexProp = serializedObject.FindProperty("minWaveIndex");
        _maxPickCountProp = serializedObject.FindProperty("maxPickCount");
        _isUniqueProp = serializedObject.FindProperty("isUnique");
        _effectConfigListProp = serializedObject.FindProperty("effectConfigList");
    }

    // 绘制奖励卡牌 Inspector。
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicInfo();
        EditorGUILayout.Space(8f);
        DrawEffectConfigs();
        EditorGUILayout.Space(8f);
        DrawValidationMessages();

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制卡牌基础信息。
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_cardNameProp, new GUIContent("卡牌名称"));
        EditorGUILayout.PropertyField(_cardPrefabProp, new GUIContent("卡牌预制体"));
        EditorGUILayout.PropertyField(_weightProp, new GUIContent("权重"));
        EditorGUILayout.PropertyField(_rarityProp, new GUIContent("稀有度"));
        EditorGUILayout.PropertyField(_categoryProp, new GUIContent("分类"));
        EditorGUILayout.PropertyField(_minWaveIndexProp, new GUIContent("最小波次"));
        EditorGUILayout.PropertyField(_isUniqueProp, new GUIContent("唯一卡牌"));

        if (!_isUniqueProp.boolValue)
        {
            EditorGUILayout.PropertyField(_maxPickCountProp, new GUIContent("最大选择次数"));
        }
    }

    // 绘制效果配置列表。
    private void DrawEffectConfigs()
    {
        EditorGUILayout.LabelField("奖励效果", EditorStyles.boldLabel);

        if (GUILayout.Button("新增效果"))
        {
            AddEffectConfig();
        }

        if (_effectConfigListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("当前卡牌还没有配置奖励效果。", MessageType.Info);
            return;
        }

        for (int i = 0; i < _effectConfigListProp.arraySize; i++)
        {
            SerializedProperty effectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(i);
            DrawEffectConfig(effectConfigProp, i);
        }
    }

    // 绘制单个效果配置。
    private void DrawEffectConfig(SerializedProperty effectConfigProp, int index)
    {
        SerializedProperty effectDefinitionProp = effectConfigProp.FindPropertyRelative("effectDefinition");
        RewardEffectDefinitionSo effectDefinition = effectDefinitionProp.objectReferenceValue as RewardEffectDefinitionSo;
        string title = effectDefinition ? effectDefinition.DisplayName : "未绑定效果";

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{index + 1}. {title}", EditorStyles.boldLabel);
        if (GUILayout.Button("删除", GUILayout.Width(56f)))
        {
            _effectConfigListProp.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(effectDefinitionProp, new GUIContent("效果定义"));
        if (EditorGUI.EndChangeCheck())
        {
            effectDefinition = effectDefinitionProp.objectReferenceValue as RewardEffectDefinitionSo;
            SyncParameterConfigList(effectConfigProp, effectDefinition);
        }

        if (effectDefinition)
        {
            if (GUILayout.Button("按效果定义同步参数"))
            {
                SyncParameterConfigList(effectConfigProp, effectDefinition);
            }

            DrawParameterValues(effectConfigProp, effectDefinition);
        }

        EditorGUILayout.EndVertical();
    }

    // 添加一个新的效果配置。
    private void AddEffectConfig()
    {
        int newIndex = _effectConfigListProp.arraySize;
        _effectConfigListProp.InsertArrayElementAtIndex(newIndex);

        SerializedProperty effectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(newIndex);
        effectConfigProp.FindPropertyRelative("effectDefinition").objectReferenceValue = null;
        effectConfigProp.FindPropertyRelative("parameterConfigList").ClearArray();
    }

    // 根据效果定义同步参数列表。
    private void SyncParameterConfigList(SerializedProperty effectConfigProp, RewardEffectDefinitionSo effectDefinition)
    {
        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        List<ParameterValueSnapshot> oldValueList = BuildParameterValueSnapshotList(parameterConfigListProp);
        List<ParameterDisplaySnapshot> displaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);

        parameterConfigListProp.arraySize = displaySnapshotList.Count;
        for (int i = 0; i < displaySnapshotList.Count; i++)
        {
            ParameterDisplaySnapshot displaySnapshot = displaySnapshotList[i];
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterConfigProp.FindPropertyRelative("parameterId");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            SerializedProperty displayImpactOverrideProp = parameterConfigProp.FindPropertyRelative("displayImpactOverride");

            ParameterValueSnapshot oldValue = FindOldParameterValue(oldValueList, displaySnapshot.parameterId);
            parameterIdProp.stringValue = displaySnapshot.parameterId;
            valueProp.floatValue = oldValue.hasValue ? oldValue.value : valueProp.floatValue;
            displayImpactOverrideProp.enumValueIndex = (int)RewardEffectDisplayImpact.Auto;
        }
    }

    // 绘制当前效果需要填写的参数值。
    private void DrawParameterValues(SerializedProperty effectConfigProp, RewardEffectDefinitionSo effectDefinition)
    {
        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        if (parameterConfigListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("这个效果没有参数。", MessageType.None);
            return;
        }

        List<ParameterDisplaySnapshot> displaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterConfigProp.FindPropertyRelative("parameterId");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            SerializedProperty displayImpactOverrideProp = parameterConfigProp.FindPropertyRelative("displayImpactOverride");

            string parameterId = GetSerializedParameterId(parameterIdProp);
            ParameterDisplaySnapshot displaySnapshot = FindParameterDisplaySnapshot(displaySnapshotList, parameterId);
            string label = string.IsNullOrWhiteSpace(displaySnapshot.displayName) ? parameterId : displaySnapshot.displayName;
            DrawParameterValueField(valueProp, displaySnapshot, parameterId, label);
            displayImpactOverrideProp.enumValueIndex = (int)RewardEffectDisplayImpact.Auto;
        }
    }

    // 根据参数显示格式绘制整数或小数输入框。
    private void DrawParameterValueField(SerializedProperty valueProp, ParameterDisplaySnapshot displaySnapshot, string parameterId, string label)
    {
        RewardEffectValueFormat valueFormat = displaySnapshot.isValid
            ? displaySnapshot.valueFormat
            : RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterId);

        if (!IsIntegerValueFormat(valueFormat))
        {
            EditorGUILayout.PropertyField(valueProp, new GUIContent(label));
            return;
        }

        int intValue = Mathf.RoundToInt(valueProp.floatValue);
        if (!Mathf.Approximately(valueProp.floatValue, intValue))
        {
            valueProp.floatValue = intValue;
        }

        EditorGUI.BeginChangeCheck();
        int newIntValue = EditorGUILayout.IntField(new GUIContent(label), intValue);
        if (EditorGUI.EndChangeCheck())
        {
            valueProp.floatValue = newIntValue;
        }
    }

    // 绘制卡牌配置校验信息。
    private void DrawValidationMessages()
    {
        foreach (ValidationMessage validationMessage in BuildValidationMessageList())
        {
            EditorGUILayout.HelpBox(validationMessage.message, validationMessage.messageType);
        }
    }

    // 生成卡牌配置校验信息。
    private List<ValidationMessage> BuildValidationMessageList()
    {
        List<ValidationMessage> validationMessageList = new();

        if (_weightProp.intValue < 0)
        {
            validationMessageList.Add(new ValidationMessage("权重不应小于 0。", MessageType.Warning));
        }

        if (_minWaveIndexProp.intValue < 0)
        {
            validationMessageList.Add(new ValidationMessage("最小波次不应小于 0。", MessageType.Warning));
        }

        for (int i = 0; i < _effectConfigListProp.arraySize; i++)
        {
            SerializedProperty effectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(i);
            ValidateEffectConfig(effectConfigProp, i, validationMessageList);
        }

        return validationMessageList;
    }

    // 校验单个效果配置。
    private void ValidateEffectConfig(SerializedProperty effectConfigProp, int index, List<ValidationMessage> validationMessageList)
    {
        SerializedProperty effectDefinitionProp = effectConfigProp.FindPropertyRelative("effectDefinition");
        RewardEffectDefinitionSo effectDefinition = effectDefinitionProp.objectReferenceValue as RewardEffectDefinitionSo;
        if (!effectDefinition)
        {
            validationMessageList.Add(new ValidationMessage($"第 {index + 1} 个效果没有绑定效果定义。", MessageType.Error));
            return;
        }

        if (!effectDefinition.Handler)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 没有绑定 Handler，不会产生玩法效果。", MessageType.Warning));
        }

        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        List<ParameterDisplaySnapshot> displaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);
        HashSet<string> parameterIdSet = new();

        foreach (ParameterDisplaySnapshot displaySnapshot in displaySnapshotList)
        {
            if (!TryFindParameterValue(parameterConfigListProp, displaySnapshot.parameterId, out _))
            {
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 缺少参数：{GetParameterDisplayName(displaySnapshot)}。", MessageType.Error));
            }
        }

        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterConfigProp.FindPropertyRelative("parameterId");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            string parameterId = GetSerializedParameterId(parameterIdProp);
            ParameterDisplaySnapshot displaySnapshot = FindParameterDisplaySnapshot(displaySnapshotList, parameterId);

            if (!parameterIdSet.Add(parameterId))
            {
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 重复配置参数：{parameterId}。", MessageType.Warning));
            }

            if (!displaySnapshot.isValid)
            {
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的参数 {parameterId} 不在效果定义资产的参数规则里。", MessageType.Warning));
            }

            ValidateParameterValue(effectDefinition, displaySnapshot, parameterId, valueProp.floatValue, validationMessageList);
        }
    }

    // 校验单个参数的数值范围和单位。
    private void ValidateParameterValue(RewardEffectDefinitionSo effectDefinition, ParameterDisplaySnapshot displaySnapshot, string parameterId, float value, List<ValidationMessage> validationMessageList)
    {
        RewardEffectValueFormat valueFormat = displaySnapshot.isValid
            ? displaySnapshot.valueFormat
            : RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterId);
        string parameterName = displaySnapshot.isValid ? GetParameterDisplayName(displaySnapshot) : parameterId;

        if (RewardEffectAuthoringPresets.IsRatioLimitedParameter(parameterId) && (value < 0f || value > 1f))
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应填写 0~1，小数 0.1 表示 10%。", MessageType.Error));
            return;
        }

        bool isRatioLimited = RewardEffectAuthoringPresets.IsRatioLimitedParameter(parameterId);
        if (IsPercentValueFormat(valueFormat) && !isRatioLimited && Mathf.Abs(value) > 1f)
        {
            int displayPercent = Mathf.RoundToInt(value * 100f);
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 当前会显示为 {displayPercent}%。百分比统一按 0.1=10% 填写。", MessageType.Warning));
        }

        if (valueFormat == RewardEffectValueFormat.PercentWithoutSign && value < 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 使用无符号百分比显示，通常不应填写负数。", MessageType.Warning));
        }

        if (IsIntegerValueFormat(valueFormat) && !Mathf.Approximately(value, Mathf.RoundToInt(value)))
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应填写整数，当前小数会在面板中自动取整。", MessageType.Warning));
        }

        if (RewardEffectAuthoringPresets.IsPositiveIntegerParameter(parameterId) && value <= 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应大于 0。", MessageType.Warning));
        }

        if (RewardEffectAuthoringPresets.IsNonNegativeIntegerParameter(parameterId) && value < 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 不应小于 0。", MessageType.Warning));
        }

        if (RewardEffectAuthoringPresets.IsPositiveNumberParameter(parameterId) && value <= 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应大于 0。", MessageType.Warning));
        }
    }

    // 判断显示格式是否为百分比。
    private bool IsPercentValueFormat(RewardEffectValueFormat valueFormat)
    {
        return valueFormat == RewardEffectValueFormat.PercentWithSign || valueFormat == RewardEffectValueFormat.PercentWithoutSign;
    }

    // 判断显示格式是否为整数。
    private bool IsIntegerValueFormat(RewardEffectValueFormat valueFormat)
    {
        return valueFormat == RewardEffectValueFormat.IntegerWithSign || valueFormat == RewardEffectValueFormat.IntegerWithoutSign;
    }

    // 尝试从参数列表中查找参数值。
    private bool TryFindParameterValue(SerializedProperty parameterConfigListProp, string parameterId, out float value)
    {
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterConfigProp.FindPropertyRelative("parameterId");
            if (GetSerializedParameterId(parameterIdProp) != parameterId)
            {
                continue;
            }

            value = parameterConfigProp.FindPropertyRelative("value").floatValue;
            return true;
        }

        value = 0f;
        return false;
    }

    // 收集旧参数值，方便同步参数列表时保留已经填写的数值。
    private List<ParameterValueSnapshot> BuildParameterValueSnapshotList(SerializedProperty parameterConfigListProp)
    {
        List<ParameterValueSnapshot> parameterValueList = new();
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterConfigProp.FindPropertyRelative("parameterId");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            parameterValueList.Add(new ParameterValueSnapshot(GetSerializedParameterId(parameterIdProp), valueProp.floatValue));
        }

        return parameterValueList;
    }

    // 从效果定义资产中读取参数显示配置。
    private List<ParameterDisplaySnapshot> BuildParameterDisplaySnapshotList(RewardEffectDefinitionSo effectDefinition)
    {
        List<ParameterDisplaySnapshot> parameterDisplaySnapshotList = new();
        if (!effectDefinition)
        {
            return parameterDisplaySnapshotList;
        }

        SerializedObject effectDefinitionObject = new SerializedObject(effectDefinition);
        SerializedProperty parameterDisplayDefinitionListProp = effectDefinitionObject.FindProperty("parameterDisplayDefinitionList");
        for (int i = 0; i < parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterIdProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterId");
            SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
            SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
            SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

            string parameterId = GetSerializedParameterId(parameterIdProp);
            parameterDisplaySnapshotList.Add(new ParameterDisplaySnapshot(
                parameterId,
                displayNameProp.stringValue,
                (RewardEffectValueFormat)valueFormatProp.enumValueIndex,
                (RewardEffectAutoImpactRule)autoImpactRuleProp.enumValueIndex));
        }

        return parameterDisplaySnapshotList;
    }

    // 查找旧参数值快照。
    private ParameterValueSnapshot FindOldParameterValue(List<ParameterValueSnapshot> parameterValueList, string parameterId)
    {
        foreach (ParameterValueSnapshot parameterValue in parameterValueList)
        {
            if (parameterValue.parameterId == parameterId)
            {
                return parameterValue;
            }
        }

        return ParameterValueSnapshot.Empty;
    }

    // 查找参数显示快照。
    private ParameterDisplaySnapshot FindParameterDisplaySnapshot(List<ParameterDisplaySnapshot> parameterDisplaySnapshotList, string parameterId)
    {
        foreach (ParameterDisplaySnapshot parameterDisplaySnapshot in parameterDisplaySnapshotList)
        {
            if (parameterDisplaySnapshot.parameterId == parameterId)
            {
                return parameterDisplaySnapshot;
            }
        }

        return ParameterDisplaySnapshot.Empty;
    }

    // 获取参数显示快照的中文名。
    private string GetParameterDisplayName(ParameterDisplaySnapshot displaySnapshot)
    {
        return string.IsNullOrWhiteSpace(displaySnapshot.displayName)
            ? displaySnapshot.parameterId
            : displaySnapshot.displayName;
    }

    // 获取序列化参数 ID。
    private string GetSerializedParameterId(SerializedProperty parameterIdProp)
    {
        return string.IsNullOrWhiteSpace(parameterIdProp.stringValue)
            ? RewardEffectParameterIds.VALUE
            : parameterIdProp.stringValue.Trim();
    }

    /// <summary>
    /// 参数旧值快照，用于同步参数列表时保留玩家已经填写的数值。
    /// </summary>
    private readonly struct ParameterValueSnapshot
    {
        public static readonly ParameterValueSnapshot Empty = new(string.Empty, 0f, false);

        public readonly string parameterId;
        public readonly float value;
        public readonly bool hasValue;

        // 创建参数旧值快照。
        public ParameterValueSnapshot(string parameterId, float value, bool hasValue = true)
        {
            this.parameterId = parameterId;
            this.value = value;
            this.hasValue = hasValue;
        }
    }

    /// <summary>
    /// 参数显示快照，用于把效果定义中的参数信息转换为简化检查面板标签。
    /// </summary>
    private readonly struct ParameterDisplaySnapshot
    {
        public static readonly ParameterDisplaySnapshot Empty = new(string.Empty, string.Empty, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral);

        public readonly string parameterId;
        public readonly string displayName;
        public readonly RewardEffectValueFormat valueFormat;
        public readonly RewardEffectAutoImpactRule autoImpactRule;
        public readonly bool isValid;

        // 创建参数显示快照。
        public ParameterDisplaySnapshot(string parameterId, string displayName, RewardEffectValueFormat valueFormat, RewardEffectAutoImpactRule autoImpactRule)
        {
            this.parameterId = parameterId;
            this.displayName = displayName;
            this.valueFormat = valueFormat;
            this.autoImpactRule = autoImpactRule;
            isValid = !string.IsNullOrWhiteSpace(parameterId);
        }
    }

    /// <summary>
    /// 校验提示快照，用于延迟绘制 HelpBox。
    /// </summary>
    private readonly struct ValidationMessage
    {
        public readonly string message;
        public readonly MessageType messageType;

        // 创建校验提示快照。
        public ValidationMessage(string message, MessageType messageType)
        {
            this.message = message;
            this.messageType = messageType;
        }
    }
}
