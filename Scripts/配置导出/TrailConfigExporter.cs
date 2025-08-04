using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Trail;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class TrailConfigExporter : ConfigExporter<BaseTrailConfig>
    {
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/尾迹" };
        }
        public override string GetAssetPath()
        {
            return "Assets/GameConf/尾迹";
        }
        // 使用基类的反射导出方法
    }
} 