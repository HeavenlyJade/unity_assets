namespace MiGame.Core
{
    /// <summary>
    /// 定义加成的计算方式
    /// </summary>
    public enum BonusCalculationMethod
    {
        加法, // 将加成值直接累加
        乘法  // 将加成值（通常是百分比）相乘
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
        public BonusCalculationMethod Calculation = BonusCalculationMethod.加法;
    }
}
