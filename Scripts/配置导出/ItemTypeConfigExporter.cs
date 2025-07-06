using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Items;
using System.Linq;

namespace MiGame.Editor.Exporter
{
    public class ItemTypeConfigExporter : ConfigExporter<ItemType>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/物品";
        }

        // The complex Export method is removed.
        // It will now use the base class's reflection-based Export method.
    }
} 