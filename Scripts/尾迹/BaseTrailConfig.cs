using UnityEngine;
using System;
using System.Collections.Generic;
using MiGame.Items;
using MiGame.Utils;
using MiGame.Pet;

namespace MiGame.Trail
{
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
        public List<MiGame.Pet.携带效果> 携带效果;

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