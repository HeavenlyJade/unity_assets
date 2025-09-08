using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Utils;

namespace MiGame.Pet
{
    /// <summary>
    /// 变量类型枚举
    /// </summary>
    public enum 变量类型
    {
        [Tooltip("玩家属性")]
        玩家属性,
        [Tooltip("玩家变量")]
        玩家变量
    }

    /// <summary>
    /// 消耗类型枚举
    /// </summary>
    public enum 消耗类型
    {
        [Tooltip("普通物品")]
        物品,
        [Tooltip("宠物材料")]
        宠物,
        [Tooltip("伙伴")]
        伙伴,
        [Tooltip("翅膀")]
        翅膀
    }

    /// <summary>
    /// 宠物类型枚举
    /// </summary>
    public enum 宠物类型
    {
        [Tooltip("宠物")]
        宠物,
        [Tooltip("伙伴")]
        伙伴
    }

    /// <summary>
    /// 稀有度枚举
    /// </summary>
    public enum 稀有度
    {
        [Tooltip("N级")]
        N,
        [Tooltip("R级")]
        R,
        [Tooltip("SR级")]
        SR,
        [Tooltip("SSR级")]
        SSR,
        [Tooltip("UR级")]
        UR
    }

    /// <summary>
    /// 属性配置结构
    /// </summary>
    [Serializable]
    public class 属性配置
    {
        [Tooltip("属性名称")]
        public string 属性名称;
        
        [Tooltip("属性数值")]
        public int 属性数值;
    }

    /// <summary>
    /// 成长率配置结构
    /// </summary>
    [Serializable]
    public class 成长率配置
    {
        [Tooltip("属性名称")]
        public string 属性名称;
        
        [Tooltip("成长公式")]
        public string 成长公式;
    }

    /// <summary>
    /// 属性加成结构
    /// </summary>
    [Serializable]
    public class 属性加成
    {
        [Tooltip("属性名称")]
        public string 属性名称;
        
        [Tooltip("加成数值或公式")]
        public string 加成数值;
    }

    /// <summary>
    /// 升星材料结构
    /// </summary>
    [Serializable]
    public class 升星材料
    {
        [Tooltip("消耗类型")]
        public 消耗类型 消耗类型 = 消耗类型.物品;
        
        [Tooltip("材料物品（消耗类型为物品时使用）")]
        public ItemType 材料物品;
        
        [Tooltip("消耗名称（消耗类型为宠物或伙伴时使用）")]
        public BasePetConfig 消耗名称;
        
        [Tooltip("需要数量")]
        public int 需要数量;
        
        [Tooltip("消耗星级要求（消耗类型为宠物或伙伴时使用）")]
        [Range(1, 10)]
        public int 消耗星级 = 1;
    }

    /// <summary>
    /// 升星消耗结构
    /// </summary>
    [Serializable]
    public class 升星消耗
    {
        [Tooltip("目标星级")]
        public int 星级;
        
        [Tooltip("消耗材料列表（物品或宠物）")]
        public List<升星材料> 消耗材料;
        
        [Tooltip("成功率（0-100）")]
        [Range(0, 100)]
        public float 成功率;
    }

    /// <summary>
    /// 等级效果结构
    /// </summary>
    [Serializable]
    public class 等级效果
    {
        [Tooltip("等级")]
        public int 等级;
        
        [Tooltip("解锁的技能名称")]
        public string 解锁技能;
        
        [Tooltip("属性加成列表")]
        public List<属性加成> 属性加成列表;
        
        [Tooltip("特殊效果描述")]
        public string 特殊效果;
    }

    /// <summary>
    /// 升星效果结构
    /// </summary>
    [Serializable]
    public class 升星效果
    {
        [Tooltip("星级")]
        public int 星级;
        
        [Tooltip("属性倍率加成列表")]
        public List<属性加成> 属性倍率;
        
