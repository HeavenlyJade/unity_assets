using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiGame.Pet;

namespace MiGame.Editor.Tools
{
    /// <summary>
    /// 宠物资源替换工具
    /// 用于批量更新宠物配置中的模型和动画资源
    /// </summary>
    public class PetResourceReplacer : EditorWindow
    {
        private string jsonFilePath = "Assets/Scripts/配置exel/宠物.json";
        private string petConfigFolder = "Assets/GameConf/宠物";
        private bool showProgress = true;
        private Vector2 scrollPosition;
        
        // 存储JSON数据的结构
        [System.Serializable]
        private class PetJsonData
        {
            public string 宠物名称;
            public string 模型;
            public string 动画;
        }
        
        [System.Serializable]
        private class PetJsonRoot
        {
            public PetJsonData[] pets;
        }

        [MenuItem("Tools/宠物资源替换工具")]
        public static void ShowWindow()
        {
            GetWindow<PetResourceReplacer>("宠物资源替换工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("宠物资源替换工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 文件路径配置
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("宠物JSON文件路径:", GUILayout.Width(120));
            jsonFilePath = EditorGUILayout.TextField(jsonFilePath);
            if (GUILayout.Button("选择文件", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("选择宠物JSON文件", "Assets", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    jsonFilePath = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("宠物配置文件夹:", GUILayout.Width(120));
            petConfigFolder = EditorGUILayout.TextField(petConfigFolder);
            if (GUILayout.Button("选择文件夹", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择宠物配置文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    petConfigFolder = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 操作按钮
            if (GUILayout.Button("读取JSON数据"))
            {
                ReadJsonData();
            }

            if (GUILayout.Button("预览变更"))
            {
                PreviewChanges();
            }

            if (GUILayout.Button("执行替换"))
            {
                ExecuteReplacement();
            }

            EditorGUILayout.Space();

            // 显示进度和结果
            if (showProgress)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DisplayProgress();
                EditorGUILayout.EndScrollView();
            }
        }

        private List<PetJsonData> jsonDataList = new List<PetJsonData>();
        private List<string> previewResults = new List<string>();

        /// <summary>
        /// 读取JSON数据
        /// </summary>
        private void ReadJsonData()
        {
            if (!File.Exists(jsonFilePath))
            {
                EditorUtility.DisplayDialog("错误", "JSON文件不存在: " + jsonFilePath, "确定");
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                // 包装JSON数组以便使用Unity的JsonUtility解析
                string wrappedJson = "{\"pets\":" + jsonContent + "}";
                var jsonRoot = JsonUtility.FromJson<PetJsonRoot>(wrappedJson);
                jsonDataList = jsonRoot.pets?.ToList() ?? new List<PetJsonData>();
                
                EditorUtility.DisplayDialog("成功", $"成功读取 {jsonDataList.Count} 条宠物数据", "确定");
                Debug.Log($"成功读取 {jsonDataList.Count} 条宠物数据");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", "解析JSON文件失败: " + e.Message, "确定");
                Debug.LogError("解析JSON文件失败: " + e.Message);
            }
        }

        /// <summary>
        /// 预览变更
        /// </summary>
        private void PreviewChanges()
        {
            if (jsonDataList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先读取JSON数据", "确定");
                return;
            }

            previewResults.Clear();
            int updatedCount = 0;
            int notFoundCount = 0;

            foreach (var jsonData in jsonDataList)
            {
                if (string.IsNullOrEmpty(jsonData.宠物名称))
                    continue;

                // 查找对应的宠物配置
                var petConfig = FindPetConfig(jsonData.宠物名称);
                if (petConfig != null)
                {
                    string oldModel = petConfig.模型资源;
                    string oldAnimation = petConfig.动画资源;
                    
                    previewResults.Add($"宠物: {jsonData.宠物名称}");
                    previewResults.Add($"  模型: {oldModel} -> {jsonData.模型}");
                    previewResults.Add($"  动画: {oldAnimation} -> {jsonData.动画}");
                    previewResults.Add("");
                    
                    updatedCount++;
                }
                else
                {
                    previewResults.Add($"未找到宠物配置: {jsonData.宠物名称}");
                    previewResults.Add("");
                    notFoundCount++;
                }
            }

            previewResults.Insert(0, $"预览结果: 找到 {updatedCount} 个配置，未找到 {notFoundCount} 个");
            previewResults.Insert(1, "");
        }

        /// <summary>
        /// 执行替换
        /// </summary>
        private void ExecuteReplacement()
        {
            if (jsonDataList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先读取JSON数据", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认", "确定要执行资源替换吗？此操作不可撤销。", "确定", "取消"))
                return;

            int updatedCount = 0;
            int errorCount = 0;

            // 记录撤销操作
            Undo.RecordObjects(GetAllPetConfigs().ToArray(), "宠物资源替换");

            foreach (var jsonData in jsonDataList)
            {
                if (string.IsNullOrEmpty(jsonData.宠物名称))
                    continue;

                var petConfig = FindPetConfig(jsonData.宠物名称);
                if (petConfig != null)
                {
                    try
                    {
                        petConfig.模型资源 = jsonData.模型;
                        petConfig.动画资源 = jsonData.动画;
                        
                        // 标记为已修改
                        EditorUtility.SetDirty(petConfig);
                        updatedCount++;
                        
                        Debug.Log($"已更新宠物 {jsonData.宠物名称} 的资源");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"更新宠物 {jsonData.宠物名称} 失败: " + e.Message);
                        errorCount++;
                    }
                }
            }

            // 保存所有修改
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完成", 
                $"资源替换完成！\n成功更新: {updatedCount} 个\n失败: {errorCount} 个", "确定");
        }

        /// <summary>
        /// 查找宠物配置
        /// </summary>
        private PetConfig FindPetConfig(string petName)
        {
            var allConfigs = GetAllPetConfigs();
            return allConfigs.Find(config => config.宠物名称 == petName);
        }

        /// <summary>
        /// 获取所有宠物配置
        /// </summary>
        private List<PetConfig> GetAllPetConfigs()
        {
            var configs = new List<PetConfig>();
            var guids = AssetDatabase.FindAssets("t:PetConfig", new[] { petConfigFolder });
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
            
            return configs;
        }

        /// <summary>
        /// 显示进度信息
        /// </summary>
        private void DisplayProgress()
        {
            if (jsonDataList.Count > 0)
            {
                EditorGUILayout.LabelField($"已读取 {jsonDataList.Count} 条宠物数据", EditorStyles.boldLabel);
                EditorGUILayout.Space();
            }

            if (previewResults.Count > 0)
            {
                EditorGUILayout.LabelField("预览结果:", EditorStyles.boldLabel);
                foreach (var result in previewResults)
                {
                    if (result.StartsWith("宠物:") || result.StartsWith("  模型:") || result.StartsWith("  动画:"))
                    {
                        EditorGUILayout.LabelField(result, EditorStyles.miniLabel);
                    }
                    else if (result.StartsWith("未找到") || result.StartsWith("预览结果:"))
                    {
                        EditorGUILayout.LabelField(result, EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(result);
                    }
                }
            }
        }
    }
}
