using MiGame.Data;

namespace MiGame.Editor.Exporter
{
    /// <summary>
    /// 导出 ActionCostConfig 配置到 Lua。
    /// </summary>
    public class ActionCostConfigExporter : ConfigExporter<ActionCostConfig>
    {
        /// <summary>
        /// 指定包含 ActionCostConfig.asset 文件的目录路径。
        /// </summary>
        /// <returns>资产文件所在的目录路径</returns>
        public override string GetAssetPath()
        {
            return "Assets/GameConf/成本配置";
        }
    }
}