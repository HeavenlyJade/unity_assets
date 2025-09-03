using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MiGame.Reward;
using MiGame.Pet;

namespace MiGame.Reward.Editor
{
    /// <summary>
    /// 新奖励物品自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(新奖励物品))]
    public class 新奖励物品Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局 - 不使用PrefixLabel，直接使用完整位置
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 奖励类型Prop = property.FindPropertyRelative("奖励类型");
            var 物品Prop = property.FindPropertyRelative("物品");
            var 翅膀配置Prop = property.FindPropertyRelative("翅膀配置");
            var 宠物配置Prop = property.FindPropertyRelative("宠物配置");
            var 伙伴配置Prop = property.FindPropertyRelative("伙伴配置");
            var 数量Prop = property.FindPropertyRelative("数量");
            var 星级Prop = property.FindPropertyRelative("星级");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 奖励类型
            var 奖励类型Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(奖励类型Rect, 奖励类型Prop, new GUIContent("奖励类型"));
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
                    EditorGUI.PropertyField(物品Rect, 物品Prop, new GUIContent("物品"));
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
            EditorGUI.PropertyField(数量Rect, 数量Prop, new GUIContent("数量"));
            currentY += lineHeight + spacing;

            // 星级字段（所有类型都显示）
            var 星级Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(星级Rect, 星级Prop, new GUIContent("星级"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var 奖励类型Prop = property.FindPropertyRelative("奖励类型");
            奖励类型 selectedType = (奖励类型)奖励类型Prop.enumValueIndex;
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 基础高度：奖励类型 + 数量 + 星级 + 三个间距
            float height = lineHeight * 3 + spacing * 3;
            
            // 根据奖励类型添加对应配置字段的高度
            switch (selectedType)
            {
                case 奖励类型.物品:
                case 奖励类型.翅膀:
                case 奖励类型.宠物:
                case 奖励类型.伙伴:
                    height += lineHeight + spacing;
                    break;
            }
            
            // 确保最小高度，避免布局问题
            float minHeight = lineHeight * 4 + spacing * 3;
            return Mathf.Max(height, minHeight);
        }
    }

    /// <summary>
    /// 奖励列表项自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(奖励列表项))]
    public class 奖励列表项Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 计算布局
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 获取子属性
            var 描述Prop = property.FindPropertyRelative("描述");
            var 条件公式Prop = property.FindPropertyRelative("条件公式");
            var 消耗迷你币Prop = property.FindPropertyRelative("消耗迷你币");
            var 权重Prop = property.FindPropertyRelative("权重");
            var 奖励物品列表Prop = property.FindPropertyRelative("奖励物品列表");

            // 计算字段高度
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 描述 - 使用实际高度
            float 描述Height = EditorGUI.GetPropertyHeight(描述Prop, true);
            var 描述Rect = new Rect(position.x, currentY, position.width, 描述Height);
            EditorGUI.PropertyField(描述Rect, 描述Prop, new GUIContent("描述"), true);
            currentY += 描述Height + spacing;

            // 条件公式 - 使用实际高度，确保TextArea有足够空间
            float 条件公式Height = EditorGUI.GetPropertyHeight(条件公式Prop, true);
            var 条件公式Rect = new Rect(position.x, currentY, position.width, 条件公式Height);
            EditorGUI.PropertyField(条件公式Rect, 条件公式Prop, new GUIContent("条件公式"), true);
            currentY += 条件公式Height + spacing;

            // 消耗迷你币
            var 消耗迷你币Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(消耗迷你币Rect, 消耗迷你币Prop, new GUIContent("消耗迷你币"));
            currentY += lineHeight + spacing;

            // 权重
            var 权重Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(权重Rect, 权重Prop, new GUIContent("权重"));
            currentY += lineHeight + spacing;

            // 奖励物品列表 - 使用实际高度
            float 奖励物品列表Height = EditorGUI.GetPropertyHeight(奖励物品列表Prop, true);
            var 奖励物品列表Rect = new Rect(position.x, currentY, position.width, 奖励物品列表Height);
            EditorGUI.PropertyField(奖励物品列表Rect, 奖励物品列表Prop, new GUIContent("奖励物品列表"), true);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 获取描述/条件公式属性并计算其实际高度
            var 描述Prop = property.FindPropertyRelative("描述");
            float 描述Height = EditorGUI.GetPropertyHeight(描述Prop, true);
            var 条件公式Prop = property.FindPropertyRelative("条件公式");
            float 条件公式Height = EditorGUI.GetPropertyHeight(条件公式Prop, true);
            
            // 获取奖励物品列表属性并计算其实际高度
            var 奖励物品列表Prop = property.FindPropertyRelative("奖励物品列表");
            float 奖励物品列表Height = EditorGUI.GetPropertyHeight(奖励物品列表Prop, true);
            
            // 确保TextArea有足够的最小高度
            if (描述Height < lineHeight * 2)
            {
                描述Height = lineHeight * 2;
            }
            if (条件公式Height < lineHeight * 2)
            {
                条件公式Height = lineHeight * 2;
            }
            
            // 总高度 = 描述 + 条件公式 + 消耗迷你币 + 权重 + 奖励物品列表 + 间距
            return 描述Height + spacing + 条件公式Height + spacing + lineHeight + spacing + lineHeight + spacing + 奖励物品列表Height;
        }
    }

    /// <summary>
    /// 新奖励配置自定义编辑器
    /// </summary>
    [CustomEditor(typeof(NewRewardConfig))]
    public class NewRewardConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (NewRewardConfig)target;
            
            // 绘制默认Inspector
            DrawDefaultInspector();
            
            // 添加一些实用按钮
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);
            
            if (GUILayout.Button("添加奖励项"))
            {
                if (config.奖励列表 == null)
                {
                    config.奖励列表 = new List<奖励列表项>();
                }
                var newItem = new 奖励列表项();
                newItem.奖励物品列表 = new List<新奖励物品>();
                config.奖励列表.Add(newItem);
                EditorUtility.SetDirty(config);
            }
            
            if (GUILayout.Button("清空奖励列表"))
            {
                if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有奖励项吗？", "确定", "取消"))
                {
                    config.奖励列表.Clear();
                    EditorUtility.SetDirty(config);
                }
            }
            
            // 显示奖励列表统计信息
            if (config.奖励列表 != null && config.奖励列表.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"奖励列表统计: {config.奖励列表.Count} 项", EditorStyles.helpBox);
            }
        }
    }
}
