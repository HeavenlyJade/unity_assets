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

            // 将一行拆分为三列：名称 55% | 计算方式 25% | 缩放倍率 20%
            float nameWidth = position.width * 0.55f - 5f;
            float calcWidth = position.width * 0.25f - 5f;
            float scaleWidth = position.width * 0.20f;
            var nameRect = new Rect(position.x, position.y, nameWidth, position.height);
            var calculationRect = new Rect(position.x + nameWidth + 5f, position.y, calcWidth, position.height);
            var scaleRect = new Rect(position.x + nameWidth + 5f + calcWidth + 5f, position.y, scaleWidth, position.height);
            
            // 设置紧凑的标签宽度
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40f;

            // Look for our new [BonusType] attribute on the list field
            var bonusTypeAttributes = fieldInfo.GetCustomAttributes(typeof(BonusTypeAttribute), true);
            if (bonusTypeAttributes.Any())
            {
                var bonusTypeAttr = bonusTypeAttributes[0] as BonusTypeAttribute;
                // Use the static method from VariableSelectorDrawer to draw the dropdown
                VariableSelectorDrawer.DrawSelector(nameRect, nameProp, new GUIContent("名称"), bonusTypeAttr.NameType);
            }
            else
            {
                // Fallback to a normal text field if the attribute is missing
                EditorGUI.PropertyField(nameRect, nameProp, new GUIContent("名称"));
            }

            EditorGUI.PropertyField(calculationRect, calculationProp, new GUIContent("计算"));
            EditorGUI.PropertyField(scaleRect, scaleProp, new GUIContent("倍率"));
            
            // 还原标签宽度
            EditorGUIUtility.labelWidth = oldLabelWidth;
            
            EditorGUI.EndProperty();
        }
    }
}
