namespace MiGame.Core
{
    /// <summary>
    /// 定义加成的计算方式
    /// </summary>
    public enum BonusCalculationMethod
    {
        单独相加, // 将加成值直接累加
        最终乘法,  // 计算的值在最后的结果向乘
        最终相加  // 将加成值（通常是百分比）相加
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
    }
}
