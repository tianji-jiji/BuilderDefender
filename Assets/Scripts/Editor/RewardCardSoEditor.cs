using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励卡牌数据的中文简化检视面板，负责隐藏参数嵌套、按效果定义显示数值，并提示常见配置风险。
/// </summary>
[CustomEditor(typeof(RewardCardSo))]
public class RewardCardSoEditor : Editor
{
    private const int BUTTON_WIDTH = 48;

    private static readonly string[] RarityDisplayNameArray = { "普通", "稀有", "史诗", "传说" };
    private static readonly string[] CategoryDisplayNameArray = { "防御塔", "资源", "基地", "风险" };

    private SerializedProperty _cardNameProp;
    private SerializedProperty _cardPrefabProp;
    private SerializedProperty _weightProp;
    private SerializedProperty _rarityProp;
    private SerializedProperty _categoryProp;
    private SerializedProperty _effectConfigListProp;

    // 缓存需要反复使用的序列化字段。
    private void OnEnable()
    {
        _cardNameProp = serializedObject.FindProperty("cardName");
        _cardPrefabProp = serializedObject.FindProperty("cardPrefab");
        _weightProp = serializedObject.FindProperty("weight");
        _rarityProp = serializedObject.FindProperty("rarity");
        _categoryProp = serializedObject.FindProperty("category");
        _effectConfigListProp = serializedObject.FindProperty("effectConfigList");

        if (target is RewardCardSo rewardCard)
        {
            rewardCard.RefreshGeneratedCardIdInEditor();
        }
    }

