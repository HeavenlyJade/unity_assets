using UnityEngine;
using UnityEditor;
using MiGame.Reward;
using MiGame.Pet;

namespace MiGame.Reward.Editor
{
    /// <summary>
    /// 奖励节点自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(奖励节点))]
    public class 奖励节点Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 时间节点Prop = property.FindPropertyRelative("时间节点");
            var 奖励物品Prop = property.FindPropertyRelative("奖励物品");
            var 临取执行指令Prop = property.FindPropertyRelative("临取执行指令");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 时间节点
            var 时间节点Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(时间节点Rect, 时间节点Prop);
            currentY += lineHeight + spacing;

            // 奖励物品
            var 奖励物品Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(奖励物品Rect, 奖励物品Prop);
            currentY += lineHeight + spacing;

            // 临取执行指令
            var 临取执行指令Rect = new Rect(position.x, currentY, position.width, lineHeight * 3);
            EditorGUI.PropertyField(临取执行指令Rect, 临取执行指令Prop);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 基础高度：标签 + 时间节点 + 奖励物品 + 临取执行指令
            float height = lineHeight * 5 + spacing * 3; // 包括临取执行指令的高度
            
            return height;
        }
    }

    /// <summary>
    /// 奖励物品自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(奖励物品))]
    public class 奖励物品Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 奖励类型Prop = property.FindPropertyRelative("奖励类型");
            var 物品Prop = property.FindPropertyRelative("物品");
            var 翅膀配置Prop = property.FindPropertyRelative("翅膀配置");
            var 宠物配置Prop = property.FindPropertyRelative("宠物配置");
            var 伙伴配置Prop = property.FindPropertyRelative("伙伴配置");
            var 数量Prop = property.FindPropertyRelative("数量");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 奖励类型
            var 奖励类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(奖励类型Rect, 奖励类型Prop);
            currentY += lineHeight + spacing;

            // 根据奖励类型显示对应字段并清空其他字段
            奖励类型 selectedType = (奖励类型)奖励类型Prop.enumValueIndex;
            
            // 清空不相关的字段
            if (selectedType != 奖励类型.物品)
            {
                物品Prop.objectReferenceValue = null;
            }
            if (selectedType != 奖励类型.翅膀)
            {
                翅膀配置Prop.objectReferenceValue = null;
            }
            if (selectedType != 奖励类型.宠物)
            {
                宠物配置Prop.objectReferenceValue = null;
            }
            if (selectedType != 奖励类型.伙伴)
            {
                伙伴配置Prop.objectReferenceValue = null;
            }
            
            switch (selectedType)
            {
                case 奖励类型.物品:
                    var 物品Rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(物品Rect, 物品Prop);
                    currentY += lineHeight + spacing;
                    break;
                    
                case 奖励类型.翅膀:
                    var 翅膀配置Rect = new Rect(position.x, currentY, position.width, lineHeight);
                    翅膀配置Prop.objectReferenceValue = EditorGUI.ObjectField(翅膀配置Rect, new GUIContent("翅膀配置"), 翅膀配置Prop.objectReferenceValue, typeof(WingConfig), false);
                    currentY += lineHeight + spacing;
                    break;
                    
                case 奖励类型.宠物:
                    var 宠物配置Rect = new Rect(position.x, currentY, position.width, lineHeight);
                    宠物配置Prop.objectReferenceValue = EditorGUI.ObjectField(宠物配置Rect, new GUIContent("宠物配置"), 宠物配置Prop.objectReferenceValue, typeof(PetConfig), false);
                    currentY += lineHeight + spacing;
                    break;
                    
                case 奖励类型.伙伴:
                    var 伙伴配置Rect = new Rect(position.x, currentY, position.width, lineHeight);
                    伙伴配置Prop.objectReferenceValue = EditorGUI.ObjectField(伙伴配置Rect, new GUIContent("伙伴配置"), 伙伴配置Prop.objectReferenceValue, typeof(PartnerConfig), false);
                    currentY += lineHeight + spacing;
                    break;
            }

            // 数量字段（所有类型都显示）
            var 数量Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(数量Rect, 数量Prop);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var 奖励类型Prop = property.FindPropertyRelative("奖励类型");
            奖励类型 selectedType = (奖励类型)奖励类型Prop.enumValueIndex;
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 基础高度：标签 + 奖励类型 + 数量
            float height = lineHeight * 3 + spacing * 2;
            
            // 根据奖励类型添加额外字段高度
            switch (selectedType)
            {
                case 奖励类型.物品:
                case 奖励类型.翅膀:
                case 奖励类型.宠物:
                case 奖励类型.伙伴:
                    height += lineHeight + spacing;
                    break;
            }
            
            return height;
        }
    }
} 