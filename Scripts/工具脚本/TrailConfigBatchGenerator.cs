using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Trail;
using System;

public class TrailConfigBatchGenerator : EditorWindow
{
    [MenuItem("Tools/批量生成尾迹配置")]
    public static void ShowWindow()
    {
        GetWindow<TrailConfigBatchGenerator>("批量生成尾迹配置");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("一键生成所有尾迹配置"))
        {
            GenerateAllTrailConfigs();
        }
    }

    private static void GenerateAllTrailConfigs()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/拖尾配置.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到json文件: {jsonPath}");
            return;
        }
        string jsonText = File.ReadAllText(jsonPath);
        var trailList = JsonUtilityWrapper.FromJsonArray<TrailJsonData>(jsonText);
        if (trailList == null)
        {
            Debug.LogError("json解析失败");
            return;
        }
        string baseDir = "Assets/GameConf/尾迹";
        foreach (var trail in trailList)
        {
            string quality = trail.品级;
            string folder = Path.Combine(baseDir, quality);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = baseDir;
                string newFolder = quality;
                AssetDatabase.CreateFolder(parent, newFolder);
            }
            string assetPath = Path.Combine(folder, trail.名称 + ".asset").Replace("\\", "/");
            BaseTrailConfig asset = AssetDatabase.LoadAssetAtPath<BaseTrailConfig>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BaseTrailConfig>();
            }
            asset.名称 = trail.名称;
            asset.显示名 = trail.名称;
            asset.稀有度 = ParseQuality(trail.品级);
            asset.图片资源 = trail.图片;
            asset.携带效果 = new List<携带效果>
            {
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_速度加成",
                    效果数值 = trail.加成_百分比_速度加成.ToString()
                },
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_金币加成",
                    效果数值 = trail.加成_百分比_金币加成.ToString()
                },
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_加速度",
                    效果数值 = trail.加成_百分比_加速度.ToString()
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
        Debug.Log("批量生成尾迹配置完成");
    }

    [Serializable]
    private class TrailJsonData
    {
        public string 名称;
        public string 品级;
        public string 图片;
        public string 节点路径;
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