using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace MiGame.Commands
{
    /// <summary>
    /// PlayerAttributeName 属性的自定义绘制器
    /// 从 PlayerAttributeNames 配置中读取属性名称列表，显示为下拉选择框
    /// </summary>
    [CustomPropertyDrawer(typeof(PlayerAttributeNameAttribute))]
    public class PlayerAttributeNameDrawer : PropertyDrawer
    {
        private const string JsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
        private const string NoneOption = "(不选择)";
        private static List<string> playerAttributeNames;
        private static System.DateTime lastFileWriteTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 检查文件是否已被修改，如果是则重新加载
            if (ShouldReloadPlayerAttributeNames())
            {
                LoadPlayerAttributeNames();
            }

            if (playerAttributeNames != null && playerAttributeNames.Count > 0)
            {
                int currentIndex = playerAttributeNames.IndexOf(property.stringValue);
                // 如果当前值不在列表中（例如空字符串），默认为 "(不选择)"
                if (currentIndex < 0)
                {
                    currentIndex = playerAttributeNames.IndexOf(NoneOption);
                }

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, playerAttributeNames.ToArray());

                if (newIndex >= 0 && newIndex < playerAttributeNames.Count)
                {
                    string selectedValue = playerAttributeNames[newIndex];
                    // 如果选择了 "(不选择)"，则将属性设置为空字符串
                    property.stringValue = (selectedValue == NoneOption) ? "" : selectedValue;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        /// <summary>
        /// 检查是否需要重新加载玩家属性名称
        /// </summary>
        private bool ShouldReloadPlayerAttributeNames()
        {
            if (!File.Exists(JsonPath))
                return false;

            try
            {
                var currentWriteTime = File.GetLastWriteTime(JsonPath);
                if (currentWriteTime != lastFileWriteTime)
                {
                    lastFileWriteTime = currentWriteTime;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"检查文件修改时间时出错: {e.Message}");
            }

            return false;
        }

        /// <summary>
        /// 从JSON文件加载玩家属性名称
        /// </summary>
        private void LoadPlayerAttributeNames()
        {
            try
            {
                if (File.Exists(JsonPath))
                {
                    string json = File.ReadAllText(JsonPath);
                    var data = JsonUtility.FromJson<VariableData>(json);
                    
                    playerAttributeNames = new List<string>();
                    playerAttributeNames.Add(NoneOption); // 添加"不选择"选项
                    
                    if (data.PlayerAttributeNames != null)
                    {
                        playerAttributeNames.AddRange(data.PlayerAttributeNames);
                    }
                    
                    Debug.Log($"已加载 {playerAttributeNames.Count - 1} 个玩家属性名称");
                }
                else
                {
                    Debug.LogWarning($"玩家属性名称配置文件未找到: {JsonPath}");
                    playerAttributeNames = new List<string> { NoneOption };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载玩家属性名称时出错: {e.Message}");
                playerAttributeNames = new List<string> { NoneOption };
            }
        }

        /// <summary>
        /// 用于JSON反序列化的数据结构
        /// </summary>
        [System.Serializable]
        private class VariableData
        {
            public List<string> PlayerAttributeNames;
        }
    }
}
