using System;
using System.Collections.Generic;
using UnityEngine;
using MiGame.Items;
using MiGame.Pet;
using MiGame.Trail;
using MiGame.Lottery;

namespace MiGame.Shop
{
    /// <summary>
    /// 商城物品配置
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItemConfig", menuName = "配置/商品配置")]
    public class ShopItemConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("商品显示名称")]
        public string 商品名 = "新商品";
        
        [Tooltip("商品描述文本")]
        [TextArea(2, 5)]
        public string 商品描述 = "商品描述";
        
        [Tooltip("商品分类标签")]
        public ShopCategory 商品分类 = ShopCategory.宠物;
        
        [Header("价格配置")]
        public PriceConfig 价格 = new PriceConfig();
        
        [Header("购买限制")]
        public PurchaseLimitConfig 限购配置 = new PurchaseLimitConfig();
        
        [Header("奖励内容")]
        [Tooltip("获得物品列表")]
        public List<商品奖励配置> 获得物品 = new List<商品奖励配置>();
        
        [Tooltip("购买后执行的指令列表")]
        [TextArea(3, 8)]
        public List<string> 执行指令 = new List<string>();
        
        [Tooltip("随机奖池配置")]
        public LotteryConfig 奖池;
        
        [Header("界面显示")]
        public UIDisplayConfig 界面配置 = new UIDisplayConfig();
        
        [Header("特殊配置")]
        public SpecialPropertiesConfig 特殊属性 = new SpecialPropertiesConfig();
        
        private void OnValidate()
        {
            // 确保列表不为空
            if (获得物品 == null) 获得物品 = new List<商品奖励配置>();
            if (执行指令 == null) 执行指令 = new List<string>();
        }
    }
    
    /// <summary>
    /// 商品分类枚举
    /// </summary>
    public enum ShopCategory
    {
        宠物,
        伙伴,
        翅膀,
        尾迹,
        特权,
        金币,
        会员特权
    }
    
    /// <summary>
    /// 商品类型枚举
    /// </summary>
    public enum 商品类型
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
        尾迹,
        [Tooltip("玩家变量")]
        玩家变量,
        [Tooltip("玩家属性")]
        玩家属性
    }
    
    /// <summary>
    /// 商品奖励配置结构
    /// </summary>
    [Serializable]
    public class 商品奖励配置
    {
        [Tooltip("商品类型")]
        public 商品类型 商品类型 = 商品类型.物品;
        
        [Tooltip("商品名称（根据商品类型选择对应的配置）")]
        public ScriptableObject 商品名称;
        
        [Tooltip("变量名称（商品类型为玩家变量或玩家属性时使用）")]
        public string 变量名称 = "";
        
        [Tooltip("奖励数量")]
        public int 数量 = 1;
    }
    
    /// <summary>
    /// 价格配置
    /// </summary>
    [Serializable]
    public class PriceConfig
    {
        [Tooltip("货币类型")]
        public CurrencyType 货币类型 = CurrencyType.水晶;
        
        [Tooltip("价格数值")]
        public int 价格数量 = 100;
        
        [Tooltip("购买次数记录变量")]
        public string 变量键 = "购买次数";
        
        [Tooltip("广告模式")]
        public AdMode 广告模式 = AdMode.不可看广告;
        
        [Tooltip("广告可减免次数")]
        public int 广告次数 = 0;
    }
    
    /// <summary>
    /// 货币类型枚举
    /// </summary>
    public enum CurrencyType
    {
        水晶,
        金币,
        迷你币
    }
    
    /// <summary>
    /// 广告模式枚举
    /// </summary>
    public enum AdMode
    {
        不可看广告,
        可看广告,
        必须看广告
    }
    
    /// <summary>
    /// 购买限制配置
    /// </summary>
    [Serializable]
    public class PurchaseLimitConfig
    {
        [Tooltip("限购类型")]
        public LimitType 限购类型 = LimitType.无限制;
        
        [Tooltip("限购数量，0为无限制")]
        public int 限购次数 = 0;
        
        [Tooltip("重置时间点，如04:00")]
        public string 重置时间 = "04:00";
        
        [Tooltip("购买前置条件")]
        public ModifiersConfig 购买条件;
    }
    
    /// <summary>
    /// 限购类型枚举
    /// </summary>
    public enum LimitType
    {
        无限制,
        每日,
        每周,
        每月,
        永久一次
    }
    
    /// <summary>
    /// 界面显示配置
    /// </summary>
    [Serializable]
    public class UIDisplayConfig
    {
        [Tooltip("排序优先级，数值越大越靠前")]
        public int 排序权重 = 0;
        
        [Tooltip("是否显示热卖标签")]
        public bool 热卖标签 = false;
        
        [Tooltip("是否显示限定标签")]
        public bool 限定标签 = false;
        
        [Tooltip("是否显示推荐标签")]
        public bool 推荐标签 = false;
        
        [Tooltip("自定义图标路径")]
        public string 图标路径 = "";
        
        [Tooltip("图标右下角数量显示")]
        public int 图标数量 = 0;
        
        [Tooltip("背景样式")]
        public BackgroundStyle 背景样式 = BackgroundStyle.N;
    }
    
    /// <summary>
    /// 背景样式枚举
    /// </summary>
    public enum BackgroundStyle
    {
        N,
        R,
        SR,
        SSR,
        UR
    }
    
    /// <summary>
    /// 特殊属性配置
    /// </summary>
    [Serializable]
    public class SpecialPropertiesConfig
    {
        [Tooltip("关联迷你世界商品ID，0为无关联")]
        public int 迷你商品ID = 0;
        
        [Tooltip("时效配置")]
        public TimeLimitConfig 时效配置 = new TimeLimitConfig();
        
        [Tooltip("VIP需求")]
        public VIPRequirementConfig VIP需求 = new VIPRequirementConfig();
    }
    
    /// <summary>
    /// 时效配置
    /// </summary>
    [Serializable]
    public class TimeLimitConfig
    {
        [Tooltip("是否为限时商品")]
        public bool 是否限时 = false;
        
        [Tooltip("开始时间戳或时间字符串")]
        public string 开始时间 = "";
        
        [Tooltip("结束时间戳或时间字符串")]
        public string 结束时间 = "";
    }
    
    /// <summary>
    /// VIP需求配置
    /// </summary>
    [Serializable]
    public class VIPRequirementConfig
    {
        [Tooltip("是否需要VIP才能购买")]
        public bool 需要VIP = false;
        
        [Tooltip("所需VIP等级")]
        public int VIP等级 = 1;
    }
    
    /// <summary>
    /// 购买条件配置（Modifiers对象）
    /// </summary>
    [Serializable]
    public class ModifiersConfig
    {
        [Tooltip("条件类型")]
        public ConditionType 条件类型 = ConditionType.玩家等级;
        
        [Tooltip("条件值")]
        public string 条件值 = "1";
        
        [Tooltip("比较操作符")]
        public ComparisonOperator 比较操作符 = ComparisonOperator.大于等于;
    }
    
    /// <summary>
    /// 条件类型枚举
    /// </summary>
    public enum ConditionType
    {
        玩家等级,
        任务完成,
        道具拥有,
        时间限制,
        其他
    }
    
    /// <summary>
    /// 比较操作符枚举
    /// </summary>
    public enum ComparisonOperator
    {
        等于,
        大于,
        小于,
        大于等于,
        小于等于,
        不等于
    }
}
