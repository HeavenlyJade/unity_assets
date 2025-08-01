using UnityEditor;
using UnityEngine;
using MiGame.Core;
using System.Linq;

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

            var nameRect = new Rect(position.x, position.y, position.width * 0.7f - 5, position.height);
            var calculationRect = new Rect(position.x + position.width * 0.7f, position.y, position.width * 0.3f, position.height);
            
            // Look for our new [BonusType] attribute on the list field
            var bonusTypeAttributes = fieldInfo.GetCustomAttributes(typeof(BonusTypeAttribute), true);
            if (bonusTypeAttributes.Any())
            {
                var bonusTypeAttr = bonusTypeAttributes[0] as BonusTypeAttribute;
                // Use the static method from VariableSelectorDrawer to draw the dropdown
                VariableSelectorDrawer.DrawSelector(nameRect, nameProp, GUIContent.none, bonusTypeAttr.NameType);
            }
            else
            {
                // Fallback to a normal text field if the attribute is missing
                EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
            }

            EditorGUI.PropertyField(calculationRect, calculationProp, GUIContent.none);
            
            EditorGUI.EndProperty();
        }
    }
}
