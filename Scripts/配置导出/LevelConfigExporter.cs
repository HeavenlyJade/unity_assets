using MiGame.Data;

namespace MiGame.Editor.Exporter
{
    public class LevelConfigExporter : ConfigExporter<LevelConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/关卡";
        }
    }
} 