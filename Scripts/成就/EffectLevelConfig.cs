using UnityEngine;
using System;
using System.Collections.Generic;

namespace MiGame.Achievement
{
    /// <summary>
    /// 条件类型枚举
    /// </summary>
    public enum ConditionType
    {
        [Tooltip("公式计算")]
        公式,
        [Tooltip("固定数值")]
        固定
    }

    /// <summary>
    /// 目标类型枚举
    /// </summary>
    public enum TargetType
    {
        [Tooltip("玩家属性")]
        玩家属性,
        [Tooltip("玩家变量")]
        玩家变量
    }

    /// <summary>
    /// 效果等级配置类
    /// 用于配置天赋或技能在不同等级下的效果数值
    /// </summary>
    [CreateAssetMenu(fileName = "NewEffectLevelConfig", menuName = "配置/效果等级配置")]
    public class EffectLevelConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("配置的唯一ID (根据文件名自动生成)")]
        [ReadOnly]
        public string 配置名称;

        [Tooltip("配置描述")]
        [TextArea(2, 4)]
        public string 配置描述;

        [Header("作用目标配置")]
        [Tooltip("目标类型")]
        public TargetType 目标类型 = TargetType.玩家属性;

        [Tooltip("作用目标变量")]
        public string 作用目标变量 = "";

        [Header("等级效果配置")]
        [Tooltip("各等级的效果数值配置")]
        public List<LevelEffectData> 等级效果列表 = new List<LevelEffectData>();

        

        private void OnValidate()
        {
            // 自动将资产文件名同步到"配置名称"字段
            if (name != 配置名称)
            {
                配置名称 = name;
            }
        }
    }

    /// <summary>
    /// 等级效果数据结构
    /// </summary>
    [Serializable]
    public class LevelEffectData
    {
        [Tooltip("等级")]
        public int 等级 = 1;

        [Tooltip("条件类型")]
        public ConditionType 条件类型 = ConditionType.固定;

        [Tooltip("条件公式")]
        [TextArea(2, 3)]
        public string 条件公式 = "";

        [Tooltip("效果公式")]
        [TextArea(2, 3)]
        public string 效果公式 = "";

        [Tooltip("该等级的效果数值")]
        public double 效果数值 = 0.0;
    }

    
}
