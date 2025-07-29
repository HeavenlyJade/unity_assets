using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiGame.Items;

/// <summary>
/// 自动监控并生成一个包含所有 ItemType 资产名称的JSON清单。
/// </summary>
public class ItemNameManifestGenerator : AssetPostprocessor
{
    private const string ManifestPath = "Assets/GameConf/物品/ItemNames.json";

    /// <summary>
    /// 当任何资产发生变化（导入、移动、删除）后，此方法会被Unity自动调用。
    /// </summary>
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // 检查发生变化的资产中是否有我们关心的 ItemType
        // 这是一个优化，避免在不相关的资产（如贴图、模型）变化时也执行更新
        bool needsUpdate = false;
        foreach (string path in importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths))
        {
            if (path.EndsWith(".asset")) // 简单地检查.asset文件变化
            {
                needsUpdate = true;
                break;
            }
        }

        // 如果清单文件本身不存在，也需要生成一次
        if (!File.Exists(ManifestPath))
        {
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            GenerateManifest();
        }
    }

    /// <summary>
    /// 手动触发生成清单的菜单项。
    /// </summary>
    [MenuItem("工具/物品/手动生成物品名称清单")]
    public static void GenerateManifest()
    {
        // 1. 查找项目中的所有 ItemType 资产
        string[] guids = AssetDatabase.FindAssets("t:ItemType");
        List<string> itemNames = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => Path.GetFileNameWithoutExtension(path)) // 直接使用资产文件的名字
            .OrderBy(name => name)
            .ToList();

        // 2. 为了使用JsonUtility，需要一个包装类
        var wrapper = new ItemNameListWrapper { ItemNames = itemNames };
        string newJsonContent = JsonUtility.ToJson(wrapper, true);

        // 3. 检查文件内容是否有变化，只有在变动时才写入，避免不必要的刷新
        string oldJsonContent = "";
        if (File.Exists(ManifestPath))
        {
            oldJsonContent = File.ReadAllText(ManifestPath);
        }

        if (newJsonContent != oldJsonContent)
        {
            File.WriteAllText(ManifestPath, newJsonContent);
            Debug.Log($"物品名称清单已更新: {ManifestPath}");
        }
    }

    // JsonUtility 不能直接序列化根列表，所以需要一个包装类
    [System.Serializable]
    private class ItemNameListWrapper
    {
        public List<string> ItemNames;
    }
}