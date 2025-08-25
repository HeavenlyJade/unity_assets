using MiGame.Level;

namespace MiGame.Editor.Exporter
{
    /// <summary>
    /// 关卡节点奖励配置导出器
    /// 用于导出LevelNodeRewardConfig配置数据为Lua格式
    /// </summary>
    public class LevelNodeRewardConfigExporter : ConfigExporter<LevelNodeRewardConfig>
    {
        /// <summary>
        /// 获取资产文件路径
        /// </summary>
        /// <returns>资产文件所在的目录路径</returns>
        public override string GetAssetPath()
        {
            return "Assets/GameConf/游戏场景节点触发器";
        }

        /// <summary>
        /// 获取多个资产文件路径（如果需要从多个目录收集配置）
        /// </summary>
        /// <returns>资产文件所在的目录路径数组</returns>
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/游戏场景节点触发器" };
        }

        // 使用基类的反射导出方法，自动处理所有public字段的序列化
    }
}
