using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励卡牌数据的中文简化检视面板，负责隐藏参数嵌套并按效果定义显示可填写数值。
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
            valueProp.floatValue = oldSnapshot.HasValue ? oldSnapshot.Value : valueProp.floatValue;
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

    // 收集旧参数值，方便同步参数列表时保留已有数值。
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

            parameterDisplaySnapshotList.Add(new ParameterDisplaySnapshot(
                parameterKeyProp.enumValueIndex,
                displayNameProp.stringValue));
        }

        return parameterDisplaySnapshotList;
    }

    // 查找旧参数快照。
    private ParameterSnapshot FindOldParameterSnapshot(List<ParameterSnapshot> parameterSnapshotList, int parameterKeyIndex)
    {
        foreach (ParameterSnapshot parameterSnapshot in parameterSnapshotList)
        {
            if (parameterSnapshot.ParameterKeyIndex == parameterKeyIndex)
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

    /// <summary>
    /// 参数旧值快照，用于同步参数列表时保留玩家已经填写的数值。
    /// </summary>
    private readonly struct ParameterSnapshot
    {
        public static readonly ParameterSnapshot Empty = new ParameterSnapshot(-1, 0f, false);

        public readonly int ParameterKeyIndex;
        public readonly float Value;
        public readonly bool HasValue;

        // 创建参数旧值快照。
        public ParameterSnapshot(int parameterKeyIndex, float value, bool hasValue = true)
        {
            ParameterKeyIndex = parameterKeyIndex;
            Value = value;
            HasValue = hasValue;
        }
    }

    /// <summary>
    /// 参数显示快照，用于把效果定义中的参数信息转换为简化检视面板标签。
    /// </summary>
    private readonly struct ParameterDisplaySnapshot
    {
        public static readonly ParameterDisplaySnapshot Empty = new ParameterDisplaySnapshot(-1, string.Empty);

        public readonly int parameterKeyIndex;
        public readonly string displayName;

        // 创建参数显示快照。
        public ParameterDisplaySnapshot(int parameterKeyIndex, string displayName)
        {
            this.parameterKeyIndex = parameterKeyIndex;
            this.displayName = displayName;
        }
    }
}
