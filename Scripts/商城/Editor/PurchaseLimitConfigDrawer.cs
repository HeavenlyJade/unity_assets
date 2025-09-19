using UnityEngine;
using UnityEditor;
using MiGame.Shop;

namespace MiGame.Shop.Editor
{
    /// <summary>
    /// 购买限制配置自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(PurchaseLimitConfig))]
    public class PurchaseLimitConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 限购类型Prop = property.FindPropertyRelative("限购类型");
            var 限购次数Prop = property.FindPropertyRelative("限购次数");
            var 重置时间Prop = property.FindPropertyRelative("重置时间");
            var 购买条件Prop = property.FindPropertyRelative("购买条件");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing * 8.0f; // 增加间距，让字段之间有更多空间
            float topMargin = EditorGUIUtility.standardVerticalSpacing * 4.0f; // 顶部额外间距，避免与价格配置重叠
            float currentY = position.y + topMargin;

            // 绘制折叠标题
            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // 限购类型
                var 限购类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(限购类型Rect, 限购类型Prop, new GUIContent(" 限购类型", "选择限购类型：无限制、每日、每周、每月或永久一次"));
                currentY += lineHeight + spacing;

                // 限购次数
                var 限购次数Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(限购次数Rect, 限购次数Prop, new GUIContent(" 限购次数", "限购数量，0为无限制"));
                currentY += lineHeight + spacing;

                // 重置时间
                var 重置时间Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(重置时间Rect, 重置时间Prop, new GUIContent(" 重置时间", "重置时间点，如04:00"));
                currentY += lineHeight + spacing;

                // 购买条件 - 使用实际高度
                float 购买条件Height = EditorGUI.GetPropertyHeight(购买条件Prop, true);
                var 购买条件Rect = new Rect(position.x, currentY, position.width, 购买条件Height);
                EditorGUI.PropertyField(购买条件Rect, 购买条件Prop, new GUIContent(" 购买前置条件", "设置购买的前置条件"), true);
                currentY += 购买条件Height + spacing;

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing * 8.0f; // 与OnGUI中的间距保持一致
            float topMargin = EditorGUIUtility.standardVerticalSpacing * 4.0f; // 顶部额外间距，避免与价格配置重叠

            if (!property.isExpanded)
            {
                return lineHeight + topMargin; // 折叠标题 + 顶部间距
            }

            // 展开时的高度：顶部间距 + 折叠标题 + 限购类型 + 限购次数 + 重置时间 + 购买条件
            float totalHeight = topMargin; // 顶部间距
            totalHeight += lineHeight; // 折叠标题
            
            // 添加字段高度
            totalHeight += lineHeight + spacing; // 限购类型
            totalHeight += lineHeight + spacing; // 限购次数
            totalHeight += lineHeight + spacing; // 重置时间
            
            // 购买条件使用实际高度
            var 购买条件Prop = property.FindPropertyRelative("购买条件");
            float 购买条件Height = EditorGUI.GetPropertyHeight(购买条件Prop, true);
            totalHeight += 购买条件Height + spacing;

            return totalHeight;
        }
    }
}


