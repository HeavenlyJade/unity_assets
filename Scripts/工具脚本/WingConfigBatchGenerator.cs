using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Pet;
using MiGame.Items;
using System;

public class WingConfigBatchGenerator : EditorWindow
{
    [MenuItem("Tools/批量生成翅膀配置")]
    public static void ShowWindow()
    {
        GetWindow<WingConfigBatchGenerator>("批量生成翅膀配置");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("一键生成所有翅膀配置"))
        {
            GenerateAllWingConfigs();
        }
    }

    private static void GenerateAllWingConfigs()
    {
        // 1. 定义并加载金币配置文件
        string goldItemPath = "Assets/GameConf/物品/货币/金币.asset";
        ItemType goldItem = AssetDatabase.LoadAssetAtPath<ItemType>(goldItemPath);

        if (goldItem == null)
        {
            Debug.LogError($"未能加载金币配置文件，请检查路径是否正确: {goldItemPath}");
            return;
        }

        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/翅膀.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到json文件: {jsonPath}");
            return;
        }
        string jsonText = File.ReadAllText(jsonPath);
        var wingData = JsonUtility.FromJson<WingJsonRoot>(jsonText);
        if (wingData?.翅膀配置 == null)
        {
            Debug.LogError("json解析失败");
            return;
        }
        var wingList = wingData.翅膀配置;
        string baseDir = "Assets/GameConf/翅膀";
        foreach (var wing in wingList)
        {
            string quality = wing.品级;
            string folder = Path.Combine(baseDir, quality);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = baseDir;
                string newFolder = quality;
                AssetDatabase.CreateFolder(parent, newFolder);
            }
            string assetPath = Path.Combine(folder, wing.翅膀名称 + ".asset").Replace("\\", "/");
            WingConfig asset = AssetDatabase.LoadAssetAtPath<WingConfig>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WingConfig>();
            }
            asset.宠物名称 = wing.翅膀名称;
            asset.稀有度 = ParseQuality(wing.品级);
            asset.头像资源 = wing.头像 ?? "";
            asset.模型资源 = wing.模型 ?? "";
            asset.动画资源 = wing.动画 ?? "";
            asset.携带效果 = new List<携带效果>
            {
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_移动加成",
                    效果数值 = wing.加成_百分比_速度加成.ToString(),
                    加成类型 = 加成类型.玩家属性,
                    目标变量 = "数据_固定值_移动速度",
                    作用类型 = "单独相加"
                },
                                 new 携带效果
                 {
                     变量类型 = 变量类型.玩家变量,
                     变量名称 = "加成_百分比_金币加成",
                     效果数值 = wing.加成_百分比_金币加成.ToString(),
                     加成类型 = 加成类型.物品,
                     物品目标 = goldItem,
                     作用类型 = "单独相加"
                 },
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_加速度加成",
                    效果数值 = wing.加成_百分比_加速度.ToString(),
                    加成类型 = 加成类型.玩家属性,
                    目标变量 = "数据_固定值_加速度",
                    作用类型 = "单独相加"
                }
            };
            EditorUtility.SetDirty(asset);
            if (AssetDatabase.GetAssetPath(asset) == "")
            {
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }
        AssetDatabase.SaveAssets();
        // 使用延迟调用来避免在资源导入期间刷新
        EditorApplication.delayCall += () => {
            if (!AssetDatabase.IsAssetImportWorkerProcess())
            {
                AssetDatabase.Refresh();
            }
        };
        Debug.Log("批量生成翅膀配置完成");
    }

    [Serializable]
    private class WingJsonRoot
    {
        public WingJsonData[] 翅膀配置;
    }

    [Serializable]
    private class WingJsonData
    {
        public string 翅膀名称;
        public string 品级;
        public string 获取方式;
        public string 模型;
        public string 动画;
        public string 头像;
        public float 加成_百分比_速度加成;
        public float 加成_百分比_金币加成;
        public float 加成_百分比_加速度;
    }

    private static 稀有度 ParseQuality(string q)
    {
        switch (q)
        {
            case "UR": return 稀有度.UR;
            case "SSR": return 稀有度.SSR;
            case "SR": return 稀有度.SR;
            case "R": return 稀有度.R;
            case "N": return 稀有度.N;
            default: return 稀有度.N;
        }
    }
} 