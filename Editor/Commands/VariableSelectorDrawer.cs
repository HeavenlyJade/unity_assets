using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace MiGame
{
    [CustomPropertyDrawer(typeof(VariableSelectorAttribute))]
    public class VariableSelectorDrawer : PropertyDrawer
    {
        private const string JsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        private const string NoneOption = "(不选择)";
        private static Dictionary<VariableNameType, List<string>> cachedNames = new Dictionary<VariableNameType, List<string>>();
        private static bool cacheInitialized = false;
        private static System.DateTime lastFileWriteTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectorAttribute = (VariableSelectorAttribute)attribute;
            DrawSelector(position, property, label, selectorAttribute.NameType);
        }

        public static void DrawSelector(Rect position, SerializedProperty property, GUIContent label, VariableNameType nameType)
        {
            // 检查是否需要重新加载数据
            if (ShouldReloadNames())
            {
                LoadAllNames();
                cacheInitialized = true;
            }

            if (cachedNames.TryGetValue(nameType, out var names) && names.Count > 0)
            {
                int currentIndex = names.IndexOf(property.stringValue);
                if (currentIndex < 0)
                {
                    currentIndex = 0; // Default to "(不选择)"
                }

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, names.ToArray());

                if (newIndex >= 0 && newIndex < names.Count)
                {
                    string selectedValue = names[newIndex];
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
        private static bool ShouldReloadNames()
        {
            // 如果还没有初始化，需要加载
            if (!cacheInitialized)
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

        private static void LoadAllNames()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                var data = JsonUtility.FromJson<VariableData>(json);

                var variableNames = data?.VariableNames ?? new List<string>();
                variableNames.Insert(0, NoneOption);
                cachedNames[VariableNameType.Variable] = variableNames;

                var statNames = data?.StatNames ?? new List<string>();
                statNames.Insert(0, NoneOption);
                cachedNames[VariableNameType.Stat] = statNames;

                var playerAttributeNames = data?.PlayerAttributeNames ?? new List<string>();
                playerAttributeNames.Insert(0, NoneOption);
                cachedNames[VariableNameType.PlayerAttribute] = playerAttributeNames;

                var 仅加成新增Names = data?.仅加成新增Names ?? new List<string>();
                仅加成新增Names.Insert(0, NoneOption);
                cachedNames[VariableNameType.仅加成新增] = 仅加成新增Names;

                // 更新文件修改时间
                lastFileWriteTime = File.GetLastWriteTime(JsonPath);
            }
            else
            {
                Debug.LogWarning("未找到变量名配置文件: " + JsonPath);
                cachedNames[VariableNameType.Variable] = new List<string> { NoneOption };
                cachedNames[VariableNameType.Stat] = new List<string> { NoneOption };
                cachedNames[VariableNameType.PlayerAttribute] = new List<string> { NoneOption };
                cachedNames[VariableNameType.仅加成新增] = new List<string> { NoneOption };
                lastFileWriteTime = System.DateTime.MinValue;
            }
        }

        public static void ClearCache()
        {
            cachedNames.Clear();
            cacheInitialized = false;
        }

        [System.Serializable]
        private class VariableData
        {
            public List<string> VariableNames = new List<string>();
            public List<string> StatNames = new List<string>();
            public List<string> PlayerAttributeNames = new List<string>();
            public List<string> 仅加成新增Names = new List<string>();
        }
    }
}
