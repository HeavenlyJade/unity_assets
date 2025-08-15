using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MiGame.Shop;
using MiGame.Pet;
using System;
using System.Linq;

namespace MiGame.Tools
{
    /// <summary>
    /// 批量生成伙伴商城配置（抽奖区域为"商店区域"的伙伴）
    /// </summary>
    public class BatchGeneratePartnerShopConfig : EditorWindow
    {
        private string 输出路径 = "Assets/GameConf/商城/伙伴商城";
        private bool 覆盖现有文件 = false;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/批量生成/伙伴商城配置（商店区域）")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGeneratePartnerShopConfig>("伙伴商城配置生成器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("伙伴商城配置批量生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 输出路径设置
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出目录:", GUILayout.Width(80));
            输出路径 = EditorGUILayout.TextField(输出路径);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择输出目录", 输出路径, "");
                if (!string.IsNullOrEmpty(path))
                {
                    输出路径 = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 选项设置
            覆盖现有文件 = EditorGUILayout.Toggle("覆盖现有文件", 覆盖现有文件);
            
            EditorGUILayout.Space();
            
            // 操作按钮
            if (GUILayout.Button("生成伙伴商城配置", GUILayout.Height(30)))
            {
                GeneratePartnerShopConfigs();
            }
            
            EditorGUILayout.Space();
            
            // 说明信息
            EditorGUILayout.HelpBox(
                "此工具将：\n" +
                "1. 读取 Scripts/配置exel/伙伴.json 文件\n" +
                "2. 筛选抽奖区域为'商店区域'的伙伴\n" +
                "3. 为每个符合条件的伙伴生成对应的商城配置\n" +
                "4. 自动设置商品分类为'伙伴'\n" +
                "5. 根据伙伴品质设置相应的价格和显示样式", 
                MessageType.Info);
        }
        
        private void GeneratePartnerShopConfigs()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(输出路径))
                {
                    Directory.CreateDirectory(输出路径);
                    AssetDatabase.Refresh();
                }
                
                // 读取伙伴配置JSON文件
                string jsonPath = "Assets/Scripts/配置exel/伙伴.json";
                if (!File.Exists(jsonPath))
                {
                    EditorUtility.DisplayDialog("错误", $"未找到伙伴配置文件: {jsonPath}", "确定");
                    return;
                }
                
                string jsonContent = File.ReadAllText(jsonPath);
                var partnerList = JsonUtilityWrapper.FromJsonArray<PartnerJsonData>(jsonContent);
                
                if (partnerList == null || partnerList.Count == 0)
                {
                    EditorUtility.DisplayDialog("错误", "解析伙伴配置JSON失败", "确定");
                    return;
                }
                
                // 筛选抽奖区域为"商店区域"的伙伴
                var shopPartners = partnerList.Where(p => p.抽奖区域 == "商店区域").ToList();
                
                if (shopPartners.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到抽奖区域为'商店区域'的伙伴！", "确定");
                    return;
                }
                
                int 成功数量 = 0;
                int 跳过数量 = 0;
                
                foreach (var partnerData in shopPartners)
                {
                    if (GenerateSinglePartnerShopConfig(partnerData))
                    {
                        成功数量++;
                    }
                    else
                    {
                        跳过数量++;
                    }
                }
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("完成", 
                    $"伙伴商城配置生成完成！\n" +
                    $"成功: {成功数量} 个\n" +
                    $"跳过: {跳过数量} 个\n" +
                    $"输出目录: {输出路径}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成伙伴商城配置时发生错误: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
            }
        }
        
