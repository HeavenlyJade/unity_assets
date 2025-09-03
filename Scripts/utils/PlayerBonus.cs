using UnityEngine;

namespace MiGame.Core
{
    /// <summary>
    /// 定义加成的计算方式
    /// </summary>
    public enum BonusCalculationMethod
    {
        单独相加, // 将加成值直接累加
        最终乘法,  // 计算的值在最后的结果向乘
        最终相加,  // 将加成值（通常是百分比）相加
        基础相加, // 基础值阶段相加
        基础相乘,  // 基础值阶段相乘
        仅作引用  // 仅用于标记引用，不参与计算
    }

    /// <summary>
    /// 代表一个玩家加成的数据结构
    /// </summary>
    [System.Serializable]
    public class PlayerBonus
    {
        /// <summary>
        /// 加成的名称，会从VariableNames.json中读取
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 加成的计算方式
        /// </summary>
        public BonusCalculationMethod Calculation = BonusCalculationMethod.单独相加;

        /// <summary>
        /// 缩放倍率，用于对该加成的最终效果进行缩放（1 为不变）
        /// </summary>
        public float 缩放倍率 = 1f;

        /// <summary>
        /// 玩家效果字段，用于配置对应的等级效果cs的assets配置
        /// </summary>
        [Tooltip("等级效果配置，引用EffectLevelConfig资产")]
        public MiGame.Achievement.EffectLevelConfig 玩家效果字段;
    }
}
