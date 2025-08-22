using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MiGame.Lottery;
using MiGame.Pet;
using MiGame.Items;
using System;

/// <summary>
/// 伙伴抽奖配置批量生成器
/// </summary>
public class GeneratePartnerLotteryConfigs : EditorWindow
{
    [MenuItem("Tools/批量生成/生成伙伴抽奖配置")]
    public static void ShowWindow()
    {
        GetWindow<GeneratePartnerLotteryConfigs>("生成伙伴抽奖配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("伙伴抽奖配置生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("初级伙伴抽奖配置", EditorStyles.boldLabel);
        if (GUILayout.Button("生成所有初级伙伴抽奖配置"))
        {
            GenerateAllPrimaryPartnerLotteryConfigs();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("仅生成初级初级抽奖配置"))
        {
            GeneratePrimaryPartnerLotteryConfig("初级初级");
        }
        
        if (GUILayout.Button("仅生成初级终极抽奖配置"))
        {
            GeneratePrimaryPartnerLotteryConfig("初级终极");
        }
        
        GUILayout.Space(15);
        GUILayout.Label("中级伙伴抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成所有中级伙伴抽奖配置"))
        {
            GenerateAllIntermediatePartnerLotteryConfigs();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("仅生成中级初级抽奖配置"))
        {
            GenerateIntermediatePartnerLotteryConfig("中级初级");
        }
        
        if (GUILayout.Button("仅生成中级终极抽奖配置"))
        {
            GenerateIntermediatePartnerLotteryConfig("中级终极");
        }
        
        GUILayout.Space(15);
        GUILayout.Label("高级伙伴抽奖配置", EditorStyles.boldLabel);
        
        if (GUILayout.Button("生成所有高级伙伴抽奖配置"))
        {
            GenerateAllAdvancedPartnerLotteryConfigs();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("仅生成高级初级抽奖配置"))
        {
            GenerateAdvancedPartnerLotteryConfig("高级初级");
        }
        
        if (GUILayout.Button("仅生成高级终极抽奖配置"))
        {
            GenerateAdvancedPartnerLotteryConfig("高级终极");
        }
    }

