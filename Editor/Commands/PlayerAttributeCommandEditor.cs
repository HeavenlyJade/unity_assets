using UnityEditor;
using UnityEngine;
using MiGame.Commands;

namespace MiGame.CommandSystem.Editor
{
    /// <summary>
    /// PlayerAttributeCommand 的自定义编辑器
    /// 提供友好的Inspector界面，支持所有操作类型和加成配置
    /// </summary>
    [CustomEditor(typeof(PlayerAttributeCommand))]
    public class PlayerAttributeCommandEditor : UnityEditor.Editor
    {
        private SerializedProperty 操作类型Prop;
        private SerializedProperty 玩家UIDProp;
        private SerializedProperty 属性名Prop;
        private SerializedProperty 数值Prop;
        private SerializedProperty 最终倍率Prop;
        private SerializedProperty 来源Prop;
        private SerializedProperty 玩家属性加成Prop;
        private SerializedProperty 玩家变量加成Prop;
        private SerializedProperty 其他加成Prop;

        private void OnEnable()
        {
            操作类型Prop = serializedObject.FindProperty("操作类型");
            玩家UIDProp = serializedObject.FindProperty("玩家UID");
            属性名Prop = serializedObject.FindProperty("属性名");
            数值Prop = serializedObject.FindProperty("数值");
            最终倍率Prop = serializedObject.FindProperty("最终倍率");
            来源Prop = serializedObject.FindProperty("来源");
            玩家属性加成Prop = serializedObject.FindProperty("玩家属性加成");
            玩家变量加成Prop = serializedObject.FindProperty("玩家变量加成");
            其他加成Prop = serializedObject.FindProperty("其他加成");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            DrawTitle("玩家属性操作指令");
            EditorGUILayout.Space(5);

            // 基础参数区域
            DrawBasicParameters();

            EditorGUILayout.Space(10);

            // 加成配置区域
            DrawBonusConfiguration();

            EditorGUILayout.Space(10);

            // 操作提示区域
            DrawOperationTips();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawTitle(string title)
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.6f, 1.0f) }
            };

            EditorGUILayout.LabelField(title, titleStyle);
        }

        /// <summary>
        /// 绘制基础参数区域
        /// </summary>
        private void DrawBasicParameters()
        {
            EditorGUILayout.LabelField("基础参数", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // 操作类型
            EditorGUILayout.PropertyField(操作类型Prop, new GUIContent("操作类型", "选择要执行的属性操作类型"));

            // 玩家UID
            EditorGUILayout.PropertyField(玩家UIDProp, new GUIContent("玩家UID", "目标玩家的唯一标识符"));

            // 属性名（除了查看操作外都需要）
            var currentOperation = (AttributeOperationType)操作类型Prop.enumValueIndex;
            if (currentOperation != AttributeOperationType.查看)
            {
                EditorGUILayout.PropertyField(属性名Prop, new GUIContent("属性名", "要操作的属性名称"));
            }
            else
            {
                EditorGUILayout.PropertyField(属性名Prop, new GUIContent("属性名", "要查看的属性名称（留空查看所有属性）"));
            }

            // 数值（除了查看、恢复、刷新、测试加成操作外都需要）
            if (currentOperation == AttributeOperationType.新增 || 
                currentOperation == AttributeOperationType.设置 || 
                currentOperation == AttributeOperationType.减少 ||
                currentOperation == AttributeOperationType.仅加成新增)
            {
                EditorGUILayout.PropertyField(数值Prop, new GUIContent("数值", "属性操作的数值（支持整数、小数、长整型）"));
            }
            else
            {
                EditorGUILayout.PropertyField(数值Prop, new GUIContent("数值", "属性操作的数值（当前操作类型不需要）"));
                GUI.enabled = false;
                EditorGUILayout.PropertyField(数值Prop);
                GUI.enabled = true;
            }

            // 最终倍率
            EditorGUILayout.PropertyField(最终倍率Prop, new GUIContent("最终倍率", "属性操作的最终倍率，影响最终计算结果的倍数"));

            // 来源
            EditorGUILayout.PropertyField(来源Prop, new GUIContent("来源", "属性操作的来源类型"));

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制加成配置区域
        /// </summary>
        private void DrawBonusConfiguration()
        {
            EditorGUILayout.LabelField("加成配置", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // 玩家属性加成
            EditorGUILayout.PropertyField(玩家属性加成Prop, new GUIContent("玩家属性加成", "为玩家提供的属性加成列表"), true);

            // 玩家变量加成
            EditorGUILayout.PropertyField(玩家变量加成Prop, new GUIContent("玩家变量加成", "为玩家提供的变量加成列表"), true);

            // 其他加成
            EditorGUILayout.PropertyField(其他加成Prop, new GUIContent("其他加成", "其他加成类型（宠物、伙伴、尾迹、翅膀等）"), true);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制操作提示区域
        /// </summary>
        private void DrawOperationTips()
        {
            EditorGUILayout.LabelField("操作说明", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            var currentOperation = (AttributeOperationType)操作类型Prop.enumValueIndex;
            string tipText = GetOperationTip(currentOperation);

            EditorGUILayout.HelpBox(tipText, MessageType.Info);

            // 显示当前配置的加成信息
            if (HasBonusConfiguration())
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("当前加成配置:", EditorStyles.miniLabel);
                
                if (玩家属性加成Prop.arraySize > 0)
                {
                    EditorGUILayout.LabelField($"  属性加成: {玩家属性加成Prop.arraySize} 项", EditorStyles.miniLabel);
                }
                
                if (玩家变量加成Prop.arraySize > 0)
                {
                    EditorGUILayout.LabelField($"  变量加成: {玩家变量加成Prop.arraySize} 项", EditorStyles.miniLabel);
                }
                
                if (其他加成Prop.FindPropertyRelative("items").arraySize > 0)
                {
                    var otherBonusItems = 其他加成Prop.FindPropertyRelative("items");
                    string otherBonusText = "";
                    for (int i = 0; i < otherBonusItems.arraySize; i++)
                    {
                        if (i > 0) otherBonusText += ", ";
                        otherBonusText += otherBonusItems.GetArrayElementAtIndex(i).stringValue;
                    }
                    EditorGUILayout.LabelField($"  其他加成: {otherBonusText}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 获取操作提示文本
        /// </summary>
        private string GetOperationTip(AttributeOperationType operationType)
        {
            switch (operationType)
            {
                case AttributeOperationType.新增:
                    return "新增：在玩家当前属性值基础上增加指定数值。支持各种数值类型，会自动应用配置的加成效果。";
                    
                case AttributeOperationType.设置:
                    return "设置：将玩家属性值设置为指定数值，覆盖原有值。支持各种数值类型，会自动应用配置的加成效果。";
                    
                case AttributeOperationType.减少:
                    return "减少：在玩家当前属性值基础上减少指定数值。支持各种数值类型，会自动应用配置的加成效果。";
                    
                case AttributeOperationType.查看:
                    return "查看：查看玩家指定属性或所有属性的当前值。属性名留空时查看所有属性。";
                    
                case AttributeOperationType.恢复:
                    return "恢复：将玩家属性恢复到默认状态，清除所有临时修改和加成效果。";
                    
                case AttributeOperationType.刷新:
                    return "刷新：重新计算并应用所有加成效果，更新最终属性值。适用于加成配置变更后的状态同步。";
                    
                case AttributeOperationType.测试加成:
                    return "测试加成：模拟计算当前加成配置的效果，显示详细的加成计算过程和最终结果。";
                    
                case AttributeOperationType.仅加成新增:
                    return "仅加成新增：仅增加属性加成效果，不修改基础属性值。适用于需要临时提升属性但保持基础值不变的场景。";
                    
                default:
                    return "请选择操作类型以查看详细说明。";
            }
        }

        /// <summary>
        /// 检查是否有加成配置
        /// </summary>
        private bool HasBonusConfiguration()
        {
            return (玩家属性加成Prop.arraySize > 0) || 
                   (玩家变量加成Prop.arraySize > 0) || 
                   (其他加成Prop.FindPropertyRelative("items").arraySize > 0);
        }
    }
}
