using UnityEditor;
using MiGame.Reward;

namespace MiGame.Editor.Exporter
{
    /// <summary>
    /// 奖励加成配置导出器
    /// </summary>
    public class RewardBonusConfigExporter : ConfigExporter<NewRewardConfig>
    {
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/奖励配置" };
        }
        
        public override string GetAssetPath()
        {
            return "Assets/GameConf/奖励配置";
        }
        
        // 使用基类的反射导出方法，无需自定义导出逻辑
    }
}
