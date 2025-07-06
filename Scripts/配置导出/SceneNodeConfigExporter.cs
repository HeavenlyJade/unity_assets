using System.Collections.Generic;
using System.IO;
using System.Text;
using MiGame.Scene;
using UnityEditor;

namespace MiGame.Editor.Exporter
{
    public class SceneNodeConfigExporter : ConfigExporter<SceneNodeConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/场景";
        }

        // The complex, overridden Export method has been removed.
        // It will now use the base class's reflection-based Export method,
        // which generates the correct Lua file structure.
    }
} 