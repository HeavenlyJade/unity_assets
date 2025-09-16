using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Lottery;
using MiGame.Pet;
using MiGame.Items;
using System;

/// <summary>
/// 宠物抽奖配置批量生成器（中级和高级）
/// </summary>
public class GenerateIntermediatePetLotteryConfigs : EditorWindow
{
    [MenuItem("Tools/批量生成/生成宠物抽奖配置")]
    public static void ShowWindow()
    {
        GetWindow<GenerateIntermediatePetLotteryConfigs>("生成宠物抽奖配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("宠物抽奖配置生成器（中级和高级）", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("中级宠物抽奖配置", EditorStyles.boldLabel);
        if (GUILayout.Button("生成所有中级宠物抽奖配置"))
        {
            GenerateAllIntermediatePetLotteryConfigs();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("仅生成中级高级抽奖配置"))
        {
            GenerateIntermediatePetLotteryConfig("中级高级");
        }
        
        if (GUILayout.Button("仅生成中级中级抽奖配置"))
        {
            GenerateIntermediatePetLotteryConfig("中级中级");
        }
        
        if (GUILayout.Button("仅生成中级初级抽奖配置"))
        {
            GenerateIntermediatePetLotteryConfig("中级初级");
        }
        
        GUILayout.Space(15);
        GUILayout.Label("高级宠物抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成所有高级宠物抽奖配置"))
        {
            GenerateAllAdvancedPetLotteryConfigs();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("仅生成高级高级抽奖配置"))
        {
            GenerateAdvancedPetLotteryConfig("高级高级");
        }
        
        if (GUILayout.Button("仅生成高级中级抽奖配置"))
        {
            GenerateAdvancedPetLotteryConfig("高级中级");
        }
        
        if (GUILayout.Button("仅生成高级初级抽奖配置"))
        {
            GenerateAdvancedPetLotteryConfig("高级初级");
        }
    }

