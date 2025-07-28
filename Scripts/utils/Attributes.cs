using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ServerAttrAttribute : Attribute {

}

[AttributeUsage(AttributeTargets.Class)]
public class KeyedArrayAttribute : Attribute {
}
[AttributeUsage(AttributeTargets.Field)]
public class DictKeyAttribute : Attribute {
}
[AttributeUsage(AttributeTargets.Field)]
public class ServerNameAttrAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Field)]
public class DontExportAttribute : Attribute {
}
[AttributeUsage(AttributeTargets.Field)]
public class MultiplyByAttribute : Attribute {
    public string fieldName;
    public MultiplyByAttribute(string fieldName) {
        this.fieldName = fieldName;
    }
}

public class ConditionalFieldAttribute : PropertyAttribute {
    public string ConditionFieldName { get; private set; }
    public string[] ConditionFieldNames { get; private set; }
    public object[] TargetValue { get; private set; }
    public bool IsMultiCondition { get; private set; }

    public ConditionalFieldAttribute(string conditionFieldName, params object[] targetValue) {
        ConditionFieldName = conditionFieldName;
        TargetValue = targetValue;
        IsMultiCondition = false;
    }
    
    public ConditionalFieldAttribute(string[] conditionFieldNames, params object[] targetValue) {
        ConditionFieldNames = conditionFieldNames;
        TargetValue = targetValue;
        IsMultiCondition = true;
    }
}
public class ConditionalField2Attribute : ConditionalFieldAttribute {

    public ConditionalField2Attribute(string conditionFieldName, params object[] targetValue) : base(conditionFieldName, targetValue) {
    }
}

/// <summary>
/// 根据枚举字段的值，动态改变对象字段类型的特性。
/// </summary>
public class ConfigSelectorAttribute : PropertyAttribute
{
    public string EnumFieldName { get; }
    public ConfigSelectorAttribute(string enumFieldName)
    {
        EnumFieldName = enumFieldName;
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
[CustomPropertyDrawer(typeof(ConditionalField2Attribute))]
public class ConditionalFieldDrawer : PropertyDrawer
{

    private bool ShouldDisplay(SerializedProperty property, ConditionalFieldAttribute conditionalField)
    {
        if (conditionalField.IsMultiCondition)
        {
            // 处理多条件 "AND" 逻辑
            for (int i = 0; i < conditionalField.ConditionFieldNames.Length; i++)
            {
                var conditionProperty = GetConditionProperty(property, conditionalField.ConditionFieldNames[i]);
                if (conditionProperty == null || !CheckCondition(conditionProperty, new[] { conditionalField.TargetValue[i] }))
                {
                    return false; // 任何一个条件不满足，则不显示
                }
            }
            return true; // 所有条件都满足
        }
        else
        {
            // 处理单条件逻辑
            var conditionProperty = GetConditionProperty(property, conditionalField.ConditionFieldName);
            if (conditionProperty == null) return false;
            return CheckCondition(conditionProperty, conditionalField.TargetValue);
        }
    }

    private bool CheckCondition(SerializedProperty conditionProperty, object[] targetValues)
    {
        switch (conditionProperty.propertyType)
        {
            case SerializedPropertyType.Integer:
                if (targetValues.Length > 1 && targetValues[0] is string op)
                {
                    var val = Convert.ToInt32(targetValues[1]);
                    if (op == ">") return conditionProperty.intValue > val;
                    if (op == "<") return conditionProperty.intValue < val;
                    if (op == "!=") return conditionProperty.intValue != val;
                }
                return targetValues.Any(v => conditionProperty.intValue.Equals(Convert.ChangeType(v, typeof(int))));
            case SerializedPropertyType.Float:
                 if (targetValues.Length > 1 && targetValues[0] is string opf)
                {
                    var valf = Convert.ToSingle(targetValues[1]);
                    if (opf == ">") return conditionProperty.floatValue > valf;
                    if (opf == "<") return conditionProperty.floatValue < valf;
                    if (opf == "!=") return conditionProperty.floatValue != valf;
                }
                return targetValues.Any(v => conditionProperty.floatValue.Equals(Convert.ChangeType(v, typeof(float))));
            case SerializedPropertyType.Enum:
                return targetValues.Any(v => conditionProperty.enumValueIndex == Convert.ToInt32(v));
            case SerializedPropertyType.Boolean:
                return targetValues.Length > 0 && conditionProperty.boolValue == (bool)targetValues[0];
            default:
                Debug.LogWarning($"ConditionalField: Unsupported field type '{conditionProperty.propertyType}'");
                return false;
        }
    }

    private SerializedProperty GetConditionProperty(SerializedProperty property, string conditionFieldName)
    {
        string propertyPath = property.propertyPath;
        
        // 如果路径中没有点号，说明是根级属性
        if (!propertyPath.Contains("."))
        {
            return property.serializedObject.FindProperty(conditionFieldName);
        }
        
        // 获取最后一个点之前的所有内容（父对象的路径）
        string parentPath = propertyPath.Substring(0, propertyPath.LastIndexOf('.'));
        
        // 构建完整的条件属性路径
        string fullPath = $"{parentPath}.{conditionFieldName}";
        
        // 查找条件属性
        return property.serializedObject.FindProperty(fullPath);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalField = attribute as ConditionalFieldAttribute;

        // 检查条件是否满足
        if (ShouldDisplay(property, conditionalField))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalField = attribute as ConditionalFieldAttribute;
        
        if (ShouldDisplay(property, conditionalField))
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif