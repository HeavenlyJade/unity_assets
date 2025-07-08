using MiGame.Data;

namespace MiGame.Editor.Exporter
{
    public class SimulatorTalentConfigExporter : ConfigExporter<SimulatorTalentConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/天赋";
        }
    }
} 