        [Tooltip("新增技能名称")]
        public string 新增技能;
        
        [Tooltip("特殊能力描述")]
        public string 特殊能力;
        
        [Tooltip("升星所需材料")]
        public List<升星材料> 升星材料列表;
    }

    /// <summary>
    /// 效果触发条件结构
    /// </summary>
    [Serializable]
    public class 效果触发条件
    {
        [Tooltip("变量类型")]
        public 变量类型 变量类型 = 变量类型.玩家属性;
        
        [Tooltip("变量名称")]
        public string 变量名称;
        
        [Tooltip("效果数值或公式")]
        public string 效果数值;
    }

    /// <summary>
    /// 进化条件结构
    /// </summary>
    [Serializable]
    public class 进化条件
    {
        [Tooltip("需要等级")]
        public int 需要等级;
        
        [Tooltip("需要材料列表")]
        public List<升星材料> 需要材料;
    }



    /// <summary>
    /// 加成类型
    /// </summary>
    public enum 加成类型
    {
        空,
        物品,
        玩家变量,
        玩家属性
    }

    /// <summary>
    /// 携带效果结构
    /// </summary>
    [Serializable]
    public class 携带效果
    {
        [Tooltip("变量类型")]
        public 变量类型 变量类型 = 变量类型.玩家属性;

        [Tooltip("变量名称")]
        public string 变量名称;

        [Tooltip("效果数值或公式")]
        public string 效果数值;

        [Tooltip("加成类型")]
        public 加成类型 加成类型 = 加成类型.玩家属性;

        [Tooltip("加成目标")]
        public ItemType 物品目标;

        [Tooltip("目标变量")]
        [SerializeField]
        public string 目标变量;

        [Tooltip("作用类型")]
        [SerializeField]
        public string 作用类型 = "单独相加";
    }

    /// <summary>
    /// 宠物和伙伴通用的基础配置。
    /// 这个类不应该被直接创建实例。
    /// </summary>
    public abstract class BasePetConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("宠物的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 宠物名称;
        
        [Tooltip("宠物描述")]
        [TextArea(2, 4)]
        public string 宠物描述;
        
        [Tooltip("稀有度")]
        public 稀有度 稀有度 = 稀有度.N;

        [Header("等级系统")]
        [Tooltip("初始等级")]
        public int 初始等级 = 1;
        
        [Tooltip("最大等级")]
        public int 最大等级 = 100;
        
        [Tooltip("元素类型")]
        public string 元素类型;

        [Header("属性配置")]
        [Tooltip("基础属性列表")]
        public List<属性配置> 基础属性列表;
        
        [Tooltip("成长率配置列表")]
        public List<成长率配置> 成长率列表;

        [Header("升星系统")]
        [Tooltip("升星消耗配置")]
        public List<升星消耗> 升星消耗列表;

        [Header("携带效果")]
        [Tooltip("携带效果配置")]
        public List<携带效果> 携带效果;

        [Header("技能与进化")]
        [Tooltip("技能列表")]
        public List<string> 技能列表;
        
        [Tooltip("进化条件")]
        public 进化条件 进化条件 = new 进化条件();
        
        [Tooltip("进化后形态")]
        public string 进化后形态;

        [Header("获取方式")]
        [Tooltip("获取方式列表")]
        public List<string> 获取方式;

        [Header("资源配置")]
        [Tooltip("模型资源路径")]
        public string 模型资源;
        
        [Tooltip("头像资源路径")]
        public string 头像资源;
        
        [Tooltip("动画资源路径")]
        public string 动画资源;
        
        [Tooltip("音效资源路径")]
        public string 音效资源;
        
        [Tooltip("特殊标签")]
        public List<string> 特殊标签;

        protected virtual void OnValidate()
        {
            // 自动将资产文件名同步到"宠物名称"字段
            if (name != 宠物名称)
            {
                宠物名称 = name;
            }
        }
    }
} 