using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MiGame.Shop;
using MiGame.Pet;

namespace MiGame.Tools
{
    /// <summary>
    /// 批量生成翅膀UR品质商城配置
    /// </summary>
    public class BatchGenerateWingURShopConfig : EditorWindow
    {
        private string 输出路径 = "Assets/GameConf/商城/翅膀商城";
        private bool 覆盖现有文件 = false;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/批量生成/翅膀UR商城配置")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGenerateWingURShopConfig>("翅膀UR商城配置生成器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("翅膀UR品质商城配置批量生成器", EditorStyles.boldLabel);
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
            if (GUILayout.Button("生成翅膀UR商城配置", GUILayout.Height(30)))
            {
                GenerateWingURShopConfigs();
            }
            
            EditorGUILayout.Space();
            
            // 说明信息
            EditorGUILayout.HelpBox(
                "此工具将：\n" +
                "1. 扫描所有翅膀配置，只选择UR品质的翅膀\n" +
                "2. 为每个UR翅膀生成对应的商城配置\n" +
                "3. 自动设置商品分类为'翅膀'\n" +
                "4. 设置金币货币类型和永久一次限购", 
                MessageType.Info);
        }
        
        private void GenerateWingURShopConfigs()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(输出路径))
                {
                    Directory.CreateDirectory(输出路径);
                    AssetDatabase.Refresh();
                }
                
                // 获取所有翅膀配置文件
                string[] wingConfigPaths = AssetDatabase.FindAssets("t:WingConfig");
                List<WingConfig> urWingConfigs = new List<WingConfig>();
                
                foreach (string guid in wingConfigPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    WingConfig config = AssetDatabase.LoadAssetAtPath<WingConfig>(path);
                    if (config != null && config.稀有度 == 稀有度.UR)
                    {
                        urWingConfigs.Add(config);
                    }
                }
                
                if (urWingConfigs.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到任何UR品质的翅膀配置文件！", "确定");
                    return;
                }
                
                int 成功数量 = 0;
                int 跳过数量 = 0;
                
                foreach (var wingConfig in urWingConfigs)
                {
                    if (GenerateSingleWingURShopConfig(wingConfig))
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
                    $"翅膀UR商城配置生成完成！\n" +
                    $"成功: {成功数量} 个\n" +
                    $"跳过: {跳过数量} 个\n" +
                    $"输出目录: {输出路径}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成翅膀UR商城配置时发生错误: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
            }
        }
        
        private bool GenerateSingleWingURShopConfig(WingConfig wingConfig)
        {
            try
            {
                // 检查是否已存在
                string fileName = $"{wingConfig.name}.asset";
                string filePath = Path.Combine(输出路径, fileName);
                
                if (!覆盖现有文件 && File.Exists(filePath))
                {
                    Debug.Log($"跳过已存在的文件: {fileName}");
                    return false;
                }
                
                // 创建商城配置
                var shopConfig = ScriptableObject.CreateInstance<ShopItemConfig>();
                
                // 设置基础信息
                shopConfig.商品名 = $"{wingConfig.name}";
                shopConfig.商品描述 = $"购买获得 {wingConfig.name} 翅膀";
                shopConfig.商品分类 = ShopCategory.翅膀;
                
                // 设置价格（UR品质使用金币）
                shopConfig.价格.货币类型 = CurrencyType.金币;
                shopConfig.价格.价格数量 = GetPriceForURWing();
                
                // 设置限购配置
                shopConfig.限购配置.限购类型 = LimitType.永久一次;
                shopConfig.限购配置.限购次数 = 1;
                
                // 设置奖励内容
                var reward = new 商品奖励配置();
                reward.商品类型 = 商品类型.翅膀;
                reward.商品名称 = wingConfig;
                reward.数量 = 1;
                shopConfig.获得物品.Add(reward);
                
                // 设置界面显示
                shopConfig.界面配置.排序权重 = 10; // UR品质给予最高排序权重
                shopConfig.界面配置.背景样式 = BackgroundStyle.UR;
                
                // 设置图标路径为翅膀的头像资源
                if (!string.IsNullOrEmpty(wingConfig.头像资源))
                {
                    shopConfig.界面配置.图标路径 = wingConfig.头像资源;
                }
                
                // 设置特殊标签（UR品质自动添加）
                shopConfig.界面配置.限定标签 = true;
                shopConfig.界面配置.推荐标签 = true;
                shopConfig.界面配置.热卖标签 = true;
                
                // 保存文件
                AssetDatabase.CreateAsset(shopConfig, filePath);
                
                Debug.Log($"成功生成翅膀UR商城配置: {fileName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成 {wingConfig.name} 的商城配置时发生错误: {e.Message}");
                return false;
            }
        }
        
        private int GetPriceForURWing()
        {
            // UR品质翅膀使用金币，价格较高
            return 50000; // 5万金币
        }
    }
}
