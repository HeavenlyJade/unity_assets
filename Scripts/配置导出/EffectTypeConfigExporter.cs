using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Items;

namespace MiGame.Editor.Exporter
{
    public class EffectTypeConfigExporter : ConfigExporter<EffectType>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/特效";
        }

        // The complex Export method is removed.
        // It will now use the base class's reflection-based Export method.
    }
} 