        private bool GenerateSinglePartnerShopConfig(PartnerJsonData partnerData)
        {
            try
            {
                // 检查是否已存在
                string fileName = $"{partnerData.名称}.asset";
                string filePath = Path.Combine(输出路径, fileName);
                
                if (!覆盖现有文件 && File.Exists(filePath))
                {
                    Debug.Log($"跳过已存在的文件: {fileName}");
                    return false;
                }
                
                // 查找对应的伙伴配置资源
                PartnerConfig partnerConfig = FindPartnerConfigByName(partnerData.名称);
                if (partnerConfig == null)
                {
                    Debug.LogWarning($"未找到伙伴配置资源: {partnerData.名称}，将跳过生成");
                    return false;
                }
                
                // 创建商城配置
                var shopConfig = ScriptableObject.CreateInstance<ShopItemConfig>();
                
                // 设置基础信息
                shopConfig.商品名 = partnerData.名称;
                shopConfig.商品描述 = $"购买获得 {partnerData.名称} 伙伴";
                shopConfig.商品分类 = ShopCategory.伙伴;
                
                // 设置价格（根据商店区域/品质）
                shopConfig.价格.货币类型 = CurrencyType.金币;
                shopConfig.价格.价格数量 = GetPriceByQuality(partnerData.商店区域);
                
                // 设置限购配置
                shopConfig.限购配置.限购类型 = LimitType.永久一次;
                shopConfig.限购配置.限购次数 = 1;
                
                // 设置奖励内容
                var reward = new 商品奖励配置();
                reward.商品类型 = 商品类型.伙伴;
                reward.商品名称 = partnerConfig; // 关联到对应的伙伴配置
                reward.数量 = 1;
                shopConfig.获得物品.Add(reward);
                
                // 设置界面显示
                shopConfig.界面配置.排序权重 = GetSortWeightByQuality(partnerData.商店区域);
                shopConfig.界面配置.背景样式 = GetBackgroundStyleByQuality(partnerData.商店区域);
                
                // 设置图标路径
                if (!string.IsNullOrEmpty(partnerData.图片))
                {
                    shopConfig.界面配置.图标路径 = partnerData.图片;
                }
                else if (!string.IsNullOrEmpty(partnerConfig.头像资源))
                {
                    // 如果JSON中没有图片路径，使用伙伴配置中的头像资源
                    shopConfig.界面配置.图标路径 = partnerConfig.头像资源;
                }
                
                // 设置特殊标签（根据品质）
                if (partnerData.商店区域 == "UR" || partnerData.商店区域 == "SSR")
                {
                    shopConfig.界面配置.限定标签 = true;
                    shopConfig.界面配置.推荐标签 = true;
                    shopConfig.界面配置.热卖标签 = true;
                }
                else if (partnerData.商店区域 == "SR")
                {
                    shopConfig.界面配置.推荐标签 = true;
                }
                
                // 保存文件
                AssetDatabase.CreateAsset(shopConfig, filePath);
                
                Debug.Log($"成功生成伙伴商城配置: {fileName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成 {partnerData.名称} 的商城配置时发生错误: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 根据伙伴名称查找对应的PartnerConfig资源
        /// </summary>
        private PartnerConfig FindPartnerConfigByName(string partnerName)
        {
            try
            {
                // 在所有伙伴配置目录中查找
                string[] partnerConfigPaths = AssetDatabase.FindAssets("t:PartnerConfig");
                
                foreach (string guid in partnerConfigPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    PartnerConfig config = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);
                    if (config != null && config.name == partnerName)
                    {
                        return config;
                    }
                }
                
                Debug.LogWarning($"未找到名为 '{partnerName}' 的伙伴配置资源");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"查找伙伴配置资源时发生错误: {e.Message}");
                return null;
            }
        }
        
        private int GetPriceByQuality(string quality)
        {
            switch (quality)
            {
                case "UR": return 100000; // 10万金币
                case "SSR": return 50000;  // 5万金币
                case "SR": return 20000;   // 2万金币
                case "R": return 8000;     // 8千金币
                case "N": return 3000;     // 3千金币
                default: return 10000;     // 默认1万金币
            }
        }
        
        private int GetSortWeightByQuality(string quality)
        {
            switch (quality)
            {
                case "UR": return 10;
                case "SSR": return 8;
                case "SR": return 6;
                case "R": return 4;
                case "N": return 2;
                default: return 5;
            }
        }
        
        private BackgroundStyle GetBackgroundStyleByQuality(string quality)
        {
            switch (quality)
            {
                case "UR": return BackgroundStyle.UR;
                case "SSR": return BackgroundStyle.SSR;
                case "SR": return BackgroundStyle.SR;
                case "R": return BackgroundStyle.R;
                case "N": return BackgroundStyle.N;
                default: return BackgroundStyle.N;
            }
        }
    }
    
    /// <summary>
    /// 伙伴配置JSON数据结构
    /// </summary>
    [Serializable]
    public class PartnerJsonData
    {
        public string 名称;
        public string 商店区域;
        public string 抽奖区域;
        public string 图片;
        public string 加成_百分比_金币获取;
        public string 加成_百分比_训练加成;
        public string 模型;
        public string 动画;
    }
    
    /// <summary>
    /// 支持JsonUtility解析数组的包装器
    /// </summary>
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
}

