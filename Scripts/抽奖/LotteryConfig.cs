using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Utils;

namespace MiGame.Lottery
{
    /// <summary>
    /// 抽奖类型枚举
    /// </summary>
    public enum 抽奖类型
    {
        [Tooltip("普通抽奖")]
        普通抽奖,
        [Tooltip("高级抽奖")]
        高级抽奖,
        [Tooltip("限定抽奖")]
        限定抽奖,
        [Tooltip("活动抽奖")]
        活动抽奖
    }

    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum 奖励类型
    {
        [Tooltip("物品")]
        物品,
        [Tooltip("伙伴")]
        伙伴,
        [Tooltip("宠物")]
        宠物,
        [Tooltip("翅膀")]
        翅膀,
        [Tooltip("尾迹")]
        尾迹
    }

    /// <summary>
    /// 奖励池配置结构
    /// </summary>
    [Serializable]
    public class 奖励池配置
    {
        [Tooltip("奖励类型")]
        public 奖励类型 奖励类型 = 奖励类型.物品;
        
        [Tooltip("物品名称（对应奖励类型的名称）")]
        public string 物品名称;
        
        [Tooltip("奖励数量")]
        public int 数量 = 1;
        
        [Tooltip("权重（0-100，相当于概率0-100%）")]
        [Range(0, 100)]
        public int 权重 = 40;
    }

    /// <summary>
    /// 单次消耗配置结构
    /// </summary>
    [Serializable]
    public class 单次消耗配置
    {
        [Tooltip("消耗的物品")]
        public ItemType 消耗物品;
        
        [Tooltip("消耗数量")]
        public int 消耗数量 = 1;
    }

    /// <summary>
    /// 五连消耗配置结构
    /// </summary>
    [Serializable]
    public class 五连消耗配置
    {
        [Tooltip("消耗的物品")]
        public ItemType 消耗物品;
        
        [Tooltip("消耗数量")]
        public int 消耗数量 = 5;
    }

    /// <summary>
    /// 十连消耗配置结构
    /// </summary>
    [Serializable]
    public class 十连消耗配置
    {
        [Tooltip("消耗的物品")]
        public ItemType 消耗物品;
        
        [Tooltip("消耗数量")]
        public int 消耗数量 = 10;
    }

    /// <summary>
    /// 抽奖配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewLottery", menuName = "配置/抽奖配置")]
    public class LotteryConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("抽奖配置的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名字;
        
        [Tooltip("抽奖配置名称")]
        public string 配置名称;
        
        [Tooltip("抽奖配置描述")]
        [TextArea(2, 4)]
        public string 描述;
        
        [Tooltip("抽奖类型")]
        public 抽奖类型 抽奖类型 = 抽奖类型.普通抽奖;

        [Header("奖励池配置")]
        [Tooltip("抽奖奖励池列表")]
        public List<奖励池配置> 奖励池 = new List<奖励池配置>();

        [Header("单次抽奖消耗")]
        [Tooltip("单次抽奖的消耗配置")]
        public 单次消耗配置 单次消耗;

        [Header("五连抽奖消耗")]
        [Tooltip("五连抽奖的消耗配置")]
        public 五连消耗配置 五连消耗;

        [Header("十连抽奖消耗")]
        [Tooltip("十连抽奖的消耗配置")]
        public 十连消耗配置 十连消耗;

        [Header("其他设置")]
        [Tooltip("是否启用此抽奖配置")]
        public bool 是否启用 = true;
        
        [Tooltip("抽奖冷却时间（秒）")]
        public int 冷却时间 = 0;
        
        [Tooltip("每日抽奖次数限制")]
        public int 每日次数限制 = -1; // -1表示无限制

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
