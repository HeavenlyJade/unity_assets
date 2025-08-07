using MiGame.Lottery;

namespace MiGame.Editor.Exporter
{
    /// <summary>
    /// 抽奖配置导出器
    /// </summary>
    public class LotteryConfigExporter : ConfigExporter<LotteryConfig>
    {
        /// <summary>
        /// 获取资产文件路径
        /// </summary>
        /// <returns>资产文件所在的目录路径</returns>
        public override string GetAssetPath()
        {
            return "Assets/GameConf/抽奖";
        }
    }
}
