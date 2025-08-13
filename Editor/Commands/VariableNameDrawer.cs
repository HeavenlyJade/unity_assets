using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace MiGame.Commands
{
    [CustomPropertyDrawer(typeof(VariableNameAttribute))]
    public class VariableNameDrawer : PropertyDrawer
    {
        private const string JsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        private const string NoneOption = "(不选择)";
        private List<string> variableNames;
        private System.DateTime lastFileWriteTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 检查文件是否已被修改，如果是则重新加载
            if (ShouldReloadVariableNames())
            {
                LoadVariableNames();
            }

            if (variableNames != null && variableNames.Count > 0)
            {
                int currentIndex = variableNames.IndexOf(property.stringValue);
                // If the current value is not in the list (e.g., empty string), default to "(不选择)"
                if (currentIndex < 0)
                {
                    currentIndex = variableNames.IndexOf(NoneOption);
                }

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, variableNames.ToArray());

                if (newIndex >= 0 && newIndex < variableNames.Count)
                {
                    string selectedValue = variableNames[newIndex];
                    // If "(不选择)" is chosen, set the property to an empty string.
                    property.stringValue = (selectedValue == NoneOption) ? "" : selectedValue;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        /// <summary>
        /// 检查是否需要重新加载变量名列表
        /// </summary>
        private bool ShouldReloadVariableNames()
        {
            // 如果还没有加载过，需要加载
            if (variableNames == null)
                return true;

            // 如果文件不存在，不需要重新加载
            if (!File.Exists(JsonPath))
                return false;

            // 检查文件修改时间是否发生变化
            var currentFileWriteTime = File.GetLastWriteTime(JsonPath);
            if (currentFileWriteTime != lastFileWriteTime)
            {
                lastFileWriteTime = currentFileWriteTime;
                return true;
            }

            return false;
        }

        private void LoadVariableNames()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                var data = JsonUtility.FromJson<VariableData>(json);
                variableNames = data?.VariableNames ?? new List<string>();
                
                // 更新文件修改时间
                lastFileWriteTime = File.GetLastWriteTime(JsonPath);
            }
            else
            {
                variableNames = new List<string>();
                Debug.LogWarning("未找到变量名配置文件: " + JsonPath);
                lastFileWriteTime = System.DateTime.MinValue;
            }

            // Always add the "(不选择)" option at the top of the list.
            variableNames.Insert(0, NoneOption);
        }

        [System.Serializable]
        private class VariableData
        {
            public List<string> VariableNames = new List<string>();
        }
    }
}
