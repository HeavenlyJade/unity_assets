#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condHAtt = (ConditionalFieldAttribute)attribute;
        bool enabled = GetConditionalFieldAttributeResult(condHAtt, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute condHAtt = (ConditionalFieldAttribute)attribute;
        bool enabled = GetConditionalFieldAttributeResult(condHAtt, property);

        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalFieldAttributeResult(ConditionalFieldAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = true;
        
        // 获取目标属性路径
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, condHAtt.fieldName);
        
        // 查找条件属性
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            // 根据属性类型进行比较
            switch (sourcePropertyValue.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    enabled = sourcePropertyValue.boolValue.Equals(condHAtt.value);
                    break;
                case SerializedPropertyType.Enum:
                    enabled = sourcePropertyValue.enumValueIndex.Equals((int)condHAtt.value);
                    break;
                case SerializedPropertyType.Integer:
                    enabled = sourcePropertyValue.intValue.Equals(condHAtt.value);
                    break;
                case SerializedPropertyType.Float:
                    enabled = Mathf.Approximately(sourcePropertyValue.floatValue, (float)condHAtt.value);
                    break;
                case SerializedPropertyType.String:
                    enabled = sourcePropertyValue.stringValue.Equals(condHAtt.value);
                    break;
            }
        }

        return enabled;
    }
}
#endif