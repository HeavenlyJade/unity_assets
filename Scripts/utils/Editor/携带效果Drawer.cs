using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using System;

namespace MiGame.Utils.Editor
{
    [CustomPropertyDrawer(typeof(携带效果))]
    public class 携带效果Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            var foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var contentRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

                var 变量类型Prop = property.FindPropertyRelative("变量类型");
                var 变量名称Prop = property.FindPropertyRelative("变量名称");
                var 效果数值Prop = property.FindPropertyRelative("效果数值");
                var 加成类型Prop = property.FindPropertyRelative("加成类型");
                
                EditorGUI.PropertyField(contentRect, 变量类型Prop);
                contentRect.y += lineHeight + spacing;
                EditorGUI.PropertyField(contentRect, 变量名称Prop);
                contentRect.y += lineHeight + spacing;
                EditorGUI.PropertyField(contentRect, 效果数值Prop);
                contentRect.y += lineHeight + spacing;
                EditorGUI.PropertyField(contentRect, 加成类型Prop);
                contentRect.y += lineHeight + spacing;

                加成类型 bonusType = (加成类型)加成类型Prop.enumValueIndex;

                if (bonusType == 加成类型.物品)
                {
                    var 物品目标Prop = property.FindPropertyRelative("物品目标");
                    EditorGUI.PropertyField(contentRect, 物品目标Prop, new GUIContent("加成目标"));
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var 加成类型Prop = property.FindPropertyRelative("加成类型");
            加成类型 bonusType = (加成类型)加成类型Prop.enumValueIndex;

            float totalLines = 5; // Foldout + 4 basic fields
            if (bonusType == 加成类型.物品)
            {
                totalLines++; // Add line for the item target
            }

            return (EditorGUIUtility.singleLineHeight * totalLines) + (EditorGUIUtility.standardVerticalSpacing * (totalLines -1));
        }
    }
} 