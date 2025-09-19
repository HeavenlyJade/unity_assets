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
    /// 批量生成翅膀商城配置（所在区域为"商城"的翅膀）
    /// </summary>
    public class BatchGenerateWingShopConfig : EditorWindow
    {
        private string 输出路径 = "Assets/GameConf/商城/翅膀商城";
        private bool 覆盖现有文件 = false;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/批量生成/翅膀商城配置（商城区域）")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGenerateWingShopConfig>("翅膀商城配置生成器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("翅膀商城配置批量生成器", EditorStyles.boldLabel);
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
            if (GUILayout.Button("生成翅膀商城配置", GUILayout.Height(30)))
            {
                GenerateWingShopConfigs();
            }
            
            EditorGUILayout.Space();
            
            // 说明信息
            EditorGUILayout.HelpBox(
                "此工具将：\n" +
                "1. 读取 Scripts/配置exel/翅膀.json 文件\n" +
                "2. 筛选所在区域为'商城'的翅膀\n" +
                "3. 为每个符合条件的翅膀生成对应的商城配置\n" +
                "4. 自动设置商品分类为'翅膀'\n" +
                "5. 根据翅膀品质设置相应的价格和显示样式\n" +
                "6. 设置无限制购买类型，限购1000次", 
                MessageType.Info);
        }
        
        private void GenerateWingShopConfigs()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(输出路径))
                {
                    Directory.CreateDirectory(输出路径);
                    AssetDatabase.Refresh();
                }
                
                // 读取翅膀配置JSON文件
                string jsonPath = "Assets/Scripts/配置exel/翅膀.json";
                if (!File.Exists(jsonPath))
                {
                    EditorUtility.DisplayDialog("错误", $"未找到翅膀配置文件: {jsonPath}", "确定");
                    return;
                }
                
                string jsonContent = File.ReadAllText(jsonPath);
                var wingList = JsonUtilityWrapper.FromJsonArray<WingJsonData>(jsonContent);
                
                if (wingList == null || wingList.Count == 0)
                {
                    EditorUtility.DisplayDialog("错误", "解析翅膀配置JSON失败", "确定");
                    return;
                }
                
                // 筛选所在区域为"商城"的翅膀
                var shopWings = wingList.Where(w => w.所在区域 == "商城").ToList();
                
                if (shopWings.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到所在区域为'商城'的翅膀！", "确定");
                    return;
                }
                
                int 成功数量 = 0;
                int 跳过数量 = 0;
                
                foreach (var wingData in shopWings)
                {
                    if (GenerateSingleWingShopConfig(wingData))
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
                    $"翅膀商城配置生成完成！\n" +
                    $"成功: {成功数量} 个\n" +
                    $"跳过: {跳过数量} 个\n" +
                    $"输出目录: {输出路径}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成翅膀商城配置时发生错误: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
            }
        }
        
        private bool GenerateSingleWingShopConfig(WingJsonData wingData)
        {
            try
            {
                // 检查是否已存在
                string fileName = $"{wingData.翅膀名称}.asset";
                string filePath = Path.Combine(输出路径, fileName);
                
                if (!覆盖现有文件 && File.Exists(filePath))
                {
                    Debug.Log($"跳过已存在的文件: {fileName}");
                    return false;
                }
                
                // 查找对应的翅膀配置资源
                WingConfig wingConfig = FindWingConfigByName(wingData.翅膀名称);
                if (wingConfig == null)
                {
                    Debug.LogWarning($"未找到翅膀配置资源: {wingData.翅膀名称}，将跳过生成");
                    return false;
                }
                
                // 创建商城配置
                var shopConfig = ScriptableObject.CreateInstance<ShopItemConfig>();
                
                // 设置基础信息
                shopConfig.商品名 = wingData.翅膀名称;
                shopConfig.商品描述 = $"购买获得 {wingData.翅膀名称} 翅膀";
                shopConfig.商品分类 = ShopCategory.翅膀;
                
                // 设置价格（根据JSON中的金币价格和迷你币价格）
                Debug.Log($"翅膀 {wingData.翅膀名称} - JSON金币价格: {wingData.金币价格}, 迷你币价格: {wingData.迷你币价格}");
                
                if (!string.IsNullOrEmpty(wingData.金币价格) && decimal.TryParse(wingData.金币价格, out decimal goldPrice) && goldPrice > 0)
                {
                    shopConfig.价格.货币类型 = CurrencyType.金币;
                    shopConfig.价格.价格数量 = wingData.金币价格; // 直接使用原始字符串，避免转换问题
                    Debug.Log($"设置金币价格: {wingData.金币价格}");
                }
                else
                {
                    // 如果JSON中没有金币价格，根据品质设置默认价格
                    decimal defaultPrice = GetPriceByQuality(wingData.品级);
                    shopConfig.价格.货币类型 = CurrencyType.金币;
                    shopConfig.价格.价格数量 = defaultPrice.ToString("F0"); // F0格式去掉小数部分
                    Debug.Log($"使用默认价格: {defaultPrice} (品质: {wingData.品级})");
                }
                
                // 设置迷你币价格（如果有）
                if (wingData.迷你币价格.HasValue && wingData.迷你币价格.Value > 0)
                {
                    shopConfig.价格.迷你币类型 = 迷你币类型.迷你币;
                    shopConfig.价格.迷你币数量 = (int)wingData.迷你币价格.Value;
                    Debug.Log($"设置迷你币价格: {wingData.迷你币价格.Value}");
                }
                else
                {
                    Debug.Log($"未设置迷你币价格");
                }
                
                // 设置限购配置（无限制，限购1000次）
                shopConfig.限购配置.限购类型 = LimitType.无限制;
                shopConfig.限购配置.限购次数 = 1000;
                
                // 设置奖励内容
                var reward = new 商品奖励配置();
                reward.商品类型 = 商品类型.翅膀;
                reward.翅膀配置 = wingConfig; // 关联到对应的翅膀配置
                reward.数量 = 1;
                shopConfig.获得物品.Add(reward);
                
                // 设置界面显示
                shopConfig.界面配置.排序权重 = GetSortWeightByQuality(wingData.品级);
                shopConfig.界面配置.背景样式 = GetBackgroundStyleByQuality(wingData.品级);
                
                // 设置图标路径
                if (!string.IsNullOrEmpty(wingData.头像))
                {
                    shopConfig.界面配置.图标路径 = wingData.头像;
                }
                else if (!string.IsNullOrEmpty(wingConfig.头像资源))
                {
                    // 如果JSON中没有头像路径，使用翅膀配置中的头像资源
                    shopConfig.界面配置.图标路径 = wingConfig.头像资源;
                }
                
                // 设置特殊标签（根据品质）
                if (wingData.品级 == "UR" || wingData.品级 == "SSR")
                {
                    shopConfig.界面配置.限定标签 = true;
                    shopConfig.界面配置.推荐标签 = true;
                    shopConfig.界面配置.热卖标签 = true;
                }
                else if (wingData.品级 == "SR")
                {
                    shopConfig.界面配置.推荐标签 = true;
                }
                
                // 保存文件
                AssetDatabase.CreateAsset(shopConfig, filePath);
                
                Debug.Log($"成功生成翅膀商城配置: {fileName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成 {wingData.翅膀名称} 的商城配置时发生错误: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 根据翅膀名称查找对应的WingConfig资源
        /// </summary>
        private WingConfig FindWingConfigByName(string wingName)
        {
            try
            {
                // 在所有翅膀配置目录中查找
                string[] wingConfigPaths = AssetDatabase.FindAssets("t:WingConfig");
                
                foreach (string guid in wingConfigPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    WingConfig config = AssetDatabase.LoadAssetAtPath<WingConfig>(path);
                    if (config != null && config.name == wingName)
                    {
                        return config;
                    }
                }
                
                Debug.LogWarning($"未找到名为 '{wingName}' 的翅膀配置资源");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"查找翅膀配置资源时发生错误: {e.Message}");
                return null;
            }
        }
        
        private decimal GetPriceByQuality(string quality)
        {
            switch (quality)
            {
                case "UR": return 50000000; // 5千万金币
                case "SSR": return 20000000; // 2千万金币
                case "SR": return 5000000;   // 5百万金币
                case "R": return 1000000;    // 1百万金币
                case "N": return 100000;     // 10万金币
                default: return 1000000;     // 默认1百万金币
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
    /// 翅膀配置JSON数据结构
    /// </summary>
    [Serializable]
    public class WingJsonData
    {
        public string 翅膀名称;
        public string 品级;
        public string 所在区域;
        public string 加成_百分比_速度加成;
        public string 加成_百分比_加速度;
        public string 加成_百分比_金币加成;
        public string 金币价格; // 改为string类型，因为数值太大超出long范围
        public float? 迷你币价格;
        public string 模型;
        public string 动画;
        public string 头像;
    }
    

}
