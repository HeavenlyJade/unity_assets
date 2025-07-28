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

        [Header("成本计算规则")]
        [Tooltip("成本计算的规则列表。系统会从上到下查找第一个满足所有'生效条件'的规则来计算最终成本。请将最特殊、优先级最高的规则放在最前面。")]
        public List<CostRule> CostRules;
    }

    /// <summary>
    /// 单条成本计算规则。
    /// 代表一个'if...then'逻辑块：如果满足所有'生效条件'，则应用此规则的'成本列表'。
    /// </summary>
    [Serializable]
    public class CostRule
    {
        [Tooltip("对此条规则的描述，方便策划理解。例如：'重生次数100次以后，消耗钻石'")]
        [TextArea(2, 4)]
        public string Description;

        [Tooltip("此规则生效必须满足的所有条件。系统会检查传入的'动作上下文'是否满足这些字符串定义的逻辑。")]
        public List<string> Conditions;

        [Tooltip("如果条件满足，此动作需要消耗的资源列表。一个动作可以消耗多种资源。")]
        public List<CostItem> Costs;
    }

    /// <summary>
    /// 单项成本定义。
    /// </summary>
    [Serializable]
    public class CostItem
    {
        [Tooltip("要消耗的资源/物品类型。")]
        public ItemType ResourceItem;

        [Tooltip("消耗数量的计算公式。可以使用'动作上下文'中提供的任何变量。")]
        public string AmountFormula;
    }
} 