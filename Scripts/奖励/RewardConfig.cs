using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Utils;

namespace MiGame.Reward
{
    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum 奖励类型
    {
        [Tooltip("物品")]
        物品,
        [Tooltip("翅膀")]
        翅膀,
        [Tooltip("宠物")]
        宠物,
        [Tooltip("伙伴")]
        伙伴
    }

    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum 奖励类型枚举
    {
        [Tooltip("活动奖励")]
        活动奖励
    }

    /// <summary>
    /// 奖励物品结构
    /// </summary>
    [Serializable]
    public class 奖励物品
    {
        [Tooltip("奖励类型")]
        public 奖励类型 奖励类型 = 奖励类型.物品;
        
        [Tooltip("物品（奖励类型为物品时使用）")]
        public ItemType 物品;
        
        [Tooltip("翅膀配置（奖励类型为翅膀时使用）")]
        public ScriptableObject 翅膀配置;
        
        [Tooltip("宠物配置（奖励类型为宠物时使用）")]
        public ScriptableObject 宠物配置;
        
        [Tooltip("伙伴配置（奖励类型为伙伴时使用）")]
        public ScriptableObject 伙伴配置;
        
        [Tooltip("奖励数量")]
        public int 数量;
    }

    /// <summary>
    /// 奖励节点结构
    /// </summary>
    [Serializable]
    public class 奖励节点
    {
        [Tooltip("时间节点（秒为单位）")]
        public int 时间节点;
        
        [Tooltip("奖励物品")]
        public 奖励物品 奖励物品;
        
        [Tooltip("临取执行指令")]
        [TextArea(3, 6)]
        public string 临取执行指令;
    }

    /// <summary>
    /// 重置周期枚举
    /// </summary>
    public enum 重置周期
    {
        [Tooltip("每日")]
        每日,
        [Tooltip("每周")]
        每周,
        [Tooltip("永久")]
        永久,
        [Tooltip("临取全部")]
        临取全部
    }

    /// <summary>
    /// 奖励配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewOnlineReward", menuName = "奖励配置/在线奖励")]
    public class RewardConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("奖励配置名称")]
        public string 配置名称;
        
        [Tooltip("奖励配置描述")]
        [TextArea(2, 4)]
        public string 描述;
        
        [Tooltip("奖励类型")]
        public 奖励类型枚举 奖励类型 = 奖励类型枚举.活动奖励;
        
        [Header("重置设置")]
        [Tooltip("重置周期")]
        public 重置周期 重置周期 = 重置周期.每日;
        
        [Header("奖励列表")]
        [Tooltip("奖励节点列表")]
        public List<奖励节点> 奖励列表;
        
        protected virtual void OnValidate()
        {
            // 自动将资产文件名同步到"配置名称"字段
            if (name != 配置名称)
            {
                配置名称 = name;
            }
        }
    }
} 