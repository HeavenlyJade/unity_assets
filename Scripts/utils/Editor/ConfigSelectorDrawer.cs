using UnityEditor;
using UnityEngine;
using System;
using MiGame.Data;
using MiGame.Achievement;
using MiGame.Pet;
using MiGame.Skills;

[CustomPropertyDrawer(typeof(ConfigSelectorAttribute))]
public class ConfigSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConfigSelectorAttribute configSelector = (ConfigSelectorAttribute)attribute;
        SerializedProperty enumProp = property.serializedObject.FindProperty(configSelector.EnumFieldName);

        if (enumProp == null)
        {
            EditorGUI.LabelField(position, label.text, "错误: 找不到枚举字段 '" + configSelector.EnumFieldName + "'");
            return;
        }
        
        配置类型 configType = (配置类型)enumProp.enumValueIndex;
        Type objectType = GetTypeForConfig(configType);

        // 如果当前字段引用的对象类型与期望类型不匹配，则清空
        if (property.objectReferenceValue != null && !objectType.IsAssignableFrom(property.objectReferenceValue.GetType()))
        {
            property.objectReferenceValue = null;
        }

        // 绘制一个特定类型的对象字段
        property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, objectType, false);
    }

    private Type GetTypeForConfig(配置类型 type)
    {
        switch (type)
        {
            case 配置类型.天赋: return typeof(AchievementConfig);
            case 配置类型.宠物: return typeof(PetConfig);
            case 配置类型.伙伴: return typeof(PartnerConfig);
            case 配置类型.技能: return typeof(Skill);
            default: return typeof(ScriptableObject);
        }
    }
} 