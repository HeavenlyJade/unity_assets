using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Pet;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class PartnerConfigExporter : ConfigExporter<PartnerConfig>
    {
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/伙伴" };
        }
        public override string GetAssetPath()
        {
            return null;
        }
        // 使用基类的反射导出方法
    }
} 