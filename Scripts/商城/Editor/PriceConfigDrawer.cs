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
            var 效果配置器Prop = property.FindPropertyRelative("效果配置器");
            var 效果配置变量Prop = property.FindPropertyRelative("效果配置变量");
            var 变量类型Prop = property.FindPropertyRelative("变量类型");
            var 玩家变量Prop = property.FindPropertyRelative("玩家变量");
            var 迷你币类型Prop = property.FindPropertyRelative("迷你币类型");
            var 迷你币数量Prop = property.FindPropertyRelative("迷你币数量");
            var 变量键Prop = property.FindPropertyRelative("变量键");
            var 广告模式Prop = property.FindPropertyRelative("广告模式");
            var 广告次数Prop = property.FindPropertyRelative("广告次数");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing * 4.0f; // 进一步增加间距
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

                // 效果配置器 - 美化显示
                var 效果配置器Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(效果配置器Rect, 效果配置器Prop, new GUIContent("⚙️ 效果配置器", "选择EffectLevelConfig资源，用于动态计算价格"));
                currentY += lineHeight + spacing;

                // 效果配置变量
                var 效果配置变量Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(效果配置变量Rect, 效果配置变量Prop, new GUIContent("🎯 效果配置变量", "输入效果配置变量名称"));
                currentY += lineHeight + spacing;

                // 变量类型
                var 变量类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(变量类型Rect, 变量类型Prop, new GUIContent("📋 变量类型", "选择变量类型：玩家变量或玩家属性"));
                currentY += lineHeight + spacing;

                // 玩家变量 - 优化显示格式
                var 玩家变量Rect = new Rect(position.x, currentY, position.width, lineHeight);
                
                // 添加帮助框提示（仅在字段为空时显示）
                if (string.IsNullOrEmpty(玩家变量Prop.stringValue))
                {
                    var helpBoxRect = new Rect(position.x, currentY, position.width, lineHeight * 1.5f);
                    // EditorGUI.HelpBox(helpBoxRect, "💡 玩家变量名称，用于获取对应等级效果配置来动态设定金币价格", MessageType.Info);
                    currentY += lineHeight * 1.5f + spacing;
                }
                
                // 绘制玩家变量字段，使用更清晰的标签和样式
                var originalColor = GUI.backgroundColor;
                if (string.IsNullOrEmpty(玩家变量Prop.stringValue))
                {
                    GUI.backgroundColor = new Color(1f, 1f, 0.8f, 1f); // 淡黄色背景提示
                }
                
                // EditorGUI.PropertyField(玩家变量Rect, 玩家变量Prop, new GUIContent("🎯 玩家变量", "输入玩家变量名称，配合效果配置器使用"));
                GUI.backgroundColor = originalColor;
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
            float spacing = EditorGUIUtility.standardVerticalSpacing * 4.0f; // 与OnGUI中的间距保持一致

            if (!property.isExpanded)
            {
                return lineHeight; // 只显示折叠标题
            }

            // 展开时的高度：货币类型 + 价格数量 + 效果配置器 + 效果配置变量 + 变量类型 + 玩家变量(可能包含帮助框) + 迷你币类型 + 迷你币数量 + 变量键 + 广告模式 + 广告次数
            int fieldCount = 10;
            
            // 检查是否需要显示玩家变量帮助框
            var 玩家变量Prop = property.FindPropertyRelative("玩家变量");
            if (string.IsNullOrEmpty(玩家变量Prop.stringValue))
            {
                fieldCount += 1; // 帮助框额外占用1.5行高度
            }

            return lineHeight * fieldCount + spacing * (fieldCount - 1);
        }


    }
}
