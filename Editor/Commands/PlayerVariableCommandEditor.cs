using UnityEditor;
using MiGame.Commands;

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
            
            // 使用默认的列表绘制，这样会自动调用我们的 PlayerBonusDrawer
            EditorGUILayout.PropertyField(serializedObject.FindProperty("玩家属性加成"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("玩家变量加成"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("其他加成"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}