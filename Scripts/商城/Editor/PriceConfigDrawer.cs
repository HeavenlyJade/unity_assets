using UnityEngine;
using UnityEditor;
using MiGame.Shop;

namespace MiGame.Shop.Editor
{
    /// <summary>
    /// 价格配置自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(PriceConfig))]
    public class PriceConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 货币类型Prop = property.FindPropertyRelative("货币类型");
            var 价格数量Prop = property.FindPropertyRelative("价格数量");
            var 迷你币类型Prop = property.FindPropertyRelative("迷你币类型");
            var 迷你币数量Prop = property.FindPropertyRelative("迷你币数量");
            var 变量键Prop = property.FindPropertyRelative("变量键");
            var 广告模式Prop = property.FindPropertyRelative("广告模式");
            var 广告次数Prop = property.FindPropertyRelative("广告次数");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 绘制折叠标题
            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // 货币类型
                var 货币类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(货币类型Rect, 货币类型Prop, new GUIContent("货币类型"));
                currentY += lineHeight + spacing;

                // 价格数量 - 直接使用字符串字段
                var 价格数量Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(价格数量Rect, 价格数量Prop, new GUIContent("价格数量"));
                currentY += lineHeight + spacing;

                // 迷你币类型
                var 迷你币类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(迷你币类型Rect, 迷你币类型Prop, new GUIContent("迷你币类型"));
                currentY += lineHeight + spacing;

                // 迷你币数量
                var 迷你币数量Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(迷你币数量Rect, 迷你币数量Prop, new GUIContent("迷你币数量"));
                currentY += lineHeight + spacing;

                // 变量键
                var 变量键Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(变量键Rect, 变量键Prop, new GUIContent("变量键"));
                currentY += lineHeight + spacing;

                // 广告模式
                var 广告模式Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(广告模式Rect, 广告模式Prop, new GUIContent("广告模式"));
                currentY += lineHeight + spacing;

                // 广告次数
                var 广告次数Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(广告次数Rect, 广告次数Prop, new GUIContent("广告次数"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
            {
                return lineHeight; // 只显示折叠标题
            }

            // 展开时的高度：货币类型 + 价格数量 + 迷你币类型 + 迷你币数量 + 变量键 + 广告模式 + 广告次数
            int fieldCount = 7;

            return lineHeight * fieldCount + spacing * (fieldCount - 1);
        }


    }
}
