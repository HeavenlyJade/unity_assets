using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MiGame.Utils.Editor
{
    [CustomPropertyDrawer(typeof(携带效果))]
    public class 携带效果Drawer : PropertyDrawer
    {
        private static List<string> variableNames;
        private static List<string> statNames;
        private static bool isInitialized = false;

        // 公共方法，用于从外部使缓存失效
        public static void InvalidateCache()
        {
            isInitialized = false;
        }

        // 初始化时读取JSON，缓存变量名列表
        private static void Initialize()
        {
            if (isInitialized) return;

            string jsonPath = Path.Combine(Application.dataPath, "GameConf/玩家变量/VariableNames.json");
            if (File.Exists(jsonPath))
            {
                string jsonText = File.ReadAllText(jsonPath);
                JObject json = JObject.Parse(jsonText);
                variableNames = json["VariableNames"]?.ToObject<List<string>>() ?? new List<string>();
                statNames = json["StatNames"]?.ToObject<List<string>>() ?? new List<string>();
                Debug.Log("携带效果Drawer: 已重新加载VariableNames.json");
            }
            else
            {
                variableNames = new List<string>();
                statNames = new List<string>();
                Debug.LogError("未找到 VariableNames.json 文件! 路径: " + jsonPath);
            }
            isInitialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();

            EditorGUI.BeginProperty(position, label, property);

            // 可折叠的总标签
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                var currentPos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

                // 获取所有需要的属性
                var 变量类型Prop = property.FindPropertyRelative("变量类型");
                var 变量名称Prop = property.FindPropertyRelative("变量名称");
                var 效果数值Prop = property.FindPropertyRelative("效果数值");
                var 加成类型Prop = property.FindPropertyRelative("加成类型");
                var 物品目标Prop = property.FindPropertyRelative("物品目标");

                // 1. 绘制 变量类型
                EditorGUI.PropertyField(currentPos, 变量类型Prop);
                currentPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // 2. 根据 变量类型 的值，绘制 变量名称 的下拉菜单
                变量类型 currentVarType = (变量类型)变量类型Prop.enumValueIndex;
                if (currentVarType == 变量类型.玩家变量)
                {
                    DrawStringPopup(currentPos, 变量名称Prop, "变量名称", variableNames);
                }
                else if (currentVarType == 变量类型.玩家属性)
                {
                    DrawStringPopup(currentPos, 变量名称Prop, "属性名称", statNames);
                }
                else
                {
                    // 如果未来有其他类型，则显示为普通文本框
                    EditorGUI.PropertyField(currentPos, 变量名称Prop, new GUIContent("变量名称"));
                }
                currentPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // 3. 绘制 效果数值
                EditorGUI.PropertyField(currentPos, 效果数值Prop, new GUIContent("效果数值"));
                currentPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // 4. 绘制 加成类型 (作用目标)
                EditorGUI.PropertyField(currentPos, 加成类型Prop, new GUIContent("加成类型 (作用目标)"));
                currentPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // 5. 如果 加成类型 是 物品，则显示 物品目标 字段
                加成类型 currentBonusType = (加成类型)加成类型Prop.enumValueIndex;
                if (currentBonusType == 加成类型.物品)
                {
                    EditorGUI.PropertyField(currentPos, 物品目标Prop, new GUIContent("物品目标"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // 绘制字符串下拉菜单的辅助方法
        private void DrawStringPopup(Rect position, SerializedProperty property, string label, List<string> options)
        {
            if (options == null || options.Count == 0)
            {
                EditorGUI.PropertyField(position, property, new GUIContent(label)); // Fallback to text field
                return;
            }

            int currentIndex = options.IndexOf(property.stringValue);
            int newIndex = EditorGUI.Popup(position, label, currentIndex, options.ToArray());

            if (newIndex >= 0 && newIndex < options.Count)
            {
                property.stringValue = options[newIndex];
            }
            // If newIndex is -1 (nothing selected), we don't clear the string value to avoid accidental data loss.
        }

        // 动态计算属性高度
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float lineCount = 5; // 默认显示5行 (Foldout + 4个基本字段)
            var 加成类型Prop = property.FindPropertyRelative("加成类型");
            if ((加成类型)加成类型Prop.enumValueIndex == 加成类型.物品)
            {
                lineCount++; // 如果是物品，额外增加一行
            }

            return (EditorGUIUtility.singleLineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * (lineCount -1)) ;
        }
    }
}
