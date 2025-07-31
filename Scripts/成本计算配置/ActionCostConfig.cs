using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Pet;
using MiGame.Skills;
using MiGame.Achievement;

namespace MiGame.Data
{
    /// <summary>
    /// 配置类型枚举
    /// </summary>
    public enum 配置类型
    {
        天赋,
        宠物,
        伙伴,
        技能
    }

    public enum 消耗类型
    {
        物品,
        玩家变量,
        玩家属性
    }

    public enum 目标类型
    {
        玩家变量,
        玩家属性
    }

    [Serializable]
    public class 消耗项目
    {
        public 消耗类型 消耗类型;
        [Tooltip("要消耗的物品或变量的名称。")]
        public string 消耗名称;
        public List<分段> 数量分段;
    }

    [Serializable]
    public class 作用目标
    {
        public 目标类型 目标类型;
        [Tooltip("目标变量或属性的名称。")]
        public string 目标名称;
        [Tooltip("作用数值。变量格式: [物品], $玩家变量$, {玩家属性}, T_LVL (关联配置的等级)。")]
        public string 作用数值;
    }

    [Serializable]
    public class 分段
    {
        [Tooltip("条件。变量格式: [物品], $玩家变量$, {玩家属性}, T_LVL (关联配置的等级)。")]
        public string 条件;
        [Tooltip("公式。变量格式: [物品], $玩家变量$, {玩家属性}, T_LVL (关联配置的等级)。")]
        public string 公式;
    }

    /// <summary>
    /// 通用动作成本配置文件。
    /// 可以为游戏中任何需要计算成本的动作（如重生、技能施放、建造、强化）创建实例。
    /// </summary>
    [CreateAssetMenu(fileName = "NewActionCost", menuName = "成本配置")]
    public class ActionCostConfig : ScriptableObject
    {
        [Header("核心标识")]
        [Tooltip("选择配置类型")]
        public 配置类型 配置类型;

        [Tooltip("配置名称")]
        [ConfigSelector("配置类型")]
        public ScriptableObject 配置名称;

        [Header("消耗列表")]
        [Tooltip("定义该动作需要消耗的资源列表。")]
        public List<消耗项目> 消耗列表;

        [Header("作用目标")]
        [Tooltip("定义该动作会影响的目标列表。")]
        public List<作用目标> 作用目标列表;

#if UNITY_EDITOR
        [System.Serializable]
        private class VariableData
        {
            public List<string> VariableNames = new List<string>();
            public List<string> StatNames = new List<string>();
        }

        [System.Serializable]
        private class ItemNameListWrapper
        {
            public List<string> ItemNames = new List<string>();
        }

        private void OnValidate()
        {
            // 每次校验时，都直接加载最新的JSON数据，确保数据一致性
            LoadAllVariableNamesFromJson(out var allVariableNames, out var allStatNames);
            LoadAllItemNamesFromJson(out var allItemNames);

            if (allVariableNames == null || allStatNames == null || allItemNames == null) return; // 如果加载失败，则不校验

            if (消耗列表 == null) return;
            foreach (var item in 消耗列表)
            {
                // 校验消耗名称本身是否存在
                if (item.消耗类型 == 消耗类型.物品)
                {
                    if (!string.IsNullOrEmpty(item.消耗名称) && !allItemNames.Contains(item.消耗名称))
                    {
                        Debug.LogError($"配置错误: '消耗名称'中的物品 '{item.消耗名称}' 在 ItemNames.json 中未定义!", this);
                    }
                }
                else if (item.消耗类型 == 消耗类型.玩家变量)
                {
                    if (!string.IsNullOrEmpty(item.消耗名称) && !allVariableNames.Contains(item.消耗名称))
                    {
                        Debug.LogError($"配置错误: '消耗名称'中的玩家变量 '{item.消耗名称}' 在 VariableNames.json 中未定义!", this);
                    }
                }
                else if (item.消耗类型 == 消耗类型.玩家属性)
                {
                    if (!string.IsNullOrEmpty(item.消耗名称) && !allStatNames.Contains(item.消耗名称))
                    {
                        Debug.LogError($"配置错误: '消耗名称'中的玩家属性 '{item.消耗名称}' 在 VariableNames.json 中未定义!", this);
                    }
                }


                if (item.数量分段 == null) continue;
                foreach (var segment in item.数量分段)
                {
                    ValidateFormulaString(segment.条件, "条件", allVariableNames, allStatNames, allItemNames);
                    ValidateFormulaString(segment.公式, "公式", allVariableNames, allStatNames, allItemNames);
                }
            }

            // 校验作用目标列表
            if (作用目标列表 != null)
            {
                foreach (var target in 作用目标列表)
                {
                    // 校验目标名称是否存在
                    if (target.目标类型 == 目标类型.玩家变量)
                    {
                        if (!string.IsNullOrEmpty(target.目标名称) && !allVariableNames.Contains(target.目标名称))
                        {
                            Debug.LogError($"配置错误: '作用目标'中的玩家变量 '{target.目标名称}' 在 VariableNames.json 中未定义!", this);
                        }
                    }
                    else if (target.目标类型 == 目标类型.玩家属性)
                    {
                        if (!string.IsNullOrEmpty(target.目标名称) && !allStatNames.Contains(target.目标名称))
                        {
                            Debug.LogError($"配置错误: '作用目标'中的玩家属性 '{target.目标名称}' 在 VariableNames.json 中未定义!", this);
                        }
                    }

                    // 校验作用数值中的变量引用
                    if (!string.IsNullOrEmpty(target.作用数值))
                    {
                        ValidateFormulaString(target.作用数值, "作用数值", allVariableNames, allStatNames, allItemNames);
                    }
                }
            }
        }

