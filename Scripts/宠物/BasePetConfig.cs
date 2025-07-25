using UnityEngine;
using System.Collections.Generic;
using MiGame.Items;

namespace MiGame.Pet
{
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