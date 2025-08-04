using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Utils;

namespace MiGame.Trail
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
    /// 加成类型
    /// </summary>
    public enum 加成类型
    {
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
        [SerializeField]
        public 变量类型 变量类型 = 变量类型.玩家属性;

        [Tooltip("变量名称")]
        [SerializeField]
        public string 变量名称;

        [Tooltip("效果数值或公式")]
        [SerializeField]
        public string 效果数值;

        [Tooltip("加成类型")]
        [SerializeField]
        public 加成类型 加成类型 = 加成类型.玩家属性;

        [Tooltip("加成目标")]
        [SerializeField]
        public ItemType 物品目标;

        [Tooltip("目标变量")]
        [SerializeField]
        public string 目标变量;

        [Tooltip("作用类型")]
        [SerializeField]
        public string 作用类型 = "单独相加";
    }

    /// <summary>
    /// 尾迹基础配置类
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrail", menuName = "配置/尾迹")]
    public class BaseTrailConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("尾迹的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 名称;
        
        [Tooltip("尾迹描述")]
        [TextArea(2, 4)]
        public string 描述;
        
        [Tooltip("显示名称")]
        public string 显示名;
        
        [Tooltip("稀有度")]
        public 稀有度 稀有度 = 稀有度.N;

        [Header("携带效果")]
        [Tooltip("携带效果配置")]
        public List<携带效果> 携带效果;

        [Header("资源配置")]
        [Tooltip("图片资源路径")]
        public string 图片资源;

        protected virtual void OnValidate()
        {
            // 自动将资产文件名同步到"名称"字段
            if (name != 名称)
            {
                名称 = name;
            }
        }
    }
} 