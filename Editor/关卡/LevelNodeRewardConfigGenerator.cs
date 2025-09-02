using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MiGame.Level;
using MiGame.Items;

namespace MiGame.Level.Editor
{
    /// <summary>
    /// 关卡节点奖励配置批量生成器
    /// 用于根据预设规则批量生成奖励节点配置
    /// </summary>
    public class LevelNodeRewardConfigGenerator : EditorWindow
    {
        private string 配置名称 = "批量生成的关卡奖励配置";
        private string 配置描述 = "通过批量生成器自动创建的关卡节点奖励配置";
        private string 保存路径 = "Assets/GameConf/游戏场景节点触发器/";
        private bool 覆盖现有文件 = false;
        private int 要生成的组数 = 1;

        // 预设的距离配置
        private readonly long[] 距离配置列表 = new long[]
        {
            
        5000, 10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 100000,
         150000, 200000, 300000, 400000, 
        500000, 600000
        };

        [MenuItem("Tools/关卡配置/初级关卡触发节点奖励配置")]
        public static void ShowWindow()
        {
            GetWindow<LevelNodeRewardConfigGenerator>("初级关卡奖励配置生成器");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("关卡节点奖励配置批量生成器", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 基础配置
            EditorGUILayout.LabelField("基础配置", EditorStyles.boldLabel);
            配置名称 = EditorGUILayout.TextField("配置名称", 配置名称);
            配置描述 = EditorGUILayout.TextField("配置描述", 配置描述);
            保存路径 = EditorGUILayout.TextField("保存路径", 保存路径);
            覆盖现有文件 = EditorGUILayout.Toggle("覆盖现有文件", 覆盖现有文件);
            要生成的组数 = EditorGUILayout.IntField("要生成的组数", 要生成的组数);

            EditorGUILayout.Space(10);

            // 规则说明
            EditorGUILayout.LabelField("生成规则", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "距离配置: 19个预设距离值\n" +
                "数量规则: 每16个为一个循环，依次为: 10,20,30,40,50,60,70,80,90,100,150,200,300,400,500,1000\n" +
                "距离循环: 每组距离增加600000，例如第2组从6003000开始，第3组从12003000开始", 
                MessageType.Info);

            EditorGUILayout.Space(10);

            // 预览信息
            EditorGUILayout.LabelField("预览信息", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"将生成 {距离配置列表.Length} 个奖励节点");
            EditorGUILayout.LabelField("总距离范围: " + 距离配置列表[0] + " - " + 距离配置列表[距离配置列表.Length - 1]);

            EditorGUILayout.Space(20);

            // 生成按钮
            if (GUILayout.Button("生成关卡节点奖励配置", GUILayout.Height(40)))
            {
                GenerateLevelNodeRewardConfig();
            }

            EditorGUILayout.Space(10);

            // 批量生成按钮
            if (GUILayout.Button("批量生成多个配置（所有组在一个文件下）", GUILayout.Height(30)))
            {
                GenerateMultipleConfigs();
            }
        }

        /// <summary>
        /// 生成单个关卡节点奖励配置
        /// </summary>
        private void GenerateLevelNodeRewardConfig()
        {
            try
            {
                // 创建配置资产
                var config = CreateInstance<LevelNodeRewardConfig>();
                config.配置名称 = 配置名称;
                config.配置描述 = 配置描述;

                // 生成节点列表
                config.节点列表 = GenerateNodeList();

                // 确保保存路径存在
                if (!AssetDatabase.IsValidFolder(保存路径))
                {
                    string parentPath = System.IO.Path.GetDirectoryName(保存路径);
                    string folderName = System.IO.Path.GetFileName(保存路径);
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }

                // 保存配置
                string fullPath = 保存路径 + 配置名称 + ".asset";
                if (AssetDatabase.LoadAssetAtPath<LevelNodeRewardConfig>(fullPath) != null && !覆盖现有文件)
                {
                    if (!EditorUtility.DisplayDialog("文件已存在", 
                        $"文件 {fullPath} 已存在，是否覆盖？", "覆盖", "取消"))
                    {
                        return;
                    }
                }

                AssetDatabase.CreateAsset(config, fullPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 选中生成的资产
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);

                EditorUtility.DisplayDialog("生成完成", 
                    $"关卡节点奖励配置已成功生成！\n保存路径: {fullPath}\n生成节点数量: {config.节点列表.Count}", 
                    "确定");

                Debug.Log($"关卡节点奖励配置生成完成: {fullPath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("生成失败", $"生成配置时发生错误:\n{e.Message}", "确定");
                Debug.LogError($"生成关卡节点奖励配置失败: {e}");
            }
        }

        /// <summary>
        /// 批量生成多个配置（所有组在一个文件下）
        /// </summary>
        private void GenerateMultipleConfigs()
        {
            try
            {
                if (要生成的组数 <= 0)
                {
                    EditorUtility.DisplayDialog("参数错误", "要生成的组数必须大于0", "确定");
                    return;
                }

                // 创建单个配置资产
                var config = CreateInstance<LevelNodeRewardConfig>();
                config.配置名称 = 配置名称;
                config.配置描述 = $"{配置描述} - 包含{要生成的组数}组配置";
                config.节点列表 = new List<LevelNodeReward>();

                // 生成所有组的节点列表
                for (int i = 0; i < 要生成的组数; i++)
                {
                    // 生成当前组的节点列表，距离增加600000 * i
                    long distanceOffset = 600000L * i;
                    var groupNodes = GenerateNodeListWithOffset(distanceOffset);
                    
                    // 将当前组的所有节点添加到总列表中
                    config.节点列表.AddRange(groupNodes);
                }

                // 确保保存路径存在
                if (!AssetDatabase.IsValidFolder(保存路径))
                {
                    string parentPath = System.IO.Path.GetDirectoryName(保存路径);
                    string folderName = System.IO.Path.GetFileName(保存路径);
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }

                // 保存配置到单个文件
                string fullPath = 保存路径 + 配置名称 + ".asset";
                if (AssetDatabase.LoadAssetAtPath<LevelNodeRewardConfig>(fullPath) != null && !覆盖现有文件)
                {
                    if (!EditorUtility.DisplayDialog("文件已存在", 
                        $"文件 {fullPath} 已存在，是否覆盖？", "覆盖", "取消"))
                    {
                        return;
                    }
                }

                AssetDatabase.CreateAsset(config, fullPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 选中生成的资产
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);

                EditorUtility.DisplayDialog("批量生成完成", 
                    $"成功生成包含{要生成的组数}组的配置文件！\n保存路径: {fullPath}\n总节点数量: {config.节点列表.Count}", 
                    "确定");

                Debug.Log($"批量生成关卡节点奖励配置完成，共生成{要生成的组数}组，总节点数: {config.节点列表.Count}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("批量生成失败", $"批量生成配置时发生错误:\n{e.Message}", "确定");
                Debug.LogError($"批量生成关卡节点奖励配置失败: {e}");
            }
        }

        /// <summary>
        /// 获取金币物品类型
        /// </summary>
        private ItemType GetGoldItemType()
        {
            // 尝试加载现有的金币ItemType
            string goldItemPath = "Assets/GameConf/物品/货币/金币.asset";
            ItemType goldItem = AssetDatabase.LoadAssetAtPath<ItemType>(goldItemPath);
            
            if (goldItem != null)
            {
                return goldItem;
            }
            
            Debug.LogWarning($"未能找到金币物品配置: {goldItemPath}，请在Inspector中手动选择");
            return null;
        }

        /// <summary>
        /// 生成节点列表
        /// </summary>
        private List<LevelNodeReward> GenerateNodeList()
        {
            return GenerateNodeList(距离配置列表, 0);
        }

        /// <summary>
        /// 根据指定的距离列表生成节点列表
        /// </summary>
        private List<LevelNodeReward> GenerateNodeList(long[] distances, int startIndex)
        {
            var nodeList = new List<LevelNodeReward>();

            for (int i = 0; i < distances.Length; i++)
            {
                var node = new LevelNodeReward();
                node.生成的距离配置 = distances[i];
                node.物品数量 = GetItemCountByIndex(startIndex + i);
                node.奖励条件 = $"距离达到 {distances[i]} 时获得奖励";
                
                // 设置物品类型为金币
                node.奖励类型 = RewardType.物品;
                node.物品类型 = GetGoldItemType();
                
                // 确保唯一ID存在
                node.EnsureUniqueID();
                
                nodeList.Add(node);
            }

            return nodeList;
        }

        /// <summary>
        /// 生成带偏移距离的节点列表
        /// </summary>
        private List<LevelNodeReward> GenerateNodeListWithOffset(long distanceOffset)
        {
            var nodeList = new List<LevelNodeReward>();

            for (int i = 0; i < 距离配置列表.Length; i++)
            {
                var node = new LevelNodeReward();
                // 距离 = 基础距离 + 偏移距离
                node.生成的距离配置 = 距离配置列表[i] + distanceOffset;
                node.物品数量 = GetItemCountByIndex(i);
                node.奖励条件 = $"距离达到 {node.生成的距离配置} 时获得奖励";
                
                // 设置物品类型为金币
                node.奖励类型 = RewardType.物品;
                node.物品类型 = GetGoldItemType();
                
                // 确保唯一ID存在
                node.EnsureUniqueID();
                
                nodeList.Add(node);
            }

            return nodeList;
        }

        /// <summary>
        /// 根据索引获取物品数量
        /// </summary>
        private int GetItemCountByIndex(int index)
        {
            // 每16个为一个循环
            int cycleIndex = index % 16;
            
            // 前10个：10, 20, 30, 40, 50, 60, 70, 80, 90, 100
            if (cycleIndex < 10) return (cycleIndex + 1) * 200;
            
            // 后6个：150, 200, 300, 400, 500, 1000
            switch (cycleIndex)
            {
                case 10: return 10000;
                case 11: return 11000;
                case 12: return 12000;
                case 13: return 20000;
                case 14: return 50000;
                case 15: return 100000;
                default: return 10;
            }
        }
    }
}
