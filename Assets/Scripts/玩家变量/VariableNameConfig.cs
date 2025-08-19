using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MiGame.Data
{
    [System.Serializable]
    public enum DependencyCondition
    {
        大于,
        大于等于,
        变化时
    }

    [System.Serializable]
    public enum DependencyAction
    {
        设置为源值,
        设置为固定值,
        设置为倍数值
    }

    [System.Serializable]
    public class DependencyRule
    {
        public string key;              // 依赖规则的唯一标识符
        public string target;           // 目标变量
        public DependencyCondition condition;        // 条件（大于、大于等于、变化时等）
        public DependencyAction action;           // 动作（设置为源值、设置为固定值、设置为倍数值等）
        public float value;             // 固定值（可选）
        public float multiplier;        // 倍数（可选）
    }

    [System.Serializable]
    public class VariableData
    {
        public List<string> VariableNames = new List<string>();
        public List<string> StatNames = new List<string>();
        public List<string> PlayerAttributeNames = new List<string>();
        public Dictionary<string, DependencyRule> DependencyRules = new Dictionary<string, DependencyRule>();
    }

    [CreateAssetMenu(fileName = "VariableNameConfig", menuName = "配置/变量名配置")]
    public class VariableNameConfig : ScriptableObject
    {
        [Header("玩家变量 (用于 VariableSystem)")]
        public List<string> VariableNames;

        [Header("属性 (用于 StatSystem)")]
        public List<string> StatNames;

        [Header("玩家的属性配置")]
        public List<string> PlayerAttributeNames;



        [Header("量依赖规则配置")]
        public List<DependencyRule> DependencyRules;

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
            ValidateNames(PlayerAttributeNames, "玩家的属性配置", pattern);

            ValidateDependencyRules();

            // 保存到JSON
            SaveToJson();
        }

        private void LoadFromJson()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                VariableData data = JsonUtility.FromJson<VariableData>(json);
                VariableNames = data.VariableNames;
                StatNames = data.StatNames;
                PlayerAttributeNames = data.PlayerAttributeNames;

                
                // 加载依赖规则
                if (data.DependencyRules != null)
                {
                    DependencyRules = new List<DependencyRule>();
                    foreach (var rule in data.DependencyRules)
                    {
                        DependencyRules.Add(rule.Value);
                    }
                }
                else
                {
                    DependencyRules = new List<DependencyRule>();
                }
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {JsonPath}. 将使用当前或默认值。");
                DependencyRules = new List<DependencyRule>();
            }
        }

        private void SaveToJson()
        {
            // 转换依赖规则为字典格式
            var dependencyRulesDict = new Dictionary<string, DependencyRule>();
            if (DependencyRules != null)
            {
                foreach (var rule in DependencyRules)
                {
                    if (rule != null && !string.IsNullOrEmpty(rule.key))
                    {
                        // 使用key作为key，如果没有key则跳过
                        dependencyRulesDict[rule.key] = rule;
                    }
                }
            }

            VariableData data = new VariableData
            {
                VariableNames = this.VariableNames,
                StatNames = this.StatNames,
                PlayerAttributeNames = this.PlayerAttributeNames,
                DependencyRules = dependencyRulesDict
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

        private void ValidateDependencyRules()
        {
            if (DependencyRules == null) return;
            
            for (int i = 0; i < DependencyRules.Count; i++)
            {
                var rule = DependencyRules[i];
                if (rule == null)
                {
                    Debug.LogError($"配置错误: 依赖规则第 {i + 1} 个为空。", this);
                    continue;
                }

                if (string.IsNullOrEmpty(rule.target))
                {
                    Debug.LogError($"配置错误: 依赖规则第 {i + 1} 个的目标变量不能为空。", this);
                }

                if (string.IsNullOrEmpty(rule.condition.ToString()))
                {
                    Debug.LogError($"配置错误: 依赖规则第 {i + 1} 个的条件不能为空。", this);
                }

                if (string.IsNullOrEmpty(rule.action.ToString()))
                {
                    Debug.LogError($"配置错误: 依赖规则第 {i + 1} 个的动作不能为空。", this);
                }

                // 根据动作类型验证必要字段
                if (rule.action == DependencyAction.设置为固定值 && rule.value == 0)
                {
                    Debug.LogWarning($"配置警告: 依赖规则第 {i + 1} 个使用'设置为固定值'动作，但value字段为0。", this);
                }

                if (rule.action == DependencyAction.设置为倍数值 && rule.multiplier == 0)
                {
                    Debug.LogWarning($"配置警告: 依赖规则第 {i + 1} 个使用'设置为倍数值'动作，但multiplier字段为0。", this);
                }
            }
        }
#endif
    }
}