using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Lottery;
using MiGame.Items;
using MiGame.Pet;
using System;

/// <summary>
/// 翅膀抽奖配置批量生成器
/// </summary>
public class GenerateWingLotteryConfigs : EditorWindow
{
    [MenuItem("Tools/批量生成/生成翅膀抽奖配置")]
    public static void ShowWindow()
    {
        GetWindow<GenerateWingLotteryConfigs>("生成翅膀抽奖配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("翅膀抽奖配置生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("初级翅膀抽奖配置", EditorStyles.boldLabel);
        if (GUILayout.Button("生成所有初级翅膀抽奖配置"))
        {
            GenerateAllPrimaryWingLotteryConfigs();
        }
        
        GUILayout.Space(15);
        GUILayout.Label("中级翅膀抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成所有中级翅膀抽奖配置"))
        {
            GenerateAllIntermediateWingLotteryConfigs();
        }
        
        GUILayout.Space(15);
        GUILayout.Label("高级翅膀抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成所有高级翅膀抽奖配置"))
        {
            GenerateAllAdvancedWingLotteryConfigs();
        }
        
        GUILayout.Space(15);
        GUILayout.Label("其他区域翅膀抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成商城翅膀抽奖配置"))
        {
            GenerateShopWingLotteryConfig();
        }
        
        if (GUILayout.Button("生成挂机翅膀抽奖配置"))
        {
            GenerateHangupWingLotteryConfig();
        }
    }

    /// <summary>
    /// 生成所有初级翅膀抽奖配置
    /// </summary>
    private static void GenerateAllPrimaryWingLotteryConfigs()
    {
        GenerateWingLotteryConfig("初级", "初级翅膀", 抽奖类型.初级翅膀, 级别.初级);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有初级翅膀抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成所有中级翅膀抽奖配置
    /// </summary>
    private static void GenerateAllIntermediateWingLotteryConfigs()
    {
        GenerateWingLotteryConfig("中级", "中级翅膀", 抽奖类型.中级翅膀, 级别.中级);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有中级翅膀抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成所有高级翅膀抽奖配置
    /// </summary>
    private static void GenerateAllAdvancedWingLotteryConfigs()
    {
        GenerateWingLotteryConfig("高级", "高级翅膀", 抽奖类型.高级翅膀, 级别.高级);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有高级翅膀抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成商城翅膀抽奖配置
    /// </summary>
    private static void GenerateShopWingLotteryConfig()
    {
        GenerateWingLotteryConfig("商城", "商城翅膀", 抽奖类型.初级翅膀, 级别.初级);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("商城翅膀抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成挂机翅膀抽奖配置
    /// </summary>
    private static void GenerateHangupWingLotteryConfig()
    {
        GenerateWingLotteryConfig("挂机", "挂机翅膀", 抽奖类型.初级翅膀, 级别.初级);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("挂机翅膀抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成指定所在区域的翅膀抽奖配置
    /// </summary>
    /// <param name="所在区域">所在区域名称</param>
    /// <param name="目录名">目标目录名</param>
    /// <param name="抽奖类型">抽奖类型</param>
    /// <param name="级别">抽奖级别</param>
    private static void GenerateWingLotteryConfig(string 所在区域, string 目录名, 抽奖类型 抽奖类型, 级别 级别)
    {
        // 读取翅膀JSON文件
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/翅膀.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到翅膀JSON文件: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        var wingList = JsonUtilityWrapper.FromJsonArray<WingJsonData>(jsonText);
        if (wingList == null)
        {
            Debug.LogError("翅膀JSON解析失败");
            return;
        }

        // 筛选指定所在区域的翅膀
        var targetWings = wingList.FindAll(wing => wing.所在区域 == 所在区域);
        if (targetWings.Count == 0)
        {
            Debug.LogWarning($"未找到所在区域为 '{所在区域}' 的翅膀");
            return;
        }

        // 确保目标目录存在
        string baseDir = $"Assets/GameConf/抽奖/{目录名}";
        if (!AssetDatabase.IsValidFolder(baseDir))
        {
            string parent = "Assets/GameConf/抽奖";
            string newFolder = 目录名;
            AssetDatabase.CreateFolder(parent, newFolder);
        }

        // 创建抽奖配置
        string configName = $"{目录名}";
        string assetPath = Path.Combine(baseDir, configName + ".asset").Replace("\\", "/");
        
        LotteryConfig lotteryConfig = AssetDatabase.LoadAssetAtPath<LotteryConfig>(assetPath);
        if (lotteryConfig == null)
        {
            lotteryConfig = ScriptableObject.CreateInstance<LotteryConfig>();
        }

        // 设置基本信息
        lotteryConfig.名字 = configName;
        lotteryConfig.配置名称 = configName;
        lotteryConfig.描述 = $"{目录名}抽奖池 - {所在区域}区域";
        lotteryConfig.抽奖类型 = 抽奖类型;
        lotteryConfig.级别 = 级别;
        lotteryConfig.是否启用 = true;
        lotteryConfig.冷却时间 = 0;
        lotteryConfig.每日次数限制 = -1;

        // 设置消耗配置
        lotteryConfig.单次消耗 = new 单次消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(所在区域, 1)
        };

        lotteryConfig.五连消耗 = new 五连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(所在区域, 5)
        };

        lotteryConfig.十连消耗 = new 十连消耗配置
        {
            消耗物品 = GetCurrencyItem("金币"),
            消耗数量 = GetConsumeAmount(所在区域, 10)
        };

        // 设置奖励池
        lotteryConfig.奖励池 = new List<奖励池配置>();
        
        foreach (var wing in targetWings)
        {
            var rewardConfig = new 奖励池配置
            {
                奖励类型 = 奖励类型.翅膀,
                翅膀配置 = GetWingConfig(wing.翅膀名称, 所在区域),
                数量 = 1,
                权重 = GetWeightByQuality(wing.品级)
            };
            
            lotteryConfig.奖励池.Add(rewardConfig);
        }

        // 保存配置
        EditorUtility.SetDirty(lotteryConfig);
        if (AssetDatabase.GetAssetPath(lotteryConfig) == "")
        {
            AssetDatabase.CreateAsset(lotteryConfig, assetPath);
        }
        
        Debug.Log($"成功生成抽奖配置: {configName}，包含 {targetWings.Count} 个翅膀");
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
    /// 获取翅膀配置引用
    /// </summary>
    /// <param name="wingName">翅膀名称</param>
    /// <param name="所在区域">所在区域</param>
    /// <returns>翅膀配置引用</returns>
    private static WingConfig GetWingConfig(string wingName, string 所在区域)
    {
        // 根据所在区域确定翅膀配置的搜索路径
        string[] searchPaths = 所在区域 switch
        {
            "初级" => new[] { "Assets/GameConf/翅膀/初级" },
            "中级" => new[] { "Assets/GameConf/翅膀/中级" },
            "高级" => new[] { "Assets/GameConf/翅膀/高级" },
            "商城" => new[] { "Assets/GameConf/翅膀/商城" },
            "挂机" => new[] { "Assets/GameConf/翅膀/挂机" },
            _ => new string[0]
        };

        foreach (string path in searchPaths)
        {
            string assetPath = Path.Combine(path, wingName + ".asset").Replace("\\", "/");
            var wingConfig = AssetDatabase.LoadAssetAtPath<WingConfig>(assetPath);
            if (wingConfig != null)
            {
                return wingConfig;
            }
        }

        Debug.LogWarning($"未找到翅膀配置: {wingName}");
        return null;
    }

    /// <summary>
    /// 根据品质获取权重
    /// </summary>
    /// <param name="quality">品质</param>
    /// <returns>权重值</returns>
    private static float GetWeightByQuality(string quality)
    {
        switch (quality)
        {
            case "UR": return 3f;
            case "SSR": return 8f;
            case "SR": return 20f;
            case "R": return 35f;
            case "N": return 34f;
            default: return 25f;
        }
    }

    /// <summary>
    /// 根据所在区域和连抽次数获取消耗数量
    /// </summary>
    /// <param name="所在区域">所在区域</param>
    /// <param name="抽奖次数">抽奖次数</param>
    /// <returns>消耗数量</returns>
    private static long GetConsumeAmount(string 所在区域, int 抽奖次数)
    {
        long baseAmount = 所在区域 switch
        {
            "高级" => 30000000000L,  // 300亿
            "中级" => 20000000000L,  // 200亿
            "初级" => 10000000000L,  // 100亿
            "商城" => 50000000000L,  // 500亿
            "挂机" => 80000000000L,  // 800亿
            _ => 10000000000L
        };

        return baseAmount * 抽奖次数;
    }

    /// <summary>
    /// 翅膀JSON数据结构
    /// </summary>
    [Serializable]
    private class WingJsonData
    {
        public string 翅膀名称;
        public string 品级;
        public string 所在区域;
        public string 加成_百分比_速度加成;
        public string 加成_百分比_加速度;
        public string 加成_百分比_金币加成;
        public long 金币价格;
        public int? 迷你币价格;
        public string 模型;
        public string 动画;
        public string 头像;
    }
}


