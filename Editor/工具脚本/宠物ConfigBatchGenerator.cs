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
            string folder;
            
            // 根据抽奖区域生成文件夹结构
            if (!string.IsNullOrEmpty(pet.抽奖区域))
            {
                // 有抽奖区域：只按抽奖区域生成文件夹
                folder = Path.Combine(baseDir, pet.抽奖区域);
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder(baseDir, pet.抽奖区域);
                }
            }
            else
            {
                // 没有抽奖区域：按品级生成文件夹
                folder = Path.Combine(baseDir, quality);
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder(baseDir, quality);
                }
            }

            string assetPath = Path.Combine(folder, pet.宠物名称 + ".asset").Replace("\\", "/");
            PetConfig asset = AssetDatabase.LoadAssetAtPath<PetConfig>(assetPath);
            
            // 检查是否已存在相同的配置，避免重复导出
            if (asset != null)
            {
                // 检查配置是否相同，如果相同则跳过
                if (IsPetConfigSame(asset, pet))
                {
                    Debug.Log($"跳过已存在的宠物配置: {pet.宠物名称}");
                    continue;
                }
            }
            
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PetConfig>();
            }

            asset.宠物名称 = pet.宠物名称;
            asset.稀有度 = ParseQuality(pet.品级);
            asset.头像资源 = pet.图片资源;
            asset.模型资源 = pet.模型;
            asset.动画资源 = pet.动画;
            
            // 根据抽奖区域设置获取方式
            if (!string.IsNullOrEmpty(pet.抽奖区域))
            {
                asset.获取方式 = new List<string> { pet.抽奖区域 };
            }
            else
            {
                asset.获取方式 = new List<string>();
            }

            asset.携带效果 = new List<携带效果>();
            if (!string.IsNullOrEmpty(pet.加成_百分比_训练加成))
            {
                asset.携带效果.Add(new 携带效果
                {
                    变量类型 = 变量类型.玩家变量,
                    变量名称 = "加成_百分比_训练加成",
                    加成类型 = 加成类型.玩家变量,
                    目标变量 = "数据_固定值_战力值",
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

    private static bool IsPetConfigSame(PetConfig existing, PetJsonData newData)
    {
        if (existing.宠物名称 != newData.宠物名称) return false;
        if (existing.稀有度 != ParseQuality(newData.品级)) return false;
        if (existing.头像资源 != newData.图片资源) return false;
        if (existing.模型资源 != newData.模型) return false;
        if (existing.动画资源 != newData.动画) return false;
        
        // 检查获取方式
        var existingGetWay = existing.获取方式 != null && existing.获取方式.Count > 0 ? existing.获取方式[0] : "";
        var newGetWay = !string.IsNullOrEmpty(newData.抽奖区域) ? newData.抽奖区域 : "";
        if (existingGetWay != newGetWay) return false;
        
        // 检查携带效果
        if (existing.携带效果 != null && existing.携带效果.Count > 0)
        {
            var existingEffect = existing.携带效果[0];
            if (existingEffect.效果数值 != newData.加成_百分比_训练加成) return false;
        }
        else if (!string.IsNullOrEmpty(newData.加成_百分比_训练加成))
        {
            return false;
        }
        
        return true;
    }

    [Serializable]
    private class PetJsonData
    {
        public string 宠物名称;
        public string 抽奖区域;
        public string 品级;
        public string 图片;
        public string 图片资源;
        public string 模型;
        public string 动画;
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