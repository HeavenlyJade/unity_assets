using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MiGame.Shop;
using MiGame.Pet;

namespace MiGame.Tools
{
    /// <summary>
    /// 批量生成宠物UR品质商城配置
    /// </summary>
    public class BatchGeneratePetURShopConfig : EditorWindow
    {
        private string 输出路径 = "Assets/GameConf/商城/宠物商城";
        private bool 覆盖现有文件 = false;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/批量生成/宠物UR商城配置")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchGeneratePetURShopConfig>("宠物UR商城配置生成器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("宠物UR品质商城配置批量生成器", EditorStyles.boldLabel);
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
            if (GUILayout.Button("生成宠物UR商城配置", GUILayout.Height(30)))
            {
                GeneratePetURShopConfigs();
            }
            
            EditorGUILayout.Space();
            
            // 说明信息
            EditorGUILayout.HelpBox(
                "此工具将：\n" +
                "1. 扫描所有宠物配置，只选择UR品质的宠物\n" +
                "2. 为每个UR宠物生成对应的商城配置\n" +
                "3. 自动设置商品分类为'宠物'\n" +
                "4. 设置金币货币类型和永久一次限购", 
                MessageType.Info);
        }
        
        private void GeneratePetURShopConfigs()
        {
            try
            {
                // 确保输出目录存在
                if (!Directory.Exists(输出路径))
                {
                    Directory.CreateDirectory(输出路径);
                    AssetDatabase.Refresh();
                }
                
                // 获取所有宠物配置文件（排除伙伴配置）
                string[] petConfigPaths = AssetDatabase.FindAssets("t:PetConfig");
                List<PetConfig> urPetConfigs = new List<PetConfig>();
                
                foreach (string guid in petConfigPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    PetConfig config = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
                    if (config != null && config.稀有度 == 稀有度.UR)
                    {
                        urPetConfigs.Add(config);
                    }
                }
                
                if (urPetConfigs.Count == 0)
                {
                    EditorUtility.DisplayDialog("提示", "未找到任何UR品质的宠物配置文件！", "确定");
                    return;
                }
                
                int 成功数量 = 0;
                int 跳过数量 = 0;
                
                foreach (var petConfig in urPetConfigs)
                {
                    if (GenerateSinglePetURShopConfig(petConfig))
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
                    $"宠物UR商城配置生成完成！\n" +
                    $"成功: {成功数量} 个\n" +
                    $"跳过: {跳过数量} 个\n" +
                    $"输出目录: {输出路径}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成宠物UR商城配置时发生错误: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
            }
        }
        
        private bool GenerateSinglePetURShopConfig(PetConfig petConfig)
        {
            try
            {
                // 检查是否已存在
                string fileName = $"{petConfig.name}.asset";
                string filePath = Path.Combine(输出路径, fileName);
                
                if (!覆盖现有文件 && File.Exists(filePath))
                {
                    Debug.Log($"跳过已存在的文件: {fileName}");
                    return false;
                }
                
                // 创建商城配置
                var shopConfig = ScriptableObject.CreateInstance<ShopItemConfig>();
                
                // 设置基础信息
                shopConfig.商品名 = $"{petConfig.name}";
                shopConfig.商品描述 = $"购买获得 {petConfig.name} 宠物";
                shopConfig.商品分类 = ShopCategory.宠物;
                
                // 设置价格（UR品质使用金币）
                shopConfig.价格.货币类型 = CurrencyType.金币;
                shopConfig.价格.价格数量 = GetPriceForURPet().ToString("F0");
                
                // 设置限购配置
                shopConfig.限购配置.限购类型 = LimitType.永久一次;
                shopConfig.限购配置.限购次数 = 1;
                
                // 设置奖励内容
                var reward = new 商品奖励配置();
                reward.商品类型 = 商品类型.宠物;
                reward.宠物配置 = petConfig;
                reward.数量 = 1;
                shopConfig.获得物品.Add(reward);
                
                // 设置界面显示
                shopConfig.界面配置.排序权重 = 10; // UR品质给予最高排序权重
                shopConfig.界面配置.背景样式 = BackgroundStyle.UR;
                
                // 设置图标路径为宠物的头像资源（如果有的话）
                if (!string.IsNullOrEmpty(petConfig.头像资源))
                {
                    shopConfig.界面配置.图标路径 = petConfig.头像资源;
                }
                
                // 设置特殊标签（UR品质自动添加）
                shopConfig.界面配置.限定标签 = true;
                shopConfig.界面配置.推荐标签 = true;
                shopConfig.界面配置.热卖标签 = true;
                
                // 保存文件
                AssetDatabase.CreateAsset(shopConfig, filePath);
                
                Debug.Log($"成功生成宠物UR商城配置: {fileName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成 {petConfig.name} 的商城配置时发生错误: {e.Message}");
                return false;
            }
        }
        
        private decimal GetPriceForURPet()
        {
            // UR品质宠物使用金币，价格较高
            return 80000; // 8万金币
        }
    }
}
