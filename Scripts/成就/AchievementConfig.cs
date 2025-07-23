using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Achievement
{
    /// <summary>
    /// 成就类型枚举
    /// </summary>
    public enum AchievementType
    {
        普通成就,
        天赋成就
    }

    /// <summary>
    /// 解锁条件结构
    /// </summary>
    [Serializable]
    public class UnlockCondition
    {
        [Tooltip("条件类型")]
        public string 条件类型;
        
        [Tooltip("条件参数")]
        public string 条件参数;
        
        [Tooltip("目标数值")]
        public int 目标数值;
        
        [Tooltip("是否已完成")]
        public bool 已完成;
    }

    /// <summary>
    /// 解锁奖励结构
    /// </summary>
    [Serializable]
    public class UnlockReward
    {
        [Tooltip("奖励类型")]
        public string 奖励类型;
        
        [Tooltip("奖励ID")]
        public string 奖励ID;
        
        [Tooltip("奖励数量")]
        public int 奖励数量;
    }

    /// <summary>
    /// 升级条件结构
    /// </summary>
    [Serializable]
    public class UpgradeCondition
    {
        [Tooltip("消耗的物品")]
        public ItemType 消耗物品;
        
        [Tooltip("消耗数量（可以是数字或公式）")]
        public string 消耗数量;
    }

    /// <summary>
    /// 玩家变量类型枚举
    /// </summary>
    public enum PlayerVariableType
    {
        玩家变量,
        玩家属性
    }

    /// <summary>
    /// 等级效果结构
    /// </summary>
    [Serializable]
    public class LevelEffect
    {
        [Tooltip("效果类型")]
        public PlayerVariableType 效果类型;
        
        [Tooltip("效果字段名称")]
        public string 效果字段名称;
        
        [Tooltip("基础数值")]
        public float 基础数值;
        
        [Tooltip("效果数值（公式，随天赋等级变化）")]
        public string 效果数值;
        
        [Tooltip("效果描述")]
        public string 效果描述;
    }

    /// <summary>
    /// 成就配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "成就配置")]
    public class AchievementConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("成就的唯一ID和显示名称")]
        [ReadOnly]
        public string 名字;
        
        [Tooltip("排序（用于成就显示顺序）")]
        public int 排序;
        
        [Tooltip("区分普通成就或天赋成就")]
        public AchievementType 类型;
        
        [Tooltip("成就的基本描述")]
        [TextArea(2, 4)]
        public string 描述;
        
        [Tooltip("成就图标路径")]
        public string 图标;

        [Header("解锁相关")]
        [Tooltip("成就解锁的条件列表")]
        public List<UnlockCondition> 解锁条件;
        
        [Tooltip("解锁时发放的奖励")]
        public List<UnlockReward> 解锁奖励;

        [Header("天赋成就专用")]
        [Tooltip("天赋的最大等级（普通成就不设置此字段）")]
        public int 最大等级 = 1;
        
        [Tooltip("天赋升级的条件（消耗、前置等）")]
        public List<UpgradeCondition> 升级条件;
        
        [Tooltip("各等级的具体效果配置")]
        public List<LevelEffect> 等级效果;

        private void OnValidate()
        {
            // 自动将资产文件名同步到"名字"字段
            if (name != 名字)
            {
                名字 = name;
            }
        }
    }
} 