using UnityEditor;
using UnityEngine;

/// <summary>
/// 奖励效果定义的中文简化检视面板，负责用更直观的方式配置描述模板和参数显示规则。
/// </summary>
[CustomEditor(typeof(RewardEffectDefinitionSo))]
public class RewardEffectDefinitionSoEditor : Editor
{
    private static readonly string[] EffectTypeDisplayNameArray =
    {
        "防御塔攻击力",
        "防御塔攻击速度",
        "防御塔攻击范围",
        "防御塔最大生命",
        "防御塔建造成本",
        "防御塔护甲穿透",
        "每 N 次攻击额外攻击",
        "防御塔受伤",
        "防御塔波末回血",
        "击杀自动升星",
        "超载攻速变化",
        "防御塔每次攻击损失生命",
        "三星塔数量加攻击",
        "防御塔双倍伤害概率",
        "防御塔靠近提升攻速",
        "随机防御塔升满星",
        "新建塔初始星级",
        "基地低血量加攻击",
        "三星塔爆裂箭"
    };

    private static readonly string[] ParameterKeyDisplayNameArray =
    {
        "主数值",
        "触发攻击次数",
        "额外攻击数量",
        "护甲穿透百分比",
        "受到伤害变化",
        "波末回血百分比",
        "升星所需击杀数",
        "攻击速度变化",
        "攻击损失生命",
        "每座三星塔攻击加成",
        "双倍伤害概率",
        "联动距离",
        "初始星级加成",
        "基地生命阈值",
        "爆炸半径",
        "爆炸伤害倍率"
    };

    private static readonly string[] ValueFormatDisplayNameArray =
    {
        "百分比有符号，比如 +10%",
        "百分比无符号，比如 10%",
        "整数有符号，比如 +1",
        "整数无符号，比如 5",
        "普通数字，比如 2.5"
    };

    private static readonly string[] AutoImpactRuleDisplayNameArray =
    {
        "绿色",
        "红色",
        "大于 0 显示绿色，小于 0 显示红色",
        "大于 0 显示红色，小于 0 显示绿色",
        "白色",
    };

