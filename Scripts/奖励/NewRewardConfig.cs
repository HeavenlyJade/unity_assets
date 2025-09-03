using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Reward
{
    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum 新奖励类型
    {
        [Tooltip("活动奖励")]
        活动奖励,
        [Tooltip("累计充值")]
        累计充值
    }

    /// <summary>
    /// 重置周期枚举
    /// </summary>
    public enum 新重置周期
    {
        [Tooltip("每日")]
        每日,
        [Tooltip("每月")]
        每月,
        [Tooltip("每年")]
        每年,
        [Tooltip("永不重置")]
        永不重置
    }

    /// <summary>
    /// 计算方式枚举
    /// </summary>
    public enum 计算方式
    {
        [Tooltip("迷你币")]
        迷你币,
        [Tooltip("其它")]
        其它
    }

    /// <summary>
    /// 奖励物品结构
    /// </summary>
    [Serializable]
    public class 新奖励物品
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
        
        [Tooltip("星级（物品的品质等级）")]
        public int 星级 = 1;
    }

    /// <summary>
    /// 奖励列表项结构
    /// </summary>
    [Serializable]
    public class 奖励列表项
    {
        [Tooltip("描述（对该奖励条件的简短说明）")]
        [TextArea(2, 3)]
        public string 描述;
        
        [Tooltip("条件公式（用于判断是否满足领取条件，如：充值金额 >= 100）")]
        [TextArea(2, 4)]
        public string 条件公式;
        
        [Tooltip("消耗迷你币")]
        public int 消耗迷你币;
        
        [Tooltip("权重（用于计算奖励概率或优先级）")]
        public float 权重 = 1.0f;
        
        [Tooltip("奖励物品列表（可以领取多个不同的物品）")]
        public List<新奖励物品> 奖励物品列表 = new List<新奖励物品>();
    }

    /// <summary>
    /// 全新奖励配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewRewardConfig", menuName = "奖励配置/新奖励配置")]
    public class NewRewardConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("配置名称")]
        public string 配置名称;
        
        [Tooltip("描述")]
        [TextArea(2, 4)]
        public string 描述;
        
        [Tooltip("奖励类型")]
        public 新奖励类型 奖励类型 = 新奖励类型.活动奖励;
        
        [Tooltip("重置周期")]
        public 新重置周期 重置周期 = 新重置周期.每日;
        
        [Tooltip("计算方式")]
        public 计算方式 计算方式 = 计算方式.迷你币;
        
        [Header("奖励列表")]
        [Tooltip("奖励列表")]
        public List<奖励列表项> 奖励列表 = new List<奖励列表项>();
        
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