    // 绘制奖励卡牌的简化检视面板。
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicInfo();
        DrawDrawInfo();
        DrawEffectList();
        DrawCardValidation();

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制卡牌基础显示信息。
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("卡牌基础信息", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_cardNameProp, new GUIContent("卡牌名字"));
        EditorGUILayout.PropertyField(_cardPrefabProp, new GUIContent("卡牌预制体"));
    }

    // 绘制抽卡时会用到的信息。
    private void DrawDrawInfo()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("抽卡规则", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_weightProp, new GUIContent("抽中权重"));
        DrawEnumPopup(_rarityProp, "稀有度", RarityDisplayNameArray);
        DrawEnumPopup(_categoryProp, "卡牌大类", CategoryDisplayNameArray);
    }

    // 绘制卡牌包含的全部效果。
    private void DrawEffectList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("卡牌效果", EditorStyles.boldLabel);

        if (_effectConfigListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("这张卡还没有效果。点击下面按钮添加一个效果。", MessageType.Info);
        }

        for (int i = 0; i < _effectConfigListProp.arraySize; i++)
        {
            SerializedProperty effectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(i);
            DrawEffectConfig(effectConfigProp, i);
        }

        if (GUILayout.Button("添加效果"))
        {
            AddEffectConfig();
        }
    }

    // 绘制单个效果配置。
    private void DrawEffectConfig(SerializedProperty effectConfigProp, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawEffectHeader(index);

        SerializedProperty effectDefinitionProp = effectConfigProp.FindPropertyRelative("effectDefinition");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(effectDefinitionProp, new GUIContent("效果资产"));
        bool effectChanged = EditorGUI.EndChangeCheck();

        RewardEffectDefinitionSo effectDefinition = effectDefinitionProp.objectReferenceValue as RewardEffectDefinitionSo;
        if (!effectDefinition)
        {
            EditorGUILayout.HelpBox("先选择一个效果类型，然后这里会自动显示需要填写的数值。", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("效果名字", effectDefinition.DisplayName);
            if (effectChanged)
            {
                SyncParameterConfigList(effectConfigProp, effectDefinition);
            }

            SyncParameterConfigList(effectConfigProp, effectDefinition);
            DrawParameterValues(effectConfigProp, effectDefinition);
        }

        DrawEffectButtons(index);
        EditorGUILayout.EndVertical();
    }

    // 绘制效果标题。
    private void DrawEffectHeader(int index)
    {
        EditorGUILayout.LabelField($"效果 {index + 1}", EditorStyles.boldLabel);
    }

    // 绘制单个效果的上移、下移和删除按钮。
    private void DrawEffectButtons(int index)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = index > 0;
        if (GUILayout.Button("上移", GUILayout.Width(BUTTON_WIDTH)))
        {
            _effectConfigListProp.MoveArrayElement(index, index - 1);
        }

        GUI.enabled = index < _effectConfigListProp.arraySize - 1;
        if (GUILayout.Button("下移", GUILayout.Width(BUTTON_WIDTH)))
        {
            _effectConfigListProp.MoveArrayElement(index, index + 1);
        }

        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("删除", GUILayout.Width(BUTTON_WIDTH)))
        {
            _effectConfigListProp.DeleteArrayElementAtIndex(index);
        }

        EditorGUILayout.EndHorizontal();
    }

    // 添加一个新的效果配置。
    private void AddEffectConfig()
    {
        int newIndex = _effectConfigListProp.arraySize;
        _effectConfigListProp.InsertArrayElementAtIndex(newIndex);

        SerializedProperty newEffectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(newIndex);
        newEffectConfigProp.FindPropertyRelative("effectDefinition").objectReferenceValue = null;
        newEffectConfigProp.FindPropertyRelative("parameterConfigList").ClearArray();
    }

    // 根据效果定义同步参数列表，避免手动选择错误的参数键。
    private void SyncParameterConfigList(SerializedProperty effectConfigProp, RewardEffectDefinitionSo effectDefinition)
    {
        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        List<ParameterSnapshot> oldParameterSnapshotList = BuildParameterSnapshotList(parameterConfigListProp);
        List<ParameterDisplaySnapshot> parameterDisplaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);

        parameterConfigListProp.arraySize = parameterDisplaySnapshotList.Count;
        for (int i = 0; i < parameterDisplaySnapshotList.Count; i++)
        {
            ParameterDisplaySnapshot displaySnapshot = parameterDisplaySnapshotList[i];
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterConfigProp.FindPropertyRelative("parameterKey");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            SerializedProperty displayImpactOverrideProp = parameterConfigProp.FindPropertyRelative("displayImpactOverride");

            ParameterSnapshot oldSnapshot = FindOldParameterSnapshot(oldParameterSnapshotList, displaySnapshot.parameterKeyIndex);
            parameterKeyProp.enumValueIndex = displaySnapshot.parameterKeyIndex;
            valueProp.floatValue = oldSnapshot.hasValue ? oldSnapshot.value : valueProp.floatValue;
            displayImpactOverrideProp.enumValueIndex = (int)RewardEffectDisplayImpact.Auto;
        }
    }

    // 绘制当前效果需要填写的数值。
    private void DrawParameterValues(SerializedProperty effectConfigProp, RewardEffectDefinitionSo effectDefinition)
    {
        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        if (parameterConfigListProp.arraySize <= 0)
        {
            EditorGUILayout.HelpBox("这个效果不需要填写数值。", MessageType.None);
            return;
        }

        List<ParameterDisplaySnapshot> parameterDisplaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterConfigProp.FindPropertyRelative("parameterKey");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            SerializedProperty displayImpactOverrideProp = parameterConfigProp.FindPropertyRelative("displayImpactOverride");
            ParameterDisplaySnapshot displaySnapshot = FindParameterDisplaySnapshot(parameterDisplaySnapshotList, parameterKeyProp.enumValueIndex);
            string label = string.IsNullOrWhiteSpace(displaySnapshot.displayName)
                ? parameterKeyProp.enumDisplayNames[parameterKeyProp.enumValueIndex]
                : displaySnapshot.displayName;
            string valueLabel = GetValueFieldLabel(effectDefinition, parameterKeyProp, label);

            EditorGUILayout.PropertyField(valueProp, new GUIContent(valueLabel));
            displayImpactOverrideProp.enumValueIndex = (int)RewardEffectDisplayImpact.Auto;
        }
    }

    // 绘制当前卡牌配置的风险提示。
    private void DrawCardValidation()
    {
        List<ValidationMessage> validationMessageList = BuildValidationMessageList();
        if (validationMessageList.Count <= 0)
        {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("配置校验", EditorStyles.boldLabel);
        foreach (ValidationMessage validationMessage in validationMessageList)
        {
            EditorGUILayout.HelpBox(validationMessage.message, validationMessage.messageType);
        }
    }

    // 生成当前卡牌的全部配置校验信息。
    private List<ValidationMessage> BuildValidationMessageList()
    {
        List<ValidationMessage> validationMessageList = new();
        HashSet<RewardEffectType> effectTypeSet = new();
        bool hasPositiveImpact = false;
        bool hasNegativeImpact = false;

        for (int i = 0; i < _effectConfigListProp.arraySize; i++)
        {
            SerializedProperty effectConfigProp = _effectConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty effectDefinitionProp = effectConfigProp.FindPropertyRelative("effectDefinition");
            RewardEffectDefinitionSo effectDefinition = effectDefinitionProp.objectReferenceValue as RewardEffectDefinitionSo;
            if (!effectDefinition)
            {
                validationMessageList.Add(new ValidationMessage($"效果 {i + 1} 还没有选择效果资产。", MessageType.Warning));
                continue;
            }

            if (!effectTypeSet.Add(effectDefinition.EffectType))
            {
                validationMessageList.Add(new ValidationMessage($"重复效果：{effectDefinition.DisplayName}。同一张卡里重复添加同一种唯一效果，后期调参时容易产生歧义。", MessageType.Warning));
            }

            EffectImpactSnapshot impactSnapshot = ValidateEffectParameters(effectConfigProp, effectDefinition, validationMessageList);
            hasPositiveImpact |= impactSnapshot.hasPositiveImpact;
            hasNegativeImpact |= impactSnapshot.hasNegativeImpact;
        }

        bool isRiskCard = _categoryProp.enumValueIndex == (int)RewardCardCategory.Risk;
        if (isRiskCard && _effectConfigListProp.arraySize > 0 && (!hasPositiveImpact || !hasNegativeImpact))
        {
            validationMessageList.Add(new ValidationMessage("风险卡建议同时包含正面和负面效果，避免只是普通增益或普通惩罚。", MessageType.Warning));
        }

        return validationMessageList;
    }

    // 校验单个效果的参数完整性、范围和收益倾向。
    private EffectImpactSnapshot ValidateEffectParameters(SerializedProperty effectConfigProp, RewardEffectDefinitionSo effectDefinition, List<ValidationMessage> validationMessageList)
    {
        SerializedProperty parameterConfigListProp = effectConfigProp.FindPropertyRelative("parameterConfigList");
        List<ParameterDisplaySnapshot> parameterDisplaySnapshotList = BuildParameterDisplaySnapshotList(effectDefinition);
        HashSet<int> parameterKeyIndexSet = new();
        bool hasPositiveImpact = false;
        bool hasNegativeImpact = false;

        foreach (ParameterDisplaySnapshot displaySnapshot in parameterDisplaySnapshotList)
        {
            if (!TryFindParameterValue(parameterConfigListProp, displaySnapshot.parameterKeyIndex, out _))
            {
                string parameterName = GetParameterDisplayName(displaySnapshot);
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 缺少必填参数：{parameterName}。", MessageType.Error));
            }
        }

        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterConfigProp.FindPropertyRelative("parameterKey");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");
            int parameterKeyIndex = parameterKeyProp.enumValueIndex;
            RewardEffectParameterKey parameterKey = (RewardEffectParameterKey)parameterKeyIndex;
            ParameterDisplaySnapshot displaySnapshot = FindParameterDisplaySnapshot(parameterDisplaySnapshotList, parameterKeyIndex);

            if (!parameterKeyIndexSet.Add(parameterKeyIndex))
            {
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 重复配置参数：{RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyIndex)}。", MessageType.Warning));
            }

            if (!displaySnapshot.isValid)
            {
                validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的参数 {RewardEffectAuthoringPresets.GetParameterDisplayName(parameterKeyIndex)} 不在效果定义资产的参数规则里。", MessageType.Warning));
            }

            ValidateParameterValue(effectDefinition, displaySnapshot, parameterKey, valueProp.floatValue, validationMessageList);

            RewardEffectAutoImpactRule autoImpactRule = displaySnapshot.isValid
                ? displaySnapshot.autoImpactRule
                : RewardEffectAuthoringPresets.GetDefaultAutoImpactRule(parameterKey);
            RewardEffectDisplayImpact displayImpact = ResolveAutoDisplayImpact(autoImpactRule, valueProp.floatValue);
            hasPositiveImpact |= displayImpact == RewardEffectDisplayImpact.Positive;
            hasNegativeImpact |= displayImpact == RewardEffectDisplayImpact.Negative;
        }

        return new EffectImpactSnapshot(hasPositiveImpact, hasNegativeImpact);
    }

    // 校验单个参数的数值范围和单位。
    private void ValidateParameterValue(RewardEffectDefinitionSo effectDefinition, ParameterDisplaySnapshot displaySnapshot, RewardEffectParameterKey parameterKey, float value, List<ValidationMessage> validationMessageList)
    {
        RewardEffectValueFormat valueFormat = displaySnapshot.isValid
            ? displaySnapshot.valueFormat
            : RewardEffectAuthoringPresets.GetDefaultValueFormat(parameterKey);
        string parameterName = displaySnapshot.isValid
            ? GetParameterDisplayName(displaySnapshot)
            : RewardEffectAuthoringPresets.GetParameterDisplayName((int)parameterKey);

        if (IsRatioLimitedParameter(parameterKey) && (value < 0f || value > 1f))
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应填写 0~1，小数 0.1 表示 10%。", MessageType.Error));
            return;
        }

        if (IsPercentValueFormat(valueFormat) && !IsRatioLimitedParameter(parameterKey) && Mathf.Abs(value) > 1f)
        {
            int displayPercent = Mathf.RoundToInt(value * 100f);
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 当前会显示为 {displayPercent}%。百分比统一按 0.1=10% 填写。", MessageType.Warning));
        }

        if (valueFormat == RewardEffectValueFormat.PercentWithoutSign && value < 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 使用无符号百分比显示，通常不应填写负数。", MessageType.Warning));
        }

        if (IsPositiveIntegerParameter(parameterKey) && value <= 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应大于 0。", MessageType.Warning));
        }

        if (IsNonNegativeIntegerParameter(parameterKey) && value < 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 不应小于 0。", MessageType.Warning));
        }

        if (IsPositiveNumberParameter(parameterKey) && value <= 0f)
        {
            validationMessageList.Add(new ValidationMessage($"{effectDefinition.DisplayName} 的 {parameterName} 应大于 0。", MessageType.Warning));
        }
    }

    // 解析自动颜色规则对应的收益倾向。
    private RewardEffectDisplayImpact ResolveAutoDisplayImpact(RewardEffectAutoImpactRule autoImpactRule, float value)
    {
        if (Mathf.Approximately(value, 0f))
        {
            return RewardEffectDisplayImpact.Neutral;
        }

        switch (autoImpactRule)
        {
            case RewardEffectAutoImpactRule.GreaterThanZeroIsPositive:
                return value > 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.LessThanZeroIsPositive:
                return value < 0f ? RewardEffectDisplayImpact.Positive : RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.AlwaysPositive:
                return RewardEffectDisplayImpact.Positive;
            case RewardEffectAutoImpactRule.AlwaysNegative:
                return RewardEffectDisplayImpact.Negative;
            case RewardEffectAutoImpactRule.AlwaysNeutral:
                return RewardEffectDisplayImpact.Neutral;
            default:
                return RewardEffectDisplayImpact.Neutral;
        }
    }

    // 判断参数是否必须使用 0~1 的比例值。
    private bool IsRatioLimitedParameter(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.DoubleDamageChance:
            case RewardEffectParameterKey.HomeHealthThreshold:
                return true;
            default:
                return false;
        }
    }

    // 判断显示格式是否为百分比。
    private bool IsPercentValueFormat(RewardEffectValueFormat valueFormat)
    {
        return valueFormat == RewardEffectValueFormat.PercentWithSign || valueFormat == RewardEffectValueFormat.PercentWithoutSign;
    }

    // 判断参数是否应该填写正整数。
    private bool IsPositiveIntegerParameter(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.TriggerAttackCount:
            case RewardEffectParameterKey.ExtraAttackCount:
            case RewardEffectParameterKey.KillCountToUpgrade:
            case RewardEffectParameterKey.AttackHealthCost:
                return true;
            default:
                return false;
        }
    }

    // 判断参数是否应该填写非负整数。
    private bool IsNonNegativeIntegerParameter(RewardEffectParameterKey parameterKey)
    {
        return parameterKey == RewardEffectParameterKey.InitialStarBonus;
    }

    // 判断参数是否应该填写正数。
    private bool IsPositiveNumberParameter(RewardEffectParameterKey parameterKey)
    {
        switch (parameterKey)
        {
            case RewardEffectParameterKey.LinkRadius:
            case RewardEffectParameterKey.ExplosionRadius:
                return true;
            default:
                return false;
        }
    }

    // 尝试从参数列表里读取指定参数值。
    private bool TryFindParameterValue(SerializedProperty parameterConfigListProp, int parameterKeyIndex, out float value)
    {
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterConfigProp.FindPropertyRelative("parameterKey");
            if (parameterKeyProp.enumValueIndex != parameterKeyIndex)
            {
                continue;
            }

            value = parameterConfigProp.FindPropertyRelative("value").floatValue;
            return true;
        }

        value = 0f;
        return false;
    }

    // 获取数值输入框的中文显示名。
    private string GetValueFieldLabel(RewardEffectDefinitionSo effectDefinition, SerializedProperty parameterKeyProp, string fallbackLabel)
    {
        if (parameterKeyProp.enumValueIndex != (int)RewardEffectParameterKey.Value)
        {
            return fallbackLabel;
        }

        if (effectDefinition)
        {
            return GetMainValueLabel(effectDefinition.EffectType);
        }

        if (fallbackLabel == "数值" || fallbackLabel == "主数值")
        {
            return "数值大小";
        }

        return fallbackLabel;
    }

    // 根据效果类型获取主数值在卡牌上的具体含义。
    private string GetMainValueLabel(RewardEffectType effectType)
    {
        switch (effectType)
        {
            case RewardEffectType.DefenseAttackDamageMultiplier:
            case RewardEffectType.DefenseFinalDefenseAttackDamageMultiplier:
                return "攻击力加成";
            case RewardEffectType.DefenseAttackSpeedMultiplier:
                return "攻击速度加成";
            case RewardEffectType.DefenseDetectRadiusMultiplier:
                return "攻击范围加成";
            case RewardEffectType.DefenseMaxHealthMultiplier:
                return "生命值加成";
            case RewardEffectType.DefenseBuildCostMultiplier:
                return "建造成本变化";
            default:
                return "数值大小";
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

    // 收集旧参数值，方便同步参数列表时保留已经填写的数值。
    private List<ParameterSnapshot> BuildParameterSnapshotList(SerializedProperty parameterConfigListProp)
    {
        List<ParameterSnapshot> parameterSnapshotList = new List<ParameterSnapshot>();
        for (int i = 0; i < parameterConfigListProp.arraySize; i++)
        {
            SerializedProperty parameterConfigProp = parameterConfigListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterConfigProp.FindPropertyRelative("parameterKey");
            SerializedProperty valueProp = parameterConfigProp.FindPropertyRelative("value");

            parameterSnapshotList.Add(new ParameterSnapshot(
                parameterKeyProp.enumValueIndex,
                valueProp.floatValue));
        }

        return parameterSnapshotList;
    }

    // 从效果定义资产中读取参数显示配置。
    private List<ParameterDisplaySnapshot> BuildParameterDisplaySnapshotList(RewardEffectDefinitionSo effectDefinition)
    {
        List<ParameterDisplaySnapshot> parameterDisplaySnapshotList = new List<ParameterDisplaySnapshot>();
        if (!effectDefinition)
        {
            return parameterDisplaySnapshotList;
        }

        SerializedObject effectDefinitionObject = new SerializedObject(effectDefinition);
        SerializedProperty parameterDisplayDefinitionListProp = effectDefinitionObject.FindProperty("parameterDisplayDefinitionList");

        for (int i = 0; i < parameterDisplayDefinitionListProp.arraySize; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
            SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
            SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
            SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

            parameterDisplaySnapshotList.Add(new ParameterDisplaySnapshot(
                parameterKeyProp.enumValueIndex,
                displayNameProp.stringValue,
                (RewardEffectValueFormat)valueFormatProp.enumValueIndex,
                (RewardEffectAutoImpactRule)autoImpactRuleProp.enumValueIndex));
        }

        return parameterDisplaySnapshotList;
    }

    // 查找旧参数值快照。
    private ParameterSnapshot FindOldParameterSnapshot(List<ParameterSnapshot> parameterSnapshotList, int parameterKeyIndex)
    {
        foreach (ParameterSnapshot parameterSnapshot in parameterSnapshotList)
        {
            if (parameterSnapshot.parameterKeyIndex == parameterKeyIndex)
            {
                return parameterSnapshot;
            }
        }

        return ParameterSnapshot.Empty;
    }

    // 查找参数显示快照。
    private ParameterDisplaySnapshot FindParameterDisplaySnapshot(List<ParameterDisplaySnapshot> parameterDisplaySnapshotList, int parameterKeyIndex)
    {
        foreach (ParameterDisplaySnapshot parameterDisplaySnapshot in parameterDisplaySnapshotList)
        {
            if (parameterDisplaySnapshot.parameterKeyIndex == parameterKeyIndex)
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
            ? RewardEffectAuthoringPresets.GetParameterDisplayName(displaySnapshot.parameterKeyIndex)
            : displaySnapshot.displayName;
    }

    /// <summary>
    /// 参数旧值快照，用于同步参数列表时保留玩家已经填写的数值。
    /// </summary>
    private readonly struct ParameterSnapshot
    {
        public static readonly ParameterSnapshot Empty = new ParameterSnapshot(-1, 0f, false);

        public readonly int parameterKeyIndex;
        public readonly float value;
        public readonly bool hasValue;

        // 创建参数旧值快照。
        public ParameterSnapshot(int parameterKeyIndex, float value, bool hasValue = true)
        {
            this.parameterKeyIndex = parameterKeyIndex;
            this.value = value;
            this.hasValue = hasValue;
        }
    }

    /// <summary>
    /// 参数显示快照，用于把效果定义中的参数信息转换为简化检视面板标签。
    /// </summary>
    private readonly struct ParameterDisplaySnapshot
    {
        public static readonly ParameterDisplaySnapshot Empty = new ParameterDisplaySnapshot(-1, string.Empty, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral);

        public readonly int parameterKeyIndex;
        public readonly string displayName;
        public readonly RewardEffectValueFormat valueFormat;
        public readonly RewardEffectAutoImpactRule autoImpactRule;
        public readonly bool isValid;

        // 创建参数显示快照。
        public ParameterDisplaySnapshot(int parameterKeyIndex, string displayName, RewardEffectValueFormat valueFormat, RewardEffectAutoImpactRule autoImpactRule)
        {
            this.parameterKeyIndex = parameterKeyIndex;
            this.displayName = displayName;
            this.valueFormat = valueFormat;
            this.autoImpactRule = autoImpactRule;
            isValid = parameterKeyIndex >= 0;
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

    /// <summary>
    /// 效果收益倾向快照，用于判断风险收益卡是否同时包含正负效果。
    /// </summary>
    private readonly struct EffectImpactSnapshot
    {
        public readonly bool hasPositiveImpact;
        public readonly bool hasNegativeImpact;

        // 创建效果收益倾向快照。
        public EffectImpactSnapshot(bool hasPositiveImpact, bool hasNegativeImpact)
        {
            this.hasPositiveImpact = hasPositiveImpact;
            this.hasNegativeImpact = hasNegativeImpact;
        }
    }
}
