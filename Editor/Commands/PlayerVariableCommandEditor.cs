using UnityEditor;
using UnityEngine;

namespace MiGame.Commands
{
    [CustomEditor(typeof(PlayerVariableCommand))]
    public class PlayerVariableCommandEditor : UnityEditor.Editor
    {
        SerializedProperty operationTypeProp;
        SerializedProperty uidProp;
        SerializedProperty variableNameProp;
        SerializedProperty valueProp;

        void OnEnable()
        {
            operationTypeProp = serializedObject.FindProperty("操作类型");
            uidProp = serializedObject.FindProperty("玩家UID");
            variableNameProp = serializedObject.FindProperty("变量名");
            valueProp = serializedObject.FindProperty("数值");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(operationTypeProp);

            var opType = (VariableOperationType)operationTypeProp.enumValueIndex;

            EditorGUILayout.PropertyField(uidProp);
            
            // Only show the variable name field if the operation is not "View"
            // or if it is "View" and a variable name has been provided.
            EditorGUILayout.PropertyField(variableNameProp);

            // Only show the value field if the operation is not "View".
            if (opType != VariableOperationType.查看)
            {
                EditorGUILayout.PropertyField(valueProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 