    /// <summary>
    /// 生成所有初级伙伴抽奖配置
    /// </summary>
    private static void GenerateAllPrimaryPartnerLotteryConfigs()
    {
        string[] areas = { "初级初级", "初级终极" };
        
        foreach (string area in areas)
        {
            GeneratePrimaryPartnerLotteryConfig(area);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有初级伙伴抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成所有中级伙伴抽奖配置
    /// </summary>
    private static void GenerateAllIntermediatePartnerLotteryConfigs()
    {
        string[] areas = { "中级初级", "中级终极" };
        
        foreach (string area in areas)
        {
            GenerateIntermediatePartnerLotteryConfig(area);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有中级伙伴抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成所有高级伙伴抽奖配置
    /// </summary>
    private static void GenerateAllAdvancedPartnerLotteryConfigs()
    {
        string[] areas = { "高级初级", "高级终极" };
        
        foreach (string area in areas)
        {
            GenerateAdvancedPartnerLotteryConfig(area);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有高级伙伴抽奖配置生成完成！");
    }

    /// <summary>
    /// 生成指定抽奖区域的初级伙伴抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    private static void GeneratePrimaryPartnerLotteryConfig(string 抽奖区域)
    {
        GeneratePartnerLotteryConfig(抽奖区域, "初级伙伴", 抽奖类型.初级伙伴, 级别.初级);
    }

    /// <summary>
    /// 生成指定抽奖区域的中级伙伴抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    private static void GenerateIntermediatePartnerLotteryConfig(string 抽奖区域)
    {
        GeneratePartnerLotteryConfig(抽奖区域, "中级伙伴", 抽奖类型.中级伙伴, 级别.中级);
    }

    /// <summary>
    /// 生成指定抽奖区域的高级伙伴抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    private static void GenerateAdvancedPartnerLotteryConfig(string 抽奖区域)
    {
        GeneratePartnerLotteryConfig(抽奖区域, "高级伙伴", 抽奖类型.高级伙伴, 级别.高级);
    }

    /// <summary>
    /// 生成指定抽奖区域的伙伴抽奖配置
    /// </summary>
    /// <param name="抽奖区域">抽奖区域名称</param>
    /// <param name="目录名">目标目录名</param>
    /// <param name="抽奖类型">抽奖类型</param>
    /// <param name="级别">抽奖级别</param>
    private static void GeneratePartnerLotteryConfig(string 抽奖区域, string 目录名, 抽奖类型 抽奖类型, 级别 级别)
    {
        // 读取伙伴JSON文件
        string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/伙伴.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"未找到伙伴JSON文件: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);
        var partnerList = JsonUtilityWrapper.FromJsonArray<PartnerJsonData>(jsonText);
        if (partnerList == null)
        {
            Debug.LogError("伙伴JSON解析失败");
            return;
        }

        // 筛选指定抽奖区域的伙伴
        var targetPartners = partnerList.FindAll(partner => partner.抽奖区域 == 抽奖区域);
        if (targetPartners.Count == 0)
        {
            Debug.LogWarning($"未找到抽奖区域为 '{抽奖区域}' 的伙伴");
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
        string configName = $"{目录名}{抽奖区域.Replace(目录名.Replace("伙伴", ""), "")}";
        string assetPath = Path.Combine(baseDir, configName + ".asset").Replace("\\", "/");
        
        LotteryConfig lotteryConfig = AssetDatabase.LoadAssetAtPath<LotteryConfig>(assetPath);
        if (lotteryConfig == null)
        {
            lotteryConfig = ScriptableObject.CreateInstance<LotteryConfig>();
        }

        // 设置基本信息
        lotteryConfig.名字 = configName;
        lotteryConfig.配置名称 = configName;
        lotteryConfig.描述 = $"{目录名}抽奖池 - {抽奖区域}";
        lotteryConfig.抽奖类型 = 抽奖类型;
        lotteryConfig.级别 = 级别;
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
        
        foreach (var partner in targetPartners)
        {
            var rewardConfig = new 奖励池配置
            {
                奖励类型 = 奖励类型.伙伴,
                伙伴配置 = GetPartnerConfig(partner.名称, 目录名),
                数量 = 1,
                权重 = GetWeightByQuality(partner.商店区域)
            };
            
            lotteryConfig.奖励池.Add(rewardConfig);
        }

        // 保存配置
        EditorUtility.SetDirty(lotteryConfig);
        if (AssetDatabase.GetAssetPath(lotteryConfig) == "")
        {
            AssetDatabase.CreateAsset(lotteryConfig, assetPath);
        }
        
        Debug.Log($"成功生成抽奖配置: {configName}，包含 {targetPartners.Count} 个伙伴");
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
    /// 获取伙伴配置引用
    /// </summary>
    /// <param name="partnerName">伙伴名称</param>
    /// <param name="目录名">目录名</param>
    /// <returns>伙伴配置引用</returns>
    private static ScriptableObject GetPartnerConfig(string partnerName, string 目录名)
    {
        // 根据目录名确定伙伴配置的搜索路径
        string[] searchPaths = 目录名 switch
        {
            "初级伙伴" => new[] { "Assets/GameConf/伙伴/初级初级", "Assets/GameConf/伙伴/初级终极" },
            "中级伙伴" => new[] { "Assets/GameConf/伙伴/中级初级", "Assets/GameConf/伙伴/中级终极" },
            "高级伙伴" => new[] { "Assets/GameConf/伙伴/高级初级", "Assets/GameConf/伙伴/高级终极" },
            _ => new string[0]
        };

        foreach (string path in searchPaths)
        {
            string assetPath = Path.Combine(path, partnerName + ".asset").Replace("\\", "/");
            var partnerConfig = AssetDatabase.LoadAssetAtPath<PartnerConfig>(assetPath);
            if (partnerConfig != null)
            {
                return partnerConfig;
            }
        }

        Debug.LogWarning($"未找到伙伴配置: {partnerName}");
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
            case "UR": return 3;
            case "SSR": return 8;
            case "SR": return 20;
            case "R": return 35;
            case "N": return 34;
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
            "高级终极" => 30000000000L,  // 300亿
            "高级初级" => 25000000000L,  // 250亿
            "中级终极" => 20000000000L,  // 200亿
            "中级初级" => 15000000000L,  // 150亿
            "初级终极" => 12000000000L,  // 120亿
            "初级初级" => 10000000000L,  // 100亿
            _ => 10000000000L
        };

        return baseAmount * 抽奖次数;
    }

    /// <summary>
    /// 伙伴JSON数据结构
    /// </summary>
    [Serializable]
    private class PartnerJsonData
    {
        public string 名称;
        public string 商店区域;
        public string 抽奖区域;
        public string 图片;
        public string 加成_百分比_金币获取;
        public string 加成_百分比_训练加成;
        public long 金币价格;
        public int? 迷你币价格;
        public string 模型;
        public string 动画;
    }
}





