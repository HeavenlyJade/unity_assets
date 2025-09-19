using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using MiGame.Lottery;
using MiGame.Pet;

namespace MiGame.Editor.Tools
{
    /// <summary>
    /// 蛋蛋抽奖奖池导出工具
    /// 导出字段：奖励类型、宠物配置、数量、权重
    /// </summary>
    public class 蛋蛋抽奖奖池导出工具 : EditorWindow
    {
        private string 导出路径 = "Assets/配置exel";
        private bool 包含保底配置 = true;
        private bool 只导出宠物奖励 = false;

        [MenuItem("Tools/抽奖配置/蛋蛋抽奖奖池导出工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<蛋蛋抽奖奖池导出工具>("蛋蛋抽奖奖池导出工具");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("蛋蛋抽奖奖池导出工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 导出路径设置
            GUILayout.Label("导出设置", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("导出路径:", GUILayout.Width(80));
            导出路径 = EditorGUILayout.TextField(导出路径);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择导出路径", 导出路径, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // 转换为相对于Assets的路径
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        导出路径 = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        导出路径 = selectedPath;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 导出选项
            GUILayout.Label("导出选项", EditorStyles.boldLabel);
            包含保底配置 = EditorGUILayout.Toggle("包含保底配置", 包含保底配置);
            只导出宠物奖励 = EditorGUILayout.Toggle("只导出宠物奖励", 只导出宠物奖励);

            GUILayout.Space(20);

            // 导出按钮
            if (GUILayout.Button("导出蛋蛋抽奖奖池", GUILayout.Height(30)))
            {
                导出蛋蛋抽奖奖池();
            }

            GUILayout.Space(10);

            // 说明信息
            EditorGUILayout.HelpBox(
                "导出字段：奖励类型、宠物配置、数量、权重\n" +
                "支持导出普通奖池和保底配置\n" +
                "可选择只导出宠物类型的奖励",
                MessageType.Info
            );
        }

        /// <summary>
        /// 导出蛋蛋抽奖奖池配置
        /// </summary>
        private void 导出蛋蛋抽奖奖池()
        {
            try
            {
                // 查找蛋蛋抽奖配置
                var lotteryConfigs = 查找蛋蛋抽奖配置();
                if (lotteryConfigs.Count == 0)
                {
                    EditorUtility.DisplayDialog("导出失败", "未找到蛋蛋抽奖配置！", "确定");
                    return;
                }

                // 创建导出数据
                var exportData = new 蛋蛋抽奖奖池数据
                {
                    配置名称 = "蛋蛋抽奖奖池",
                    导出时间 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    奖池列表 = new List<奖池数据>()
                };

                foreach (var config in lotteryConfigs)
                {
                    // 导出普通奖池
                    foreach (var reward in config.奖励池)
                    {
                        if (只导出宠物奖励 && reward.奖励类型 != 奖励类型.宠物)
                            continue;

                        var poolData = 创建奖池数据(reward);
                        if (poolData != null)
                        {
                            exportData.奖池列表.Add(poolData);
                        }
                    }

                    // 导出保底配置
                    if (包含保底配置)
                    {
                        foreach (var guarantee in config.保底配置列表)
                        {
                            if (只导出宠物奖励 && guarantee.奖励类型 != 奖励类型.宠物)
                                continue;

                            var poolData = 创建奖池数据(guarantee);
                            if (poolData != null)
                            {
                                exportData.奖池列表.Add(poolData);
                            }
                        }
                    }
                }

                // 导出为JSON
                导出为JSON(exportData);

                EditorUtility.DisplayDialog("导出成功", $"蛋蛋抽奖奖池已导出到：{导出路径}/蛋蛋抽奖奖池.json", "确定");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"导出过程中出现错误：\n{e.Message}", "确定");
                Debug.LogError($"导出蛋蛋抽奖奖池失败: {e}");
            }
        }

        /// <summary>
        /// 查找蛋蛋抽奖配置
        /// </summary>
        private List<LotteryConfig> 查找蛋蛋抽奖配置()
        {
            var configs = new List<LotteryConfig>();
            string[] guids = AssetDatabase.FindAssets("t:LotteryConfig", new[] { "Assets/GameConf/抽奖" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<LotteryConfig>(path);
                
                if (config != null && config.抽奖类型 == 抽奖类型.蛋蛋抽奖)
                {
                    configs.Add(config);
                }
            }

            return configs;
        }

        /// <summary>
        /// 创建奖池数据
        /// </summary>
        private 奖池数据 创建奖池数据(奖励池配置 reward)
        {
            var poolData = new 奖池数据
            {
                奖励类型 = reward.奖励类型.ToString(),
                数量 = reward.数量,
                权重 = reward.权重,
                宠物配置名称 = null
            };

            // 根据奖励类型设置对应的配置信息
            switch (reward.奖励类型)
            {
                case 奖励类型.宠物:
                    if (reward.宠物配置 != null)
                    {
                        poolData.宠物配置名称 = reward.宠物配置.name;
                    }
                    break;
                case 奖励类型.伙伴:
                    if (reward.伙伴配置 != null)
                    {
                        poolData.宠物配置名称 = reward.伙伴配置.name;
                    }
                    break;
                case 奖励类型.翅膀:
                    if (reward.翅膀配置 != null)
                    {
                        poolData.宠物配置名称 = reward.翅膀配置.name;
                    }
                    break;
                case 奖励类型.尾迹:
                    if (reward.尾迹配置 != null)
                    {
                        poolData.宠物配置名称 = reward.尾迹配置.name;
                    }
                    break;
                case 奖励类型.物品:
                    if (reward.物品 != null)
                    {
                        poolData.宠物配置名称 = reward.物品.name;
                    }
                    break;
            }

            return poolData;
        }

        /// <summary>
        /// 创建奖池数据（保底配置版本）
        /// </summary>
        private 奖池数据 创建奖池数据(保底配置 guarantee)
        {
            var poolData = new 奖池数据
            {
                奖励类型 = guarantee.奖励类型.ToString(),
                数量 = guarantee.数量,
                权重 = 0, // 保底配置没有权重
                宠物配置名称 = null
            };

            // 根据奖励类型设置对应的配置信息
            switch (guarantee.奖励类型)
            {
                case 奖励类型.宠物:
                    if (guarantee.宠物配置 != null)
                    {
                        poolData.宠物配置名称 = guarantee.宠物配置.name;
                    }
                    break;
                case 奖励类型.伙伴:
                    if (guarantee.伙伴配置 != null)
                    {
                        poolData.宠物配置名称 = guarantee.伙伴配置.name;
                    }
                    break;
                case 奖励类型.翅膀:
                    if (guarantee.翅膀配置 != null)
                    {
                        poolData.宠物配置名称 = guarantee.翅膀配置.name;
                    }
                    break;
                case 奖励类型.尾迹:
                    if (guarantee.尾迹配置 != null)
                    {
                        poolData.宠物配置名称 = guarantee.尾迹配置.name;
                    }
                    break;
                case 奖励类型.物品:
                    if (guarantee.物品 != null)
                    {
                        poolData.宠物配置名称 = guarantee.物品.name;
                    }
                    break;
            }

            return poolData;
        }

        /// <summary>
        /// 导出为JSON文件
        /// </summary>
        private void 导出为JSON(蛋蛋抽奖奖池数据 data)
        {
            // 确保导出目录存在
            if (!Directory.Exists(导出路径))
            {
                Directory.CreateDirectory(导出路径);
            }

            string json = JsonUtility.ToJson(data, true);
            string filePath = Path.Combine(导出路径, "蛋蛋抽奖奖池.json");
            File.WriteAllText(filePath, json, Encoding.UTF8);

            Debug.Log($"蛋蛋抽奖奖池已导出到: {filePath}");
        }

        /// <summary>
        /// 蛋蛋抽奖奖池数据结构
        /// </summary>
        [Serializable]
        public class 蛋蛋抽奖奖池数据
        {
            public string 配置名称;
            public string 导出时间;
            public List<奖池数据> 奖池列表;
        }

        /// <summary>
        /// 奖池数据结构
        /// </summary>
        [Serializable]
        public class 奖池数据
        {
            public string 奖励类型;
            public string 宠物配置名称;
            public int 数量;
            public float 权重;
        }
    }
}
