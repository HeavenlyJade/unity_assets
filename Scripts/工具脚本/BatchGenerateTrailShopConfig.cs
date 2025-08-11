using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MiGame.Shop;
using MiGame.Trail;

namespace MiGame.Tools
{
    /// <summary>
    /// 批量生成尾迹商城配置
    /// </summary>
    public class BatchGenerateTrailShopConfig : EditorWindow
    {
        private string 输出路径 = "Assets/GameConf/商城/尾迹商城";
        private bool 覆盖现有文件 = false;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/批量生成/尾迹商城配置")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGenerateTrailShopConfig>("尾迹商城配置生成器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("尾迹商城配置批量生成器", EditorStyles.boldLabel);
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
            if (GUILayout.Button("生成尾迹商城配置", GUILayout.Height(30)))
            {
                GenerateTrailShopConfigs();
            }
            
            EditorGUILayout.Space();
            
            // 说明信息
            EditorGUILayout.HelpBox(
                "此工具将：\n" +
                "1. 扫描 GameConf/尾迹 目录下的所有尾迹配置\n" +
                "2. 为每个尾迹生成对应的商城配置\n" +
                "3. 自动设置商品分类为'尾迹'\n" +
                "4. 根据尾迹品质设置价格和标签", 
                MessageType.Info);
        }
        
        private void GenerateTrailShopConfigs()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(输出路径))
                {
                    Directory.CreateDirectory(输出路径);
                    AssetDatabase.Refresh();
                }
                
                // 获取所有尾迹配置文件
                string[] trailConfigPaths = AssetDatabase.FindAssets("t:BaseTrailConfig");
                List<BaseTrailConfig> trailConfigs = new List<BaseTrailConfig>();
                
                foreach (string guid in trailConfigPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    BaseTrailConfig config = AssetDatabase.LoadAssetAtPath<BaseTrailConfig>(path);
                    if (config != null)
                    {
                        trailConfigs.Add(config);
                    }
                }
                
                if (trailConfigs.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到任何尾迹配置文件！", "确定");
                    return;
                }
                
                int 成功数量 = 0;
                int 跳过数量 = 0;
                
                foreach (var trailConfig in trailConfigs)
                {
                    if (GenerateSingleTrailShopConfig(trailConfig))
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
                    $"尾迹商城配置生成完成！\n" +
                    $"成功: {成功数量} 个\n" +
                    $"跳过: {跳过数量} 个\n" +
                    $"输出目录: {输出路径}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成尾迹商城配置时发生错误: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
            }
        }
        
        private bool GenerateSingleTrailShopConfig(BaseTrailConfig trailConfig)
        {
            try
            {
                // 检查是否已存在
                string fileName = $"{trailConfig.name}.asset";
                string filePath = Path.Combine(输出路径, fileName);
                
                if (!覆盖现有文件 && File.Exists(filePath))
                {
                    Debug.Log($"跳过已存在的文件: {fileName}");
                    return false;
                }
                
                // 创建商城配置
                var shopConfig = ScriptableObject.CreateInstance<ShopItemConfig>();
                
                // 设置基础信息
                shopConfig.商品名 = $"{trailConfig.name}";
                shopConfig.商品描述 = $"购买获得 {trailConfig.name} 尾迹";
                shopConfig.商品分类 = ShopCategory.尾迹;
                
                // 设置价格（根据尾迹品质）
                shopConfig.价格.货币类型 = CurrencyType.金币;
                shopConfig.价格.价格数量 = GetPriceByQuality(trailConfig.稀有度);
                
                // 设置限购配置
                shopConfig.限购配置.限购类型 = LimitType.永久一次;
                shopConfig.限购配置.限购次数 = 1;
                
                // 设置奖励内容
                var reward = new 商品奖励配置();
                reward.商品类型 = 商品类型.尾迹;
                reward.商品名称 = trailConfig;
                reward.数量 = 1;
                shopConfig.获得物品.Add(reward);
                
                // 设置界面显示
                shopConfig.界面配置.排序权重 = GetSortWeightByQuality(trailConfig.稀有度);
                shopConfig.界面配置.背景样式 = GetBackgroundStyleByQuality(trailConfig.稀有度);
                
                // 设置特殊标签
                if (trailConfig.稀有度 == 稀有度.SSR || trailConfig.稀有度 == 稀有度.UR)
                {
                    shopConfig.界面配置.限定标签 = true;
                    shopConfig.界面配置.推荐标签 = true;
                }
                
                // 保存文件
                AssetDatabase.CreateAsset(shopConfig, filePath);
                
                Debug.Log($"成功生成尾迹商城配置: {fileName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成 {trailConfig.name} 的商城配置时发生错误: {e.Message}");
                return false;
            }
        }
        
        private int GetPriceByQuality(稀有度 quality)
        {
            switch (quality)
            {
                case 稀有度.N: return 100;
                case 稀有度.R: return 300;
                case 稀有度.SR: return 800;
                case 稀有度.SSR: return 2000;
                case 稀有度.UR: return 5000;
                default: return 100;
            }
        }
        
        private int GetSortWeightByQuality(稀有度 quality)
        {
            switch (quality)
            {
                case 稀有度.N: return 1;
                case 稀有度.R: return 2;
                case 稀有度.SR: return 3;
                case 稀有度.SSR: return 4;
                case 稀有度.UR: return 5;
                default: return 1;
            }
        }
        
        private BackgroundStyle GetBackgroundStyleByQuality(稀有度 quality)
        {
            switch (quality)
            {
                case 稀有度.N:
                    return BackgroundStyle.N;
                case 稀有度.R:
                    return BackgroundStyle.R;
                case 稀有度.SR:
                    return BackgroundStyle.SR;
                case 稀有度.SSR:
                    return BackgroundStyle.SSR;
                case 稀有度.UR:
                    return BackgroundStyle.UR;
                default:
                    return BackgroundStyle.N;
            }
        }
    }
}