    private static readonly ParameterRulePreset[][] EffectParameterPresetArray =
    {
        new[] { new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成") },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击速度加成") },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击范围加成") },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "生命值加成") },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive, "建造成本变化") },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.ArmorIgnorePercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[]
        {
            new ParameterRulePreset(RewardEffectParameterKey.TriggerAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
            new ParameterRulePreset(RewardEffectParameterKey.ExtraAttackCount, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive)
        },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.DamageTakenMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.LessThanZeroIsPositive) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.WaveEndHealPercent, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.KillCountToUpgrade, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.AttackHealthCost, RewardEffectValueFormat.IntegerWithoutSign, RewardEffectAutoImpactRule.AlwaysNegative) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.DamageBonusPerThreeStarTower, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.DoubleDamageChance, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive) },
        new[]
        {
            new ParameterRulePreset(RewardEffectParameterKey.LinkRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
            new ParameterRulePreset(RewardEffectParameterKey.AttackSpeedMultiplier, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive)
        },
        new ParameterRulePreset[] { },
        new[] { new ParameterRulePreset(RewardEffectParameterKey.InitialStarBonus, RewardEffectValueFormat.IntegerWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive) },
        new[]
        {
            new ParameterRulePreset(RewardEffectParameterKey.HomeHealthThreshold, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysNeutral),
            new ParameterRulePreset(RewardEffectParameterKey.Value, RewardEffectValueFormat.PercentWithSign, RewardEffectAutoImpactRule.GreaterThanZeroIsPositive, "攻击力加成")
        },
        new[]
        {
            new ParameterRulePreset(RewardEffectParameterKey.ExplosionRadius, RewardEffectValueFormat.NumberOnly, RewardEffectAutoImpactRule.AlwaysNeutral),
            new ParameterRulePreset(RewardEffectParameterKey.ExplosionDamageMultiplier, RewardEffectValueFormat.PercentWithoutSign, RewardEffectAutoImpactRule.AlwaysPositive)
        }
    };

    private static readonly string[] DescriptionTemplateArray =
    {
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {value}",
        "{displayName} {ArmorIgnorePercent}",
        "每攻击 {TriggerAttackCount} 次，额外攻击 {ExtraAttackCount} 次",
        "{displayName} {DamageTakenMultiplier}",
        "每波结束恢复 {WaveEndHealPercent} 最大生命",
        "击杀 {KillCountToUpgrade} 个敌人后自动升星",
        "{displayName} {AttackSpeedMultiplier}",
        "每次攻击损失 {AttackHealthCost} 生命",
        "每座三星塔使攻击力 {DamageBonusPerThreeStarTower}",
        "攻击时有 {DoubleDamageChance} 概率造成双倍伤害",
        "{LinkRadius} 范围内有防御塔时，攻击速度 {AttackSpeedMultiplier}",
        "随机一座防御塔升到满星",
        "新建防御塔初始星级 {InitialStarBonus}",
        "基地生命低于 {HomeHealthThreshold} 时，防御塔攻击力 {value}",
        "三星塔攻击造成 {ExplosionRadius} 范围爆炸，伤害倍率 {ExplosionDamageMultiplier}"
    };

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
        SyncParameterDisplayRuleList(false);
        DrawDescriptionSettings();
        DrawParameterDisplayRuleList();

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制效果基础信息。
    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("效果", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        DrawEnumPopup(_effectTypeProp, "玩法效果", EffectTypeDisplayNameArray);
        if (EditorGUI.EndChangeCheck())
        {
            ApplyEffectTypeDefaults();
        }

        ApplyEmptyDefaults();
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
            SetStringIfDifferent(_descriptionTemplateProp, GetDescriptionTemplate(_effectTypeProp.enumValueIndex));
        }

        if (_useCustomDescriptionProp.boolValue)
        {
            DrawCustomDescriptionEditor();
            return;
        }

        SetStringIfDifferent(_descriptionTemplateProp, GetDescriptionTemplate(_effectTypeProp.enumValueIndex));
        EditorGUILayout.LabelField("自动描述", ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue));
    }

    // 绘制自定义描述文本框和可插入数值按钮。
    private void DrawCustomDescriptionEditor()
    {
        string displayDescription = ConvertTemplateToDisplayText(_descriptionTemplateProp.stringValue);

        EditorGUI.BeginChangeCheck();
        string newDisplayDescription = EditorGUILayout.TextArea(displayDescription, GUILayout.MinHeight(48));
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
            EditorGUILayout.HelpBox("不要手写大括号参数，用下面按钮插入数值。", MessageType.Warning);
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
            DrawParameterDisplayRule(parameterDisplayDefinitionProp, i);
        }

    }

    // 绘制单条参数显示规则。
    private void DrawParameterDisplayRule(SerializedProperty parameterDisplayDefinitionProp, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"数值 {index + 1}", EditorStyles.boldLabel);

        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        SerializedProperty displayNameProp = parameterDisplayDefinitionProp.FindPropertyRelative("displayName");
        SerializedProperty valueFormatProp = parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat");
        SerializedProperty autoImpactRuleProp = parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule");

        if (string.IsNullOrWhiteSpace(displayNameProp.stringValue))
        {
            SetStringIfDifferent(displayNameProp, GetParameterDisplayName(parameterKeyProp.enumValueIndex));
        }

        EditorGUILayout.LabelField("数值含义", displayNameProp.stringValue);

        DrawEnumPopup(valueFormatProp, "显示方式", ValueFormatDisplayNameArray);
        DrawEnumPopup(autoImpactRuleProp, "颜色", AutoImpactRuleDisplayNameArray);

        EditorGUILayout.EndVertical();
    }

    // 玩法效果变化时自动填充名字和描述模板。
    private void ApplyEffectTypeDefaults()
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        SetStringIfDifferent(_displayNameProp, GetEffectTypeDisplayName(effectTypeIndex));
        SetBoolIfDifferent(_useCustomDescriptionProp, false);
        SetStringIfDifferent(_descriptionTemplateProp, GetDescriptionTemplate(effectTypeIndex));
        SyncParameterDisplayRuleList(true);
    }

    // 空字段自动补齐，避免手写程序占位符。
    private void ApplyEmptyDefaults()
    {
        int effectTypeIndex = _effectTypeProp.enumValueIndex;
        if (string.IsNullOrWhiteSpace(_displayNameProp.stringValue))
        {
            SetStringIfDifferent(_displayNameProp, GetEffectTypeDisplayName(effectTypeIndex));
        }

        if (string.IsNullOrWhiteSpace(_descriptionTemplateProp.stringValue))
        {
            SetStringIfDifferent(_descriptionTemplateProp, GetDescriptionTemplate(effectTypeIndex));
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

    // 根据玩法效果索引获取中文效果名。
    private string GetEffectTypeDisplayName(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < EffectTypeDisplayNameArray.Length)
        {
            return EffectTypeDisplayNameArray[effectTypeIndex];
        }

        return "奖励效果";
    }

    // 根据玩法效果索引获取自动描述模板。
    private string GetDescriptionTemplate(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < DescriptionTemplateArray.Length)
        {
            return DescriptionTemplateArray[effectTypeIndex];
        }

        return "{displayName} {value}";
    }

    // 根据参数枚举索引获取中文显示名。
    private string GetParameterDisplayName(int parameterKeyIndex)
    {
        if (parameterKeyIndex >= 0 && parameterKeyIndex < ParameterKeyDisplayNameArray.Length)
        {
            return ParameterKeyDisplayNameArray[parameterKeyIndex];
        }

        return "数值";
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

        return GetParameterDisplayName((int)GetParameterRuleKey(parameterDisplayDefinitionProp));
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

    // 获取参数规则对应的参数类型。
    private RewardEffectParameterKey GetParameterRuleKey(SerializedProperty parameterDisplayDefinitionProp)
    {
        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        return (RewardEffectParameterKey)parameterKeyProp.enumValueIndex;
    }

    // 根据玩法效果同步需要显示的数值定义。
    private void SyncParameterDisplayRuleList(bool shouldResetDisplaySettings)
    {
        ParameterRulePreset[] parameterRulePresetArray = GetParameterRulePresets(_effectTypeProp.enumValueIndex);
        bool structureChanged = _parameterDisplayDefinitionListProp.arraySize != parameterRulePresetArray.Length;
        if (structureChanged)
        {
            _parameterDisplayDefinitionListProp.arraySize = parameterRulePresetArray.Length;
        }

        for (int i = 0; i < parameterRulePresetArray.Length; i++)
        {
            SerializedProperty parameterDisplayDefinitionProp = _parameterDisplayDefinitionListProp.GetArrayElementAtIndex(i);
            ApplyParameterRulePreset(parameterDisplayDefinitionProp, parameterRulePresetArray[i], shouldResetDisplaySettings || structureChanged);
        }
    }

    // 把预设参数规则写入序列化字段。
    private void ApplyParameterRulePreset(SerializedProperty parameterDisplayDefinitionProp, ParameterRulePreset parameterRulePreset, bool shouldResetDisplaySettings)
    {
        int parameterKeyIndex = (int)parameterRulePreset.ParameterKey;
        SerializedProperty parameterKeyProp = parameterDisplayDefinitionProp.FindPropertyRelative("parameterKey");
        bool parameterKeyChanged = parameterKeyProp.enumValueIndex != parameterKeyIndex;

        SetEnumIndexIfDifferent(parameterKeyProp, parameterKeyIndex);
        SetStringIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("displayName"), parameterRulePreset.GetDisplayName(GetParameterDisplayName(parameterKeyIndex)));
        SetStringIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("templateToken"), string.Empty);

        if (shouldResetDisplaySettings || parameterKeyChanged)
        {
            SetEnumIndexIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("valueFormat"), (int)parameterRulePreset.ValueFormat);
            SetEnumIndexIfDifferent(parameterDisplayDefinitionProp.FindPropertyRelative("autoImpactRule"), (int)parameterRulePreset.AutoImpactRule);
        }
    }

    // 根据玩法效果获取默认数值定义。
    private ParameterRulePreset[] GetParameterRulePresets(int effectTypeIndex)
    {
        if (effectTypeIndex >= 0 && effectTypeIndex < EffectParameterPresetArray.Length)
        {
            return EffectParameterPresetArray[effectTypeIndex];
        }

        return EffectParameterPresetArray[0];
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

    // 仅在字符串不同的时候写入序列化字段，减少 Inspector 重绘卡顿。
    private void SetStringIfDifferent(SerializedProperty property, string value)
    {
        if (property.stringValue != value)
        {
            property.stringValue = value;
        }
    }

    // 仅在布尔值不同的时候写入序列化字段，减少 Inspector 重绘卡顿。
    private void SetBoolIfDifferent(SerializedProperty property, bool value)
    {
        if (property.boolValue != value)
        {
            property.boolValue = value;
        }
    }

    // 仅在枚举值不同的时候写入序列化字段，减少 Inspector 重绘卡顿。
    private void SetEnumIndexIfDifferent(SerializedProperty property, int value)
    {
        if (property.enumValueIndex != value)
        {
            property.enumValueIndex = value;
        }
    }

    /// <summary>
    /// 奖励效果参数显示预设，负责描述某个玩法效果默认需要哪些数值。
    /// </summary>
    private readonly struct ParameterRulePreset
    {
        public readonly RewardEffectParameterKey ParameterKey;
        public readonly RewardEffectValueFormat ValueFormat;
        public readonly RewardEffectAutoImpactRule AutoImpactRule;
        public readonly string DisplayNameOverride;

        // 创建参数显示预设。
        public ParameterRulePreset(RewardEffectParameterKey parameterKey, RewardEffectValueFormat valueFormat, RewardEffectAutoImpactRule autoImpactRule, string displayNameOverride = "")
        {
            ParameterKey = parameterKey;
            ValueFormat = valueFormat;
            AutoImpactRule = autoImpactRule;
            DisplayNameOverride = displayNameOverride;
        }

        // 获取参数在面板里显示的中文名。
        public string GetDisplayName(string fallbackDisplayName)
        {
            return string.IsNullOrWhiteSpace(DisplayNameOverride) ? fallbackDisplayName : DisplayNameOverride;
        }
    }
}
