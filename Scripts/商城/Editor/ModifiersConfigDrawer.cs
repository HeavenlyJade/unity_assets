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
            var 条件类型Prop = property.FindPropertyRelative("条件类型");
            var 条件值Prop = property.FindPropertyRelative("条件值");
            var 比较操作符Prop = property.FindPropertyRelative("比较操作符");

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

                // 条件类型
                var 条件类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(条件类型Rect, 条件类型Prop, new GUIContent("📝 条件类型", "选择条件类型：玩家等级、任务完成、道具拥有等"));
                currentY += lineHeight + spacing;

                // 条件值
                var 条件值Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(条件值Rect, 条件值Prop, new GUIContent("💯 条件值", "输入条件的具体数值"));
                currentY += lineHeight + spacing;

                // 比较操作符
                var 比较操作符Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(比较操作符Rect, 比较操作符Prop, new GUIContent("⚖️ 比较操作符", "选择比较方式：等于、大于、小于等"));
                currentY += lineHeight + spacing;

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

            // 展开时的高度：条件类型 + 条件值 + 比较操作符
            float totalHeight = lineHeight; // 折叠标题
            totalHeight += lineHeight + spacing; // 条件类型
            totalHeight += lineHeight + spacing; // 条件值
            totalHeight += lineHeight + spacing; // 比较操作符

            return totalHeight;
        }
    }
}


