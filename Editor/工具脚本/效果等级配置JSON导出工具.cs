using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MiGame.Achievement;
using Newtonsoft.Json;

namespace MiGame.Editor.Tools
{
    /// <summary>
    /// 将 GameConf/效果等级配置 下的 EffectLevelConfig 的 等级效果列表 导出为 JSON。
    /// 输出位置：Assets/配置exel/效果等级配置_等级效果列表.json
    /// </summary>
    public static class 效果等级配置JSON导出工具
    {
        private const string AssetFolder = "Assets/GameConf/效果等级配置";
        private const string OutputJsonPath = "Assets/配置exel/效果等级配置_等级效果列表.json";

        [MenuItem("Tools/配置导出/效果等级配置导出")]
        public static void 导出()
        {
            var all = LoadAllEffectLevelConfigs();
            if (all.Count == 0)
            {
                Debug.LogWarning("未找到任何 效果等级配置 .asset，导出已跳过。");
                return;
            }

            var exportData = new Dictionary<string, List<LevelItemDTO>>();
            foreach (var cfg in all)
            {
                if (cfg == null) continue;
                var list = new List<LevelItemDTO>();
                if (cfg.等级效果列表 != null)
                {
                    foreach (var item in cfg.等级效果列表)
                    {
                        list.Add(new LevelItemDTO
                        {
                            等级 = item.等级,
                            条件类型 = item.条件类型.ToString(),
                            条件公式 = item.条件公式,
                            效果公式 = item.效果公式,
                            效果数值 = item.效果数值
                        });
                    }
                }
                exportData[cfg.name] = list;
            }

            // 使用 Newtonsoft.Json 正确序列化 Dictionary
            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);

            var fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputJsonPath));
            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, json, System.Text.Encoding.UTF8);
            Debug.Log($"效果等级配置 JSON 导出完成: {fullPath}");

            // 刷新资源数据库
            EditorApplication.delayCall += () => {
                if (!AssetDatabase.IsAssetImportWorkerProcess())
                {
                    AssetDatabase.Refresh();
                }
            };
        }

        private static List<EffectLevelConfig> LoadAllEffectLevelConfigs()
        {
            var guids = AssetDatabase.FindAssets("t:EffectLevelConfig", new[] { AssetFolder });
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<EffectLevelConfig>(path))
                .Where(x => x != null)
                .ToList();
        }

        [System.Serializable]
        private class LevelItemDTO
        {
            public int 等级;
            public string 条件类型;
            public string 条件公式;
            public string 效果公式;
            public double 效果数值;
        }
    }
}


