using UnityEngine;
using UnityEditor;
using MiGame.Lottery;
using MiGame.Pet;
using MiGame.Trail;

namespace MiGame.Lottery.Editor
{
    /// <summary>
    /// 奖励池配置自定义编辑器
    /// </summary>
    [CustomPropertyDrawer(typeof(奖励池配置))]
    public class 奖励池配置Drawer : PropertyDrawer
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
            var 尾迹配置Prop = property.FindPropertyRelative("尾迹配置");
            var 数量Prop = property.FindPropertyRelative("数量");
            var 权重Prop = property.FindPropertyRelative("权重");

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
            if (selectedType != 奖励类型.尾迹)
            {
                尾迹配置Prop.objectReferenceValue = null;
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
                    
                case 奖励类型.尾迹:
                    var 尾迹配置Rect = new Rect(position.x, currentY, position.width, lineHeight);
                    尾迹配置Prop.objectReferenceValue = EditorGUI.ObjectField(尾迹配置Rect, new GUIContent("尾迹配置"), 尾迹配置Prop.objectReferenceValue, typeof(BaseTrailConfig), false);
                    currentY += lineHeight + spacing;
                    break;
            }

            // 数量字段（所有类型都显示）
            var 数量Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(数量Rect, 数量Prop, new GUIContent("数量"));
            currentY += lineHeight + spacing;

            // 权重字段（所有类型都显示）
            var 权重Rect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(权重Rect, 权重Prop, new GUIContent("权重"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var 奖励类型Prop = property.FindPropertyRelative("奖励类型");
            奖励类型 selectedType = (奖励类型)奖励类型Prop.enumValueIndex;
            
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            
            // 基础高度：奖励类型 + 数量 + 权重 + 三个间距
            float height = lineHeight * 3 + spacing * 3;
            
            // 根据奖励类型添加对应配置字段的高度
            switch (selectedType)
            {
                case 奖励类型.物品:
                case 奖励类型.翅膀:
                case 奖励类型.宠物:
                case 奖励类型.伙伴:
                case 奖励类型.尾迹:
                    height += lineHeight + spacing;
                    break;
            }
            
            // 确保最小高度，避免布局问题
            float minHeight = lineHeight * 4 + spacing * 3;
            return Mathf.Max(height, minHeight);
        }
    }
}
