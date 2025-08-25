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
        活动抽奖,
        [Tooltip("初级宠物抽奖")]
        初级宠物,
        [Tooltip("初级翅膀抽奖")]
        初级翅膀,
        [Tooltip("初级伙伴抽奖")]
        初级伙伴,
        [Tooltip("初级尾迹抽奖")]
        初级尾迹,
        [Tooltip("中级伙伴抽奖")]
        中级伙伴,
        [Tooltip("中级宠物抽奖")]
        中级宠物,
        [Tooltip("中级翅膀抽奖")]
        中级翅膀,
        [Tooltip("高级伙伴抽奖")]
        高级伙伴,
        [Tooltip("高级宠物抽奖")]
        高级宠物,
        [Tooltip("高级翅膀抽奖")]
        高级翅膀
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
    /// 级别枚举
    /// </summary>
    public enum 级别
    {
        [Tooltip("初级")]
        初级,
        [Tooltip("中级")]
        中级,
        [Tooltip("高级")]
        高级,
        [Tooltip("终极")]
        终极
    }

    /// <summary>
    /// 奖励池配置结构
    /// </summary>
    [Serializable]
    public class 奖励池配置
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
        
        [Tooltip("尾迹配置（奖励类型为尾迹时使用）")]
        public ScriptableObject 尾迹配置;
        
        [Tooltip("奖励数量")]
        public int 数量 = 1;
        
        [Tooltip("权重（支持小数点，数值越大概率越高）")]
        public float 权重 = 40f;
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
        public long 消耗数量 = 1;
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
        public long 消耗数量 = 5;
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
        public long 消耗数量 = 10;
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
        
        [Tooltip("抽奖级别")]
        public 级别 级别 = 级别.初级;

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
