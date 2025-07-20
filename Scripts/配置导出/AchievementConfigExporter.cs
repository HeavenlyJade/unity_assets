using MiGame.Achievement;

namespace MiGame.Editor.Exporter
{
    public class AchievementConfigExporter : ConfigExporter<AchievementConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/成就";
        }
    }
} 