using UnityEngine;
using UnityEditor;
using MiGame.Shop;

namespace MiGame.Shop.Editor
{
    /// <summary>
    /// 购买条件配置自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(ModifiersConfig))]
    public class ModifiersConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 条件描述Prop = property.FindPropertyRelative("条件描述");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing * 2.5f; // 增加间距
            float currentY = position.y;

            // 绘制折叠标题
            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // 条件描述
                var 条件描述Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(条件描述Rect, 条件描述Prop, new GUIContent("📝 条件描述", "输入购买前置条件的描述"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing * 2.5f; // 与OnGUI中的间距保持一致

            if (!property.isExpanded)
            {
                return lineHeight; // 只显示折叠标题
            }

            // 展开时的高度：折叠标题 + 条件描述
            float totalHeight = lineHeight; // 折叠标题
            totalHeight += lineHeight + spacing; // 条件描述

            return totalHeight;
        }
    }
}


