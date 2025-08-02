using UnityEngine;
using UnityEditor;
using MiGame.Pet;

namespace MiGame.Utils.Editor
{
    /// <summary>
    /// 升星材料的自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(升星材料))]
    public class 升星材料Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 绘制主标签
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 消耗类型Prop = property.FindPropertyRelative("消耗类型");
            var 材料物品Prop = property.FindPropertyRelative("材料物品");
            var 消耗名称Prop = property.FindPropertyRelative("消耗名称");
            var 需要数量Prop = property.FindPropertyRelative("需要数量");
            var 消耗星级Prop = property.FindPropertyRelative("消耗星级");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            // 消耗类型
            var 消耗类型Rect = new Rect(position.x, position.y, position.width, lineHeight);
            EditorGUI.PropertyField(消耗类型Rect, 消耗类型Prop);

            // 根据消耗类型显示不同字段
            var 消耗类型 = (消耗类型)消耗类型Prop.enumValueIndex;
            
            if (消耗类型 == 消耗类型.物品)
            {
                // 显示物品相关字段
                var 材料物品Rect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
                EditorGUI.PropertyField(材料物品Rect, 材料物品Prop);
                
                var 需要数量Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
                EditorGUI.PropertyField(需要数量Rect, 需要数量Prop);
            }
            else if (消耗类型 == 消耗类型.宠物)
            {
                // 显示宠物相关字段
                var 消耗名称Rect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
                EditorGUI.PropertyField(消耗名称Rect, 消耗名称Prop);
                
                var 消耗星级Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
                EditorGUI.PropertyField(消耗星级Rect, 消耗星级Prop);
                
                var 需要数量Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 3, position.width, lineHeight);
                EditorGUI.PropertyField(需要数量Rect, 需要数量Prop);
            }
            else if (消耗类型 == 消耗类型.伙伴)
            {
                // 显示伙伴相关字段
                var 消耗名称Rect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
                EditorGUI.PropertyField(消耗名称Rect, 消耗名称Prop);
                
                var 消耗星级Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
                EditorGUI.PropertyField(消耗星级Rect, 消耗星级Prop);

                var 需要数量Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 3, position.width, lineHeight);
                EditorGUI.PropertyField(需要数量Rect, 需要数量Prop);
            }
            else if (消耗类型 == 消耗类型.翅膀)
            {
                // 显示翅膀相关字段（与伙伴逻辑相同）
                var 消耗名称Rect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
                EditorGUI.PropertyField(消耗名称Rect, 消耗名称Prop);
                
                var 消耗星级Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);
                EditorGUI.PropertyField(消耗星级Rect, 消耗星级Prop);

                var 需要数量Rect = new Rect(position.x, position.y + (lineHeight + spacing) * 3, position.width, lineHeight);
                EditorGUI.PropertyField(需要数量Rect, 需要数量Prop);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var 消耗类型Prop = property.FindPropertyRelative("消耗类型");
            var 消耗类型 = (消耗类型)消耗类型Prop.enumValueIndex;
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 基础高度：标签 + 消耗类型
            float height = lineHeight * 2 + spacing;
            
            // 根据消耗类型添加额外高度
            if (消耗类型 == 消耗类型.物品)
            {
                // 材料物品 + 需要数量
                height += lineHeight * 2 + spacing;
            }
            else if (消耗类型 == 消耗类型.宠物 || 消耗类型 == 消耗类型.伙伴 || 消耗类型 == 消耗类型.翅膀)
            {
                // 消耗名称 + 消耗星级 + 需要数量
                height += lineHeight * 3 + spacing * 2;
            }
            
            return height;
        }
    }
} 