        private void LoadAllItemNamesFromJson(out HashSet<string> itemNames)
        {
            itemNames = null;
            string jsonPath = "Assets/GameConf/物品/ItemNames.json";
            if (System.IO.File.Exists(jsonPath))
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                var data = JsonUtility.FromJson<ItemNameListWrapper>(json);
                if (data != null)
                {
                    itemNames = new HashSet<string>(data.ItemNames ?? new List<string>());
                }
            }
            else
            {
                 Debug.LogWarning($"JSON 文件未找到: {jsonPath}. 物品名校验功能将不会执行。", this);
                 itemNames = new HashSet<string>(); // 即使文件不存在，也返回一个空集合以避免null错误
            }
        }

        private void LoadAllVariableNamesFromJson(out HashSet<string> variableNames, out HashSet<string> statNames)
        {
            variableNames = null;
            statNames = null;
            
            string jsonPath = "Assets/GameConf/玩家变量/VariableNames.json";
            if (System.IO.File.Exists(jsonPath))
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                VariableData data = JsonUtility.FromJson<VariableData>(json);

                if (data != null)
                {
                    variableNames = new HashSet<string>(data.VariableNames ?? new List<string>());
                    statNames = new HashSet<string>(data.StatNames ?? new List<string>());
                }
            }
            else
            {
                Debug.LogWarning($"JSON 文件未找到: {jsonPath}. 变量名校验功能将不会执行。", this);
            }
        }

        private void ValidateFormulaString(string formula, string fieldName, HashSet<string> variableNames, HashSet<string> statNames, HashSet<string> itemNames)
        {
            if (string.IsNullOrEmpty(formula)) return;

            // 校验玩家变量: $...$
            var playerVarRegex = new System.Text.RegularExpressions.Regex(@"\$([^\$]+)\$");
            foreach (System.Text.RegularExpressions.Match match in playerVarRegex.Matches(formula))
            {
                var variableName = match.Groups[1].Value;
                if (!variableNames.Contains(variableName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 玩家变量 '${variableName}$' 在 VariableNames.json 中未定义!", this);
                }
            }

            // 校验玩家属性: {...}
            var statRegex = new System.Text.RegularExpressions.Regex(@"\{([^\}]+)\}");
            foreach (System.Text.RegularExpressions.Match match in statRegex.Matches(formula))
            {
                var variableName = match.Groups[1].Value;
                if (!statNames.Contains(variableName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 玩家属性 '{{{variableName}}}' 在 VariableNames.json 中未定义!", this);
                }
            }

            // 校验物品: [...]
            var itemRegex = new System.Text.RegularExpressions.Regex(@"\[([^\]]+)\]");
            foreach (System.Text.RegularExpressions.Match match in itemRegex.Matches(formula))
            {
                var itemName = match.Groups[1].Value;
                if (!itemNames.Contains(itemName))
                {
                    Debug.LogError($"配置错误: 在 '{fieldName}' 字段中, 物品 '[{itemName}]' 在 ItemNames.json 中未定义!", this);
                }
            }
        }
#endif
    }
} 