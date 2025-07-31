using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MiGame.Data
{
    [System.Serializable]
    public class VariableData
    {
        public List<string> VariableNames = new List<string>();
        public List<string> StatNames = new List<string>();
    }

    [CreateAssetMenu(fileName = "VariableNameConfig", menuName = "配置/变量名配置")]
    public class VariableNameConfig : ScriptableObject
    {
        [Header("玩家变量 (用于 VariableSystem)")]
        public List<string> VariableNames;

        [Header("属性 (用于 StatSystem)")]
        public List<string> StatNames;

#if UNITY_EDITOR
        private const string JsonPath = "Assets/GameConf/玩家变量/VariableNames.json";

        // 当在编辑器中选中该资产时调用
        private void OnEnable()
        {
            LoadFromJson();
        }

        private void OnValidate()
        {
            // 校验输入合法性
            // 规则: ^ (字符串开头), [\u4e00-\u9fa5 (中文字符) a-zA-Z (英文字母) 0-9 (数字) _ (下划线)]+, (以上字符集出现1次或多次) $ (字符串结尾)
            const string pattern = @"^[\u4e00-\u9fa5a-zA-Z0-9_]+$";
            ValidateNames(VariableNames, "玩家变量", pattern);
            ValidateNames(StatNames, "玩家属性", pattern);

            // 保存到JSON
            // SaveToJson();
        }

        private void LoadFromJson()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                VariableData data = JsonUtility.FromJson<VariableData>(json);
                VariableNames = data.VariableNames;
                StatNames = data.StatNames;
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {JsonPath}. 将使用当前或默认值。");
            }
        }

        private void SaveToJson()
        {
            VariableData data = new VariableData
            {
                VariableNames = this.VariableNames,
                StatNames = this.StatNames
            };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(JsonPath, json);
            // 使用延迟调用来避免在资源导入期间刷新
            UnityEditor.EditorApplication.delayCall += () => {
                if (!UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
                {
                    UnityEditor.AssetDatabase.Refresh();
                }
            };
        }
        
        private void ValidateNames(List<string> names, string listName, string pattern)
        {
            if (names == null) return;
            for (int i = 0; i < names.Count; i++)
            {
                var variableName = names[i];
                if (string.IsNullOrEmpty(variableName))
                {
                    Debug.LogError($"配置错误: '{listName}' 列表中的第 {i + 1} 个变量名不能为空。", this);
                    continue;
                }

                if (!Regex.IsMatch(variableName, pattern))
                {
                    Debug.LogError($"配置错误: '{listName}' 列表中的变量名 '{variableName}' 包含非法字符。只允许使用中文、英文、数字和下划线。", this);
                }
            }
        }
#endif
    }
}