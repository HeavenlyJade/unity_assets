using UnityEditor;
using UnityEngine;
using MiGame.Core;
using System.Linq;
using MiGame;

namespace MiGame.EditorExtras
{
    [CustomPropertyDrawer(typeof(PlayerBonus))]
    public class PlayerBonusDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var nameProp = property.FindPropertyRelative("Name");
            var calculationProp = property.FindPropertyRelative("Calculation");
            var scaleProp = property.FindPropertyRelative("缩放倍率");
            var effectFieldProp = property.FindPropertyRelative("玩家效果字段");

            // 计算行高和间距
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y;

            // 设置紧凑的标签宽度
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80f;

            // 1. 名称字段
            var nameRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            var bonusTypeAttributes = fieldInfo.GetCustomAttributes(typeof(BonusTypeAttribute), true);
            if (bonusTypeAttributes.Any())
            {
                var bonusTypeAttr = bonusTypeAttributes[0] as BonusTypeAttribute;
                VariableSelectorDrawer.DrawSelector(nameRect, nameProp, new GUIContent("名称"), bonusTypeAttr.NameType);
            }
            else
            {
                EditorGUI.PropertyField(nameRect, nameProp, new GUIContent("名称"));
            }
            currentY += singleLineHeight + verticalSpacing;

            // 2. 计算方式字段
            var calculationRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(calculationRect, calculationProp, new GUIContent("计算"));
            currentY += singleLineHeight + verticalSpacing;

            // 3. 缩放倍率字段
            var scaleRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(scaleRect, scaleProp, new GUIContent("倍率"));
            currentY += singleLineHeight + verticalSpacing;

            // 4. 玩家效果字段
            var effectFieldRect = new Rect(position.x, currentY, position.width, singleLineHeight);
            EditorGUI.PropertyField(effectFieldRect, effectFieldProp, new GUIContent("效果配置"));
            
            // 还原标签宽度
            EditorGUIUtility.labelWidth = oldLabelWidth;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 计算总高度：4行 + 3个间距
            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            return singleLineHeight * 4 + verticalSpacing * 3;
        }
    }
}
