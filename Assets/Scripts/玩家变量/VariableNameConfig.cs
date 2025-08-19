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
        
        // 修复：改为List类型以支持Unity JsonUtility序列化
        public List<DependencyRule> DependencyRules = new List<DependencyRule>();
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

        [Header("变量依赖规则配置")]
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
                try
                {
                    string json = File.ReadAllText(JsonPath);
                    VariableData data = JsonUtility.FromJson<VariableData>(json);
                    
                    VariableNames = data.VariableNames ?? new List<string>();
                    StatNames = data.StatNames ?? new List<string>();
                    PlayerAttributeNames = data.PlayerAttributeNames ?? new List<string>();
                    
                    // 修复：直接使用List类型
                    DependencyRules = data.DependencyRules ?? new List<DependencyRule>();
                    
                    Debug.Log($"成功从JSON加载配置，依赖规则数量：{DependencyRules.Count}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"加载JSON文件失败: {e.Message}");
                    InitializeDefaultValues();
                }
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {JsonPath}. 将使用当前或默认值。");
                InitializeDefaultValues();
            }
        }

        private void InitializeDefaultValues()
        {
            VariableNames = VariableNames ?? new List<string>();
            StatNames = StatNames ?? new List<string>();
            PlayerAttributeNames = PlayerAttributeNames ?? new List<string>();
            DependencyRules = DependencyRules ?? new List<DependencyRule>();
        }

        private void SaveToJson()
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(JsonPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                VariableData data = new VariableData
                {
                    VariableNames = this.VariableNames ?? new List<string>(),
                    StatNames = this.StatNames ?? new List<string>(),
                    PlayerAttributeNames = this.PlayerAttributeNames ?? new List<string>(),
                    DependencyRules = this.DependencyRules ?? new List<DependencyRule>()
                };
                
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(JsonPath, json);
                
                Debug.Log($"成功保存配置到JSON，依赖规则数量：{data.DependencyRules.Count}");
                
                // 使用延迟调用来避免在资源导入期间刷新
                UnityEditor.EditorApplication.delayCall += () => {
                    if (!UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
                    {
                        UnityEditor.AssetDatabase.Refresh();
                    }
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存JSON文件失败: {e.Message}");
            }
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

                if (string.IsNullOrEmpty(rule.key))
                {
                    Debug.LogWarning($"配置警告: 依赖规则第 {i + 1} 个的key为空，建议填写以便管理。", this);
                }

                if (string.IsNullOrEmpty(rule.target))
                {
                    Debug.LogError($"配置错误: 依赖规则第 {i + 1} 个的目标变量不能为空。", this);
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
        
        // 工具方法：添加新的依赖规则
        [ContextMenu("添加测试依赖规则")]
        private void AddTestDependencyRule()
        {
            if (DependencyRules == null)
                DependencyRules = new List<DependencyRule>();
                
            DependencyRules.Add(new DependencyRule
            {
                key = "test_rule_" + System.DateTime.Now.Ticks,
                target = "数据_固定值_战力值",
                condition = DependencyCondition.大于,
                action = DependencyAction.设置为倍数值,
                multiplier = 1.5f
            });
            
            Debug.Log("已添加测试依赖规则");
        }
#endif
    }
}