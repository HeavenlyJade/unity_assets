using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Shop;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class ShopItemConfigExporter : ConfigExporter<ShopItemConfig>
    {
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/商城" };
        }
        
        public override string GetAssetPath()
        {
            return "Assets/GameConf/商城";
        }
        // 使用基类的反射导出方法
    }
}