    /// <summary>
    /// 生成所有中级宠物抽奖配置
    /// </summary>
    private static void GenerateAllIntermediatePetLotteryConfigs()
    {
        string[] areas = { "中级高级", "中级中级", "中级初级" };
        
        foreach (string area in areas)
        {
            GenerateIntermediatePetLotteryConfig(area);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有中级宠物抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成所有高级宠物抽奖配置
    /// </summary>
    private static void GenerateAllAdvancedPetLotteryConfigs()
    {
        string[] areas = { "高级高级", "高级中级", "高级初级" };
        
        foreach (string area in areas)
        {
            GenerateAdvancedPetLotteryConfig(area);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有高级宠物抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成指定抽奖区域的宠物抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    private static void GenerateIntermediatePetLotteryConfig(string 抽奖区域)
    {
        // 读取宠物JSON文件
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到宠物JSON文件: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        var petList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
        if (petList == null)
        {
            Debug.LogError("宠物JSON解析失败");
            return;
        }

        // 筛选指定抽奖区域的宠物
        var targetPets = petList.FindAll(pet => pet.抽奖区域 == 抽奖区域);
        if (targetPets.Count == 0)
        {
            Debug.LogWarning($"未找到抽奖区域为 '{抽奖区域}' 的宠物");
            return;
        }

        // 确保目标目录存在
        string baseDir = "Assets/GameConf/抽奖/中级宠物";
        if (!AssetDatabase.IsValidFolder(baseDir))
        {
            string parent = "Assets/GameConf/抽奖";
            string newFolder = "中级宠物";
            AssetDatabase.CreateFolder(parent, newFolder);
        }

        // 创建抽奖配置
        string configName = $"中级宠物{抽奖区域.Replace("中级", "")}";
        string assetPath = Path.Combine(baseDir, configName + ".asset").Replace("\\", "/");
        
        LotteryConfig lotteryConfig = AssetDatabase.LoadAssetAtPath<LotteryConfig>(assetPath);
        if (lotteryConfig == null)
        {
            lotteryConfig = ScriptableObject.CreateInstance<LotteryConfig>();
        }

        // 设置基本信息
        lotteryConfig.名字 = configName;
        lotteryConfig.配置名称 = configName;
        lotteryConfig.描述 = $"中级宠物抽奖池 - {抽奖区域}";
        lotteryConfig.抽奖类型 = 抽奖类型.中级宠物;
        lotteryConfig.级别 = 级别.中级;
        lotteryConfig.是否启用 = true;
        lotteryConfig.冷却时间 = 0;
        lotteryConfig.每日次数限制 = -1;

        // 设置消耗配置
        lotteryConfig.单次消耗 = new 单次消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 1)
        };

        lotteryConfig.五连消耗 = new 五连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 5)
        };

        lotteryConfig.十连消耗 = new 十连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 10)
        };

        // 设置奖励池
        lotteryConfig.奖励池 = new List<奖励池配置>();
        
        foreach (var pet in targetPets)
        {
            var rewardConfig = new 奖励池配置
            {
                奖励类型 = 奖励类型.宠物,
                宠物配置 = GetPetConfig(pet.宠物名称),
                数量 = 1,
                权重 = GetWeightByQuality(pet.品级)
            };
            
            lotteryConfig.奖励池.Add(rewardConfig);
        }

        // 保存配置
        EditorUtility.SetDirty(lotteryConfig);
        if (AssetDatabase.GetAssetPath(lotteryConfig) == "")
        {
            AssetDatabase.CreateAsset(lotteryConfig, assetPath);
        }
        
        Debug.Log($"成功生成抽奖配置: {configName}，包含 {targetPets.Count} 个宠物");
    }

    /// <summary>
    /// 生成指定抽奖区域的高级宠物抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    private static void GenerateAdvancedPetLotteryConfig(string 抽奖区域)
    {
        // 读取宠物JSON文件
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到宠物JSON文件: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        var petList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
        if (petList == null)
        {
            Debug.LogError("宠物JSON解析失败");
            return;
        }

        // 筛选指定抽奖区域的宠物
        var targetPets = petList.FindAll(pet => pet.抽奖区域 == 抽奖区域);
        if (targetPets.Count == 0)
        {
            Debug.LogWarning($"未找到抽奖区域为 '{抽奖区域}' 的宠物");
            return;
        }

        // 确保目标目录存在
        string baseDir = "Assets/GameConf/抽奖/高级宠物";
        if (!AssetDatabase.IsValidFolder(baseDir))
        {
            string parent = "Assets/GameConf/抽奖";
            string newFolder = "高级宠物";
            AssetDatabase.CreateFolder(parent, newFolder);
        }

        // 创建抽奖配置
        string configName = $"高级宠物{抽奖区域.Replace("高级", "")}";
        string assetPath = Path.Combine(baseDir, configName + ".asset").Replace("\\", "/");
        
        LotteryConfig lotteryConfig = AssetDatabase.LoadAssetAtPath<LotteryConfig>(assetPath);
        if (lotteryConfig == null)
        {
            lotteryConfig = ScriptableObject.CreateInstance<LotteryConfig>();
        }

        // 设置基本信息
        lotteryConfig.名字 = configName;
        lotteryConfig.配置名称 = configName;
        lotteryConfig.描述 = $"高级宠物抽奖池 - {抽奖区域}";
        lotteryConfig.抽奖类型 = 抽奖类型.高级宠物;
        lotteryConfig.级别 = 级别.高级;
        lotteryConfig.是否启用 = true;
        lotteryConfig.冷却时间 = 0;
        lotteryConfig.每日次数限制 = -1;

        // 设置消耗配置
        lotteryConfig.单次消耗 = new 单次消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 1)
        };

        lotteryConfig.五连消耗 = new 五连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 5)
        };

        lotteryConfig.十连消耗 = new 十连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(抽奖区域, 10)
        };

        // 设置奖励池
        lotteryConfig.奖励池 = new List<奖励池配置>();
        
        foreach (var pet in targetPets)
        {
            var rewardConfig = new 奖励池配置
            {
                奖励类型 = 奖励类型.宠物,
                宠物配置 = GetAdvancedPetConfig(pet.宠物名称),
                数量 = 1,
                权重 = GetWeightByQuality(pet.品级)
            };
            
            lotteryConfig.奖励池.Add(rewardConfig);
        }

        // 保存配置
        EditorUtility.SetDirty(lotteryConfig);
        if (AssetDatabase.GetAssetPath(lotteryConfig) == "")
        {
            AssetDatabase.CreateAsset(lotteryConfig, assetPath);
        }
        
        Debug.Log($"成功生成抽奖配置: {configName}，包含 {targetPets.Count} 个宠物");
    }

    /// <summary>
    /// 获取货币物品引用
    /// </summary>
    /// <param name="currencyName">货币名称</param>
    /// <returns>货币物品引用</returns>
    private static ItemType GetCurrencyItem(string currencyName)
    {
        string path = $"Assets/GameConf/物品/货币/{currencyName}.asset";
        return AssetDatabase.LoadAssetAtPath<ItemType>(path);
    }

    /// <summary>
    /// 获取宠物配置引用
    /// </summary>
    /// <param name="petName">宠物名称</param>
    /// <returns>宠物配置引用</returns>
    private static PetConfig GetPetConfig(string petName)
    {
        // 根据抽奖区域确定宠物配置的目录
        string[] searchPaths = {
            "Assets/GameConf/宠物/中级高级",
            "Assets/GameConf/宠物/中级中级", 
            "Assets/GameConf/宠物/中级初级"
        };

        foreach (string path in searchPaths)
        {
            string assetPath = Path.Combine(path, petName + ".asset").Replace("\\", "/");
            var petConfig = AssetDatabase.LoadAssetAtPath<PetConfig>(assetPath);
            if (petConfig != null)
            {
                return petConfig;
            }
        }

        Debug.LogWarning($"未找到宠物配置: {petName}");
        return null;
    }

    /// <summary>
    /// 获取高级宠物配置引用
    /// </summary>
    /// <param name="petName">宠物名称</param>
    /// <returns>宠物配置引用</returns>
    private static PetConfig GetAdvancedPetConfig(string petName)
    {
        // 根据抽奖区域确定宠物配置的目录
        string[] searchPaths = {
            "Assets/GameConf/宠物/高级高级",
            "Assets/GameConf/宠物/高级中级", 
            "Assets/GameConf/宠物/高级初级"
        };

        foreach (string path in searchPaths)
        {
            string assetPath = Path.Combine(path, petName + ".asset").Replace("\\", "/");
            var petConfig = AssetDatabase.LoadAssetAtPath<PetConfig>(assetPath);
            if (petConfig != null)
            {
                return petConfig;
            }
        }

        Debug.LogWarning($"未找到高级宠物配置: {petName}");
        return null;
    }

    /// <summary>
    /// 根据品质获取权重
    /// </summary>
    /// <param name="quality">品质</param>
    /// <returns>权重值</returns>
    private static int GetWeightByQuality(string quality)
    {
        switch (quality)
        {
            case "SSR": return 5;
            case "SR": return 15;
            case "R": return 30;
            case "N": return 50;
            default: return 25;
        }
    }

    /// <summary>
    /// 根据抽奖区域和连抽次数获取消耗数量
    /// </summary>
    /// <param name="抽奖区域">抽奖区域</param>
    /// <param name="抽奖次数">抽奖次数</param>
    /// <returns>消耗数量</returns>
    private static long GetConsumeAmount(string 抽奖区域, int 抽奖次数)
    {
        long baseAmount = 抽奖区域 switch
        {
            "高级高级" => 25000000000L,  // 250亿
            "高级中级" => 20000000000L,  // 200亿
            "高级初级" => 18000000000L,  // 180亿
            "中级高级" => 15000000000L,  // 150亿
            "中级中级" => 12000000000L,  // 120亿
            "中级初级" => 10000000000L,  // 100亿
            _ => 10000000000L
        };

        return baseAmount * 抽奖次数;
    }

    /// <summary>
    /// 宠物JSON数据结构
    /// </summary>
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
}

// 使用现有的JsonUtilityWrapper类，无需重复定义
