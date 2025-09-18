using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MiGame.Pet;

public class PetEffectBatchUpdater : EditorWindow
{
    private bool updateAllPets = false;
    private bool updateByDirectory = true;  // 默认选择按目录更新
    private Vector2 scrollPosition;
    
    // 预定义的目录选项
    private readonly string[] predefinedDirectories = {
        "GameConf/宠物/中级终极",
        "GameConf/宠物/高级初级",
        "GameConf/宠物/高级中级",
        "GameConf/宠物/高级高级",
        "GameConf/宠物/扭蛋商城", 
        "GameConf/宠物/商城"
    };
    
    // 选中的目录索引
    private int selectedDirectoryIndex = 0;

    [MenuItem("Tools/批量标准化宠物携带效果")]
    public static void ShowWindow()
    {
        GetWindow<PetEffectBatchUpdater>("批量更新宠物携带效果");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量更新宠物携带效果", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("从宠物JSON读取\"加成_百分比_训练加成\"，批量更新宠物配置的携带效果数值。\n匹配规则：优先按路径 Assets/GameConf/宠物/{品级}/{宠物名称}.asset 加载；失败则扫描全局同名 PetConfig。", MessageType.Info);

        EditorGUILayout.Space();
        
        // 更新方式选择
        GUILayout.Label("更新方式:", EditorStyles.boldLabel);
        updateAllPets = EditorGUILayout.Toggle("更新所有宠物", updateAllPets);
        
        if (!updateAllPets)
        {
            updateByDirectory = EditorGUILayout.Toggle("按目录更新", updateByDirectory);
            
            if (updateByDirectory)
            {
                EditorGUILayout.Space();
                GUILayout.Label("选择目标目录:", EditorStyles.boldLabel);
                selectedDirectoryIndex = EditorGUILayout.Popup("目录", selectedDirectoryIndex, predefinedDirectories);
                
                EditorGUILayout.HelpBox($"将更新目录 \"{predefinedDirectories[selectedDirectoryIndex]}\" 下的所有宠物配置", MessageType.Info);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("更新携带效果", GUILayout.Height(30)))
        {
            if (updateAllPets)
            {
                UpdatePetTrainingBonusFromJson();
            }
            else if (updateByDirectory)
            {
                UpdatePetTrainingBonusFromJsonByDirectory();
            }
        }
    }

    /// <summary>
    /// 从宠物JSON读取"加成_百分比_训练加成"，批量更新所有宠物的携带效果数值。
    /// </summary>
    public static void UpdatePetTrainingBonusFromJson()
    {
        try
        {
            string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物_数据_转换后_按抽奖区域排序.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"未找到宠物配置JSON文件: {jsonPath}");
                return;
            }

            string jsonText = File.ReadAllText(jsonPath);
            var jsonList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
            if (jsonList == null || jsonList.Count == 0)
            {
                Debug.LogError("宠物配置JSON解析失败或数据为空");
                return;
            }

            int updateCount = 0;
            int notFoundCount = 0;
            int skippedCount = 0;
            List<string> notFoundNames = new List<string>();

