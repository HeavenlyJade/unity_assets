using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using MiGame.Commands;

#if UNITY_EDITOR
namespace MiGame.CommandSystem.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var keysProp = property.FindPropertyRelative("keys");
            return (keysProp.arraySize + 2) * EditorGUIUtility.singleLineHeight + 5;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (property.isExpanded)
            {
                var keysProp = property.FindPropertyRelative("keys");
                var valuesProp = property.FindPropertyRelative("values");

                EditorGUI.indentLevel++;
                
                float y = position.y + EditorGUIUtility.singleLineHeight;

                for (int i = 0; i < keysProp.arraySize; i++)
                {
                    var keyProp = keysProp.GetArrayElementAtIndex(i);
                    var valueProp = valuesProp.GetArrayElementAtIndex(i);

                    float halfWidth = position.width / 2 - 25;
                    
                    EditorGUI.PropertyField(new Rect(position.x, y, halfWidth, EditorGUIUtility.singleLineHeight), keyProp, GUIContent.none);
                    EditorGUI.PropertyField(new Rect(position.x + halfWidth + 5, y, halfWidth, EditorGUIUtility.singleLineHeight), valueProp, GUIContent.none);
                    
                    if (GUI.Button(new Rect(position.x + position.width - 20, y, 20, EditorGUIUtility.singleLineHeight), "-"))
                    {
                        keysProp.DeleteArrayElementAtIndex(i);
                        valuesProp.DeleteArrayElementAtIndex(i);
                    }
                    
                    y += EditorGUIUtility.singleLineHeight;
                }

                if (GUI.Button(new Rect(position.x + position.width - 40, y, 20, EditorGUIUtility.singleLineHeight), "+"))
                {
                    keysProp.arraySize++;
                    valuesProp.arraySize++;
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }
    }
}
#endif 