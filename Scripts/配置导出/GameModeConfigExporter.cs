using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Items;
using MiGame.Data;

namespace MiGame.Editor.Exporter
{
    public class GameModeConfigExporter : ConfigExporter<GameModeConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/玩法";
        }
    }
} 