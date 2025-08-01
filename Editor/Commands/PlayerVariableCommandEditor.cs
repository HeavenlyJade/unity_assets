using UnityEditor;
using MiGame.Commands;
using UnityEngine;

namespace MiGame.CommandSystem.Editor
{
    [CustomEditor(typeof(PlayerVariableCommand))]
    public class PlayerVariableCommandEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("操作类型"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("玩家UID"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("变量名"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("数值"));
            
            EditorGUILayout.Space();

            DrawBonusList(serializedObject.FindProperty("玩家属性加成"));
            DrawBonusList(serializedObject.FindProperty("玩家变量加成"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBonusList(SerializedProperty listProperty)
        {
            // Draw the list's foldout header.
            EditorGUILayout.PropertyField(listProperty, false); 

            if (listProperty.isExpanded)
            {
                EditorGUI.indentLevel++;

                listProperty.arraySize = EditorGUILayout.IntField("Size", listProperty.arraySize);

                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(element, new GUIContent("Element " + i), true);
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
