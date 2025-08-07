using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MiGame.Lottery;
using MiGame.Pet;
using MiGame.Items;

namespace MiGame.Lottery.Editor
{
    /// <summary>
    /// 批量生成初级抽奖池配置的编辑器工具
    /// </summary>
    public class LotteryConfigBatchGenerator : EditorWindow
    {
        private string outputPath = "Assets/GameConf/抽奖/";
        private bool generateAllPools = true;
        private bool generate100Pool = true;
        private bool generate10000Pool = true;
        private bool generate100000Pool = true;
        private bool generate400000Pool = true;
        
        // 选择要生成的抽奖类型
        private 抽奖类型 selectedLotteryType = 抽奖类型.初级宠物;
        private 奖励类型 selectedRewardType = 奖励类型.宠物;
        private 级别 selectedLevel = 级别.初级;

        [MenuItem("工具/抽奖配置/批量生成初级抽奖池")]
        public static void ShowWindow()
        {
            GetWindow<LotteryConfigBatchGenerator>("批量生成初级抽奖池");
        }

        private void OnGUI()
        {
            GUILayout.Label("批量生成初级抽奖池配置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 抽奖类型选择
            GUILayout.Label("抽奖类型设置", EditorStyles.boldLabel);
            selectedLotteryType = (抽奖类型)EditorGUILayout.EnumPopup("抽奖类型", selectedLotteryType);
            
            // 级别选择
            selectedLevel = (级别)EditorGUILayout.EnumPopup("抽奖级别", selectedLevel);
            
            // 根据抽奖类型自动设置奖励类型
            switch (selectedLotteryType)
            {
                case 抽奖类型.初级宠物:
                    selectedRewardType = 奖励类型.宠物;
                    break;
                case 抽奖类型.初级翅膀:
                    selectedRewardType = 奖励类型.翅膀;
                    break;
                case 抽奖类型.初级伙伴:
                    selectedRewardType = 奖励类型.伙伴;
                    break;
                case 抽奖类型.初级尾迹:
                    selectedRewardType = 奖励类型.尾迹;
                    break;
            }
            
            EditorGUILayout.LabelField("奖励类型", selectedRewardType.ToString());

            GUILayout.Space(10);

            // 输出路径设置
            GUILayout.Label("输出路径设置", EditorStyles.boldLabel);
            outputPath = EditorGUILayout.TextField("输出路径", outputPath);
            
            if (GUILayout.Button("选择输出路径"))
            {
                string path = EditorUtility.OpenFolderPanel("选择输出路径", outputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path.Replace(Application.dataPath, "Assets");
                }
            }

            GUILayout.Space(10);

            // 选择要生成的池子
            GUILayout.Label("选择要生成的抽奖池", EditorStyles.boldLabel);
            generateAllPools = EditorGUILayout.Toggle("生成所有池子", generateAllPools);
            
            if (!generateAllPools)
            {
                EditorGUI.indentLevel++;
                generate100Pool = EditorGUILayout.Toggle("100金币池", generate100Pool);
                generate10000Pool = EditorGUILayout.Toggle("10000金币池", generate10000Pool);
                generate100000Pool = EditorGUILayout.Toggle("100000金币池", generate100000Pool);
                generate400000Pool = EditorGUILayout.Toggle("400000金币池", generate400000Pool);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(20);

            // 生成按钮
            if (GUILayout.Button($"生成{selectedLotteryType}抽奖池配置", GUILayout.Height(30)))
            {
                GenerateLotteryConfigs();
            }

            GUILayout.Space(10);

            // 说明信息
            string rewardTypeName = GetRewardTypeName(selectedRewardType);
            EditorGUILayout.HelpBox(
                $"将生成以下{selectedLotteryType}抽奖池配置：\n" +
                $"级别：{selectedLevel}\n" +
                $"每个抽奖池包含4个{rewardTypeName}：\n" +
                "• 2个R级，权重45\n" +
                "• 1个SR级，权重8\n" +
                "• 1个SSR级，权重2\n" +
                "\n抽奖池：\n" +
                $"• {selectedLevel}普通 (100金币)\n" +
                $"• {selectedLevel}中级 (10000金币)\n" +
                $"• {selectedLevel}高级 (100000金币)\n" +
                $"• {selectedLevel}终极 (400000金币)",
                MessageType.Info
            );
        }

        private string GetRewardTypeName(奖励类型 rewardType)
        {
            switch (rewardType)
            {
                case 奖励类型.宠物: return "宠物";
                case 奖励类型.翅膀: return "翅膀";
                case 奖励类型.伙伴: return "伙伴";
                case 奖励类型.尾迹: return "尾迹";
                default: return "物品";
            }
        }

        private void GenerateLotteryConfigs()
        {
            try
            {
                // 创建金币ItemType
                ItemType goldItem = CreateGoldItemType();
                
                // 确保输出目录存在
                if (!AssetDatabase.IsValidFolder(outputPath))
                {
                    string[] folders = outputPath.Split('/');
                    string currentPath = "";
                    foreach (string folder in folders)
                    {
                        if (string.IsNullOrEmpty(folder)) continue;
                        string newPath = string.IsNullOrEmpty(currentPath) ? folder : currentPath + "/" + folder;
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, folder);
                        }
                        currentPath = newPath;
                    }
                }

                List<LotteryConfig> generatedConfigs = new List<LotteryConfig>();

                // 生成100金币池
                if (generateAllPools || generate100Pool)
                {
                    var config100 = CreateLotteryConfig($"{selectedLevel}普通", 100, goldItem);
                    generatedConfigs.Add(config100);
                }

                // 生成10000金币池
                if (generateAllPools || generate10000Pool)
                {
                    var config10000 = CreateLotteryConfig($"{selectedLevel}中级", 10000, goldItem);
                    generatedConfigs.Add(config10000);
                }

                // 生成100000金币池
                if (generateAllPools || generate100000Pool)
                {
                    var config100000 = CreateLotteryConfig($"{selectedLevel}高级", 100000, goldItem);
                    generatedConfigs.Add(config100000);
                }

                // 生成400000金币池
                if (generateAllPools || generate400000Pool)
                {
                    var config400000 = CreateLotteryConfig($"{selectedLevel}终极", 400000, goldItem);
                    generatedConfigs.Add(config400000);
                }

                // 保存所有配置
                foreach (var config in generatedConfigs)
                {
                    string assetPath = $"{outputPath}/{config.配置名称}.asset";
                    AssetDatabase.CreateAsset(config, assetPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("生成完成", 
                    $"成功生成 {generatedConfigs.Count} 个{selectedLotteryType}抽奖池配置！\n" +
                    $"输出路径: {outputPath}", "确定");

                // 在Project窗口中选中生成的文件夹
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("生成失败", $"生成过程中出现错误：\n{e.Message}", "确定");
                Debug.LogError($"生成{selectedLotteryType}抽奖池配置失败: {e}");
            }
        }

        private LotteryConfig CreateLotteryConfig(string configName, int cost, ItemType goldItem)
        {
            var config = CreateInstance<LotteryConfig>();
            
            // 基本信息
            config.配置名称 = configName;
            config.描述 = $"{selectedLotteryType}抽奖池 - {configName}";
            config.抽奖类型 = selectedLotteryType;
            config.级别 = selectedLevel;
            
            // 奖励池配置
            config.奖励池 = new List<奖励池配置>();
            
            // 添加4个奖励：2个R + 1个SR + 1个SSR
            // R奖励1
            var rewardPoolR1 = new 奖励池配置
            {
                奖励类型 = selectedRewardType,
                宠物配置 = null, // 需要手动设置具体的配置
                翅膀配置 = null,
                伙伴配置 = null,
                尾迹配置 = null,
                数量 = 1,
                权重 = 45
            };
            config.奖励池.Add(rewardPoolR1);
            
            // R奖励2
            var rewardPoolR2 = new 奖励池配置
            {
                奖励类型 = selectedRewardType,
                宠物配置 = null, // 需要手动设置具体的配置
                翅膀配置 = null,
                伙伴配置 = null,
                尾迹配置 = null,
                数量 = 1,
                权重 = 45
            };
            config.奖励池.Add(rewardPoolR2);
            
            // SR奖励
            var rewardPoolSR = new 奖励池配置
            {
                奖励类型 = selectedRewardType,
                宠物配置 = null, // 需要手动设置具体的配置
                翅膀配置 = null,
                伙伴配置 = null,
                尾迹配置 = null,
                数量 = 1,
                权重 = 8
            };
            config.奖励池.Add(rewardPoolSR);
            
            // SSR奖励
            var rewardPoolSSR = new 奖励池配置
            {
                奖励类型 = selectedRewardType,
                宠物配置 = null, // 需要手动设置具体的配置
                翅膀配置 = null,
                伙伴配置 = null,
                尾迹配置 = null,
                数量 = 1,
                权重 = 2
            };
            config.奖励池.Add(rewardPoolSSR);
            
            // 消耗配置
            config.单次消耗 = new 单次消耗配置
            {
                消耗物品 = goldItem,
                消耗数量 = cost
            };
            
            config.五连消耗 = new 五连消耗配置
            {
                消耗物品 = goldItem,
                消耗数量 = cost * 5
            };
            
            config.十连消耗 = new 十连消耗配置
            {
                消耗物品 = goldItem,
                消耗数量 = cost * 10
            };
            
            // 其他设置
            config.是否启用 = true;
            config.冷却时间 = 0;
            config.每日次数限制 = -1; // 无限制
            
            return config;
        }

        private ItemType CreateGoldItemType()
        {
            // 先尝试查找现有的金币ItemType
            string[] guids = AssetDatabase.FindAssets("t:ItemType");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemType item = AssetDatabase.LoadAssetAtPath<ItemType>(path);
                if (item != null && item.名字 == "金币")
                {
                    return item;
                }
            }
            
            // 如果没有找到，创建一个新的金币ItemType
            ItemType goldItem = CreateInstance<ItemType>();
            goldItem.名字 = "金币";
            goldItem.物品类型 = 物品种类.货币;
            goldItem.描述 = "游戏中的基础货币";
            goldItem.图标 = "";
            goldItem.品级 = ItemRank.普通;
            goldItem.战力 = 0;
            goldItem.强化倍率 = 1.0f;
            goldItem.强化材料增加倍率 = 1.0f;
            goldItem.最大强化等级 = 0;
            goldItem.图鉴完成奖励数量 = 0;
            goldItem.图鉴高级完成奖励数量 = 0;
            goldItem.售出价格 = 0;
            goldItem.在背包里显示 = true;
            goldItem.是否可堆叠 = true;
            goldItem.最大数量 = 999999999;
            goldItem.获得音效 = "";
            goldItem.取消获得物品 = false;
            goldItem.获得执行指令 = new List<string>();
            goldItem.使用执行指令 = new List<string>();
            goldItem.分解可得 = new List<ItemReward>();
            
            // 保存金币ItemType
            string itemPath = $"{outputPath}/金币.asset";
            AssetDatabase.CreateAsset(goldItem, itemPath);
            
            return goldItem;
        }
    }
}
