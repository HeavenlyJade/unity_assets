using System.Text;
using UnityEditor;
using System.IO;
using MiGame.Skills;
using UnityEngine;

namespace MiGame.Editor.Exporter
{
    public class SkillConfigExporter : ConfigExporter<Skill>
    {
        public override string GetAssetPath()
        {
            return "Assets/GameConf/技能";
        }

        // The complex Export method is removed.
        // It will now use the base class's reflection-based Export method.
    }
} 