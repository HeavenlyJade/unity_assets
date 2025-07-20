using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Pet;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class PetConfigExporter : ConfigExporter<PetConfig>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/宠物伙伴";
        }

        // 使用基类的反射导出方法
    }
} 