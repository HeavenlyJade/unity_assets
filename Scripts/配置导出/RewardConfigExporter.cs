using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Reward;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class RewardConfigExporter : ConfigExporter<RewardConfig>
    {
        public override string[] GetAssetPaths()
        {
            return new[] { "Assets/GameConf/在线奖励" };
        }
        
        public override string GetAssetPath()
        {
            return "Assets/GameConf/在线奖励";
        }
        // 使用基类的反射导出方法
    }
} 