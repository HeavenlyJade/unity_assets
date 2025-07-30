using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Pet;
using System;

public class PetConfigBatchGenerator : EditorWindow
{
    [MenuItem("Tools/批量生成伙伴配置")]
    public static void ShowWindow()
    {
        GetWindow<PetConfigBatchGenerator>("批量生成伙伴配置");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("一键生成所有伙伴配置"))
        {
            GenerateAllPetConfigs();
        }
    }

    private static void GenerateAllPetConfigs()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/货币宠物.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到json文件: {jsonPath}");
            return;
        }
        string jsonText = File.ReadAllText(jsonPath);
        var petList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
        if (petList == null)
        {
            Debug.LogError("json解析失败");
            return;
        }
        string baseDir = "Assets/GameConf/伙伴";
        foreach (var pet in petList)
        {
            string quality = pet.品级;
            string folder = Path.Combine(baseDir, quality);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = baseDir;
                string newFolder = quality;
                AssetDatabase.CreateFolder(parent, newFolder);
            }
            string assetPath = Path.Combine(folder, pet.名称 + ".asset").Replace("\\", "/");
            PartnerConfig asset = AssetDatabase.LoadAssetAtPath<PartnerConfig>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PartnerConfig>();
            }
            asset.宠物名称 = pet.名称;
            asset.稀有度 = ParseQuality(pet.品级);
            asset.头像资源 = pet.图片;
            asset.模型资源 = pet.模型;
            asset.动画资源 = pet.动画;
            asset.携带效果 = new List<携带效果>
            {
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_金币获取",
                    效果数值 = pet.加成_百分比_金币获取
                },
                new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_训练加成",
                    效果数值 = pet.加成_百分比_训练加成
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
        Debug.Log("批量生成伙伴配置完成");
    }

    [Serializable]
    private class PetJsonData
    {
        public string 伙伴;
        public string 名称;
        public string 品级;
        public string 图片;
        public string 获取方式;
        public string 消耗迷你币;
        public string 消耗金币;
        public string 模型;
        public string 动画;
        public string 加成_百分比_金币获取;
        public string 加成_百分比_训练加成;
    }

    private static 稀有度 ParseQuality(string q)
    {
        switch (q)
        {
            case "UR": return 稀有度.SSR; // 如有UR枚举则改为UR
            case "SSR": return 稀有度.SSR;
            case "SR": return 稀有度.SR;
            case "R": return 稀有度.R;
            case "N": return 稀有度.N;
            default: return 稀有度.N;
        }
    }
}

// 支持JsonUtility解析数组的包装器
public static class JsonUtilityWrapper
{
    public static List<T> FromJsonArray<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
} 