using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Pet;
using System;
using System.Linq;

public class 宠物ConfigBatchGenerator : EditorWindow
{
    [MenuItem("Tools/批量生成宠物配置")]
    public static void ShowWindow()
    {
        GetWindow<宠物ConfigBatchGenerator>("批量生成宠物配置");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("一键生成所有宠物配置"))
        {
            GenerateAllPetConfigs();
        }
    }

    private static void GenerateAllPetConfigs()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物.json");
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
        string baseDir = "Assets/GameConf/宠物";
        foreach (var pet in petList)
        {
            if (string.IsNullOrEmpty(pet.宠物名称))
            {
                continue;
            }

            string quality = pet.品级;
            string folder = Path.Combine(baseDir, quality);
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(baseDir, quality);
            }

            string assetPath = Path.Combine(folder, pet.宠物名称 + ".asset").Replace("\\", "/");
            PetConfig asset = AssetDatabase.LoadAssetAtPath<PetConfig>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PetConfig>();
            }

            asset.宠物名称 = pet.宠物名称;
            asset.稀有度 = ParseQuality(pet.品级);
            asset.头像资源 = pet.图片资源;
            asset.模型资源 = pet.模型;
            asset.动画资源 = pet.动画;
            asset.获取方式 = !string.IsNullOrEmpty(pet.获取方式) ? pet.获取方式.Split(',').ToList() : new List<string>();

            asset.携带效果 = new List<携带效果>();
            if (!string.IsNullOrEmpty(pet.加成_百分比_训练加成))
            {
                asset.携带效果.Add(new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_训练加成",
                    效果数值 = pet.加成_百分比_训练加成
                });
            }

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
        Debug.Log("批量生成宠物配置完成");
    }

    [Serializable]
    private class PetJsonData
    {
        public string 宠物名称;
        public string 品级;
        public string 图片资源;
        public string 模型;
        public string 动画;
        public string 获取方式;
        public string 加成_百分比_训练加成;
    }

    private static 稀有度 ParseQuality(string q)
    {
        if (string.IsNullOrEmpty(q)) return 稀有度.N;
        switch (q.ToUpper())
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