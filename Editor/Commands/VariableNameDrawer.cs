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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Always load the names if they haven't been loaded yet.
            if (variableNames == null)
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

        private void LoadVariableNames()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                var data = JsonUtility.FromJson<VariableData>(json);
                variableNames = data?.VariableNames ?? new List<string>();
            }
            else
            {
                variableNames = new List<string>();
                Debug.LogWarning("未找到变量名配置文件: " + JsonPath);
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