            foreach (var item in jsonList)
            {
                if (string.IsNullOrEmpty(item.宠物名称))
                {
                    skippedCount++;
                    continue;
                }

                // 从JSON读取效果数值
                string effectValue = item.加成_百分比_训练加成;
                if (string.IsNullOrEmpty(effectValue))
                {
                    // 没有配置则跳过，不清空既有配置
                    skippedCount++;
                    continue;
                }

                // 尝试按品级路径直接定位
                string directPath = BuildPetAssetPath(item.品级, item.宠物名称);
                PetConfig pet = AssetDatabase.LoadAssetAtPath<PetConfig>(directPath);
                if (pet == null)
                {
                    // 退化为全局查找同名 PetConfig
                    pet = FindPetByName(item.宠物名称);
                }

                if (pet == null)
                {
                    notFoundCount++;
                    notFoundNames.Add(item.宠物名称);
                    continue;
                }

                // 更新（或创建）携带效果里“训练加成”的效果数值
                bool changed = ApplyTrainingBonusEffect(pet, effectValue);
                if (changed)
                {
                    EditorUtility.SetDirty(pet);
                    updateCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            if (updateCount > 0)
            {
                AssetDatabase.SaveAssets();
            }
            // 避免导入期间刷新问题，使用延迟调用
            EditorApplication.delayCall += () =>
            {
                if (!AssetDatabase.IsAssetImportWorkerProcess())
                {
                    AssetDatabase.Refresh();
                }
            };

            Debug.Log($"宠物训练加成批量更新完成：成功 {updateCount} 项，跳过 {skippedCount} 项，未找到 {notFoundCount} 项");
            if (notFoundNames.Count > 0)
            {
                Debug.LogWarning("未找到的宠物：" + string.Join("，", notFoundNames));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 从宠物JSON读取"加成_百分比_训练加成"，批量更新指定目录下的宠物携带效果数值。
    /// </summary>
    private void UpdatePetTrainingBonusFromJsonByDirectory()
    {
        try
        {
            string targetDirectory = predefinedDirectories[selectedDirectoryIndex];
            string fullPath = "Assets/" + targetDirectory;
            
            // 查找指定目录下的所有PetConfig资产
            string[] guids = AssetDatabase.FindAssets("t:PetConfig", new[] { fullPath });
            List<PetConfig> directoryPetConfigs = new List<PetConfig>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PetConfig config = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
                if (config != null)
                {
                    directoryPetConfigs.Add(config);
                }
            }

            if (directoryPetConfigs.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"目录 \"{targetDirectory}\" 下没有找到宠物配置文件！", "确定");
                return;
            }

            // 读取JSON数据
            string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物_数据_转换后_按抽奖区域排序.json");
            if (!File.Exists(jsonPath))
            {
                EditorUtility.DisplayDialog("错误", $"未找到宠物配置JSON文件: {jsonPath}", "确定");
                return;
            }

            string jsonText = File.ReadAllText(jsonPath);
            
            List<PetJsonData> jsonList;
            try
            {
                jsonList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
                if (jsonList == null || jsonList.Count == 0)
                {
                    EditorUtility.DisplayDialog("错误", "宠物配置JSON解析失败或数据为空", "确定");
                    return;
                }
                
                Debug.Log($"JSON解析成功，共解析出 {jsonList.Count} 个宠物");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JSON解析异常: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"JSON解析异常: {ex.Message}", "确定");
                return;
            }

            int updateCount = 0;
            int notFoundCount = 0;
            int skippedCount = 0;
            List<string> notFoundNames = new List<string>();

            // 调试：输出JSON中所有宠物名称
            Debug.Log($"JSON中共有 {jsonList.Count} 个宠物:");
            foreach (var item in jsonList)
            {
                Debug.Log($"JSON宠物: '{item.宠物名称}' (长度: {item.宠物名称?.Length})");
            }

            // 为每个目录中的宠物查找对应的JSON数据并更新
            foreach (var pet in directoryPetConfigs)
            {
                if (string.IsNullOrEmpty(pet.宠物名称))
                {
                    skippedCount++;
                    continue;
                }

                // 在JSON中查找对应的宠物数据
                var jsonItem = jsonList.FirstOrDefault(item => item.宠物名称 == pet.宠物名称);
                if (jsonItem == null)
                {
                    // 添加调试信息，查看具体的名称比较
                    Debug.LogWarning($"未找到JSON数据 - 宠物名称: '{pet.宠物名称}' (长度: {pet.宠物名称?.Length})");
                    
                    // 检查JSON中是否真的存在这个名称
                    var exactMatch = jsonList.FirstOrDefault(item => item.宠物名称 == pet.宠物名称);
                    if (exactMatch != null)
                    {
                        Debug.LogWarning($"奇怪，应该能找到: {exactMatch.宠物名称}");
                    }
                    else
                    {
                        // 尝试查找相似的名称
                        var similarItems = jsonList.Where(item => 
                            !string.IsNullOrEmpty(item.宠物名称) && 
                            (item.宠物名称.Contains(pet.宠物名称) || pet.宠物名称.Contains(item.宠物名称))
                        ).ToList();
                        
                        if (similarItems.Count > 0)
                        {
                            Debug.LogWarning($"找到相似名称: {string.Join(", ", similarItems.Select(s => $"'{s.宠物名称}'"))}");
                        }
                        else
                        {
                            Debug.LogWarning($"在JSON中完全找不到包含 '{pet.宠物名称}' 的项");
                        }
                    }
                    
                    notFoundCount++;
                    notFoundNames.Add(pet.宠物名称);
                    continue;
                }

                // 从JSON读取效果数值
                string effectValue = jsonItem.加成_百分比_训练加成;
                if (string.IsNullOrEmpty(effectValue))
                {
                    skippedCount++;
                    continue;
                }

                // 更新携带效果
                bool changed = ApplyTrainingBonusEffect(pet, effectValue);
                if (changed)
                {
                    EditorUtility.SetDirty(pet);
                    updateCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            if (updateCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            // 避免导入期间刷新问题，使用延迟调用
            EditorApplication.delayCall += () =>
            {
                if (!AssetDatabase.IsAssetImportWorkerProcess())
                {
                    AssetDatabase.Refresh();
                }
            };

            EditorUtility.DisplayDialog("操作完成", 
                $"批量更新目录 \"{targetDirectory}\" 下的宠物携带效果完成！\n\n" +
                $"成功更新 {updateCount} 个宠物，跳过 {skippedCount} 个，未找到JSON数据 {notFoundCount} 个。", "确定");

            Debug.Log($"宠物训练加成批量更新完成：成功 {updateCount} 项，跳过 {skippedCount} 项，未找到 {notFoundCount} 项");
            if (notFoundNames.Count > 0)
            {
                Debug.LogWarning("未找到JSON数据的宠物：" + string.Join("，", notFoundNames));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"批量更新出现异常: {ex.Message}", "确定");
        }
    }

    private static string BuildPetAssetPath(string quality, string name)
    {
        // 直接按原样放入品级子目录，保持与现有目录一致（N/R/SR/SSR/UR）
        // 若 quality 为空，尝试在根目录查找
        if (string.IsNullOrEmpty(quality))
        {
            return $"Assets/GameConf/宠物/{name}.asset";
        }
        return $"Assets/GameConf/宠物/{quality}/{name}.asset";
    }

    private static PetConfig FindPetByName(string name)
    {
        // 全局查找所有 PetConfig 资源，匹配 ScriptableObject 字段“宠物名称”或资源名
        string[] guids = AssetDatabase.FindAssets("t:MiGame.Pet.PetConfig");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var pet = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
            if (pet == null) continue;
            if (pet.宠物名称 == name || pet.name == name)
            {
                return pet;
            }
        }
        return null;
    }

    private static bool ApplyTrainingBonusEffect(PetConfig pet, string effectValue)
    {
        if (pet.携带效果 == null)
        {
            pet.携带效果 = new List<携带效果>();
        }

        // 优先更新变量名称为“训练加成”的效果
        var effect = pet.携带效果.FirstOrDefault(e => e != null && e.变量名称 == "训练加成");
        if (effect != null)
        {
            if (effect.效果数值 == effectValue)
            {
                return false; // 无变化
            }
            effect.效果数值 = effectValue;
            return true;
        }

        // 若没有“训练加成”，且存在单个效果，则更新第一个效果的数值（保守更新）
        if (pet.携带效果.Count == 1 && pet.携带效果[0] != null)
        {
            var first = pet.携带效果[0];
            if (first.效果数值 == effectValue)
            {
                return false;
            }
            first.效果数值 = effectValue;
            return true;
        }

        // 否则新增一个“训练加成”效果项
        var newEffect = new 携带效果
        {
            变量类型 = 变量类型.玩家属性,
            变量名称 = "训练加成",
            效果数值 = effectValue,
            加成类型 = 加成类型.玩家属性,
            目标变量 = string.Empty,
            作用类型 = "单独相加"
        };
        pet.携带效果.Add(newEffect);
        return true;
    }

    [Serializable]
    private class PetJsonData
    {
        // 与 Scripts/配置exel/宠物.json 字段对齐
        public string 宠物名称;
        public string 抽奖区域;
        public string 品级;
        public string 图片;
        public string 图片资源;
        public string 模型;
        public string 动画;
        public string 加成_百分比_训练加成;
    }
}


