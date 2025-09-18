using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Pet;
using System.Linq;

public class BatchUpdatePetConfigEditor : EditorWindow
{
    private PetConfig sourceConfig;
    private bool updateAllPets = false;
    private bool updateByDirectory = true;  // 默认选择按目录更新
    private string targetPetNamesText = "";
    private Vector2 scrollPosition;
    
    // 预定义的目录选项
    private readonly string[] predefinedDirectories = {
        "GameConf/宠物/中级终极",
        "GameConf/宠物/扭蛋商城", 
        "GameConf/宠物/商城"
    };
    
    // 选中的目录索引
    private int selectedDirectoryIndex = 0;

    [MenuItem("Tools/批量更新宠物升星消耗")]
    public static void ShowWindow()
    {
        GetWindow<BatchUpdatePetConfigEditor>("批量更新宠物配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量更新宠物配置", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("1. 将作为模板的宠物配置文件拖入下方\"源宠物配置\"。\n2. 选择目标目录进行批量更新。\n3. 点击\"更新配置\"按钮。", MessageType.Info);

        sourceConfig = (PetConfig)EditorGUILayout.ObjectField("源宠物配置 (模板)", sourceConfig, typeof(PetConfig), false);

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
            else
            {
                EditorGUILayout.Space();
                GUILayout.Label("目标宠物名称 (每行一个):");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
                targetPetNamesText = EditorGUILayout.TextArea(targetPetNamesText);
                EditorGUILayout.EndScrollView();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("更新配置", GUILayout.Height(30)))
        {
            if (sourceConfig == null)
            {
                Debug.LogError("请先指定源宠物配置！");
                return;
            }

            if (!updateAllPets && !updateByDirectory && string.IsNullOrWhiteSpace(targetPetNamesText))
            {
                Debug.LogError("请选择更新方式或输入目标宠物的名称！");
                return;
            }

            if (updateAllPets)
            {
                BatchUpdateAllPets();
            }
            else if (updateByDirectory)
            {
                BatchUpdatePetsInDirectory();
            }
            else
            {
                BatchUpdateSpecificPets();
            }
        }
    }

    /// <summary>
    /// 批量更新所有宠物的升星消耗配置
    /// </summary>
    private void BatchUpdateAllPets()
    {
        // 查找所有PetConfig资产
        string[] guids = AssetDatabase.FindAssets("t:PetConfig");
        List<PetConfig> allPetConfigs = new List<PetConfig>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PetConfig config = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
            if (config != null && config != sourceConfig) // 排除源配置自身
            {
                allPetConfigs.Add(config);
            }
        }

        int updatedCount = 0;
        foreach (PetConfig targetConfig in allPetConfigs)
        {
            if (UpdatePetConfig(targetConfig))
            {
                updatedCount++;
                Debug.Log($"成功更新宠物配置: {targetConfig.宠物名称}");
            }
        }

        // 保存所有修改过的资产
        AssetDatabase.SaveAssets();
        // 刷新编辑器资源库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("操作完成", $"批量更新所有宠物配置完成！\n\n成功更新 {updatedCount} 个宠物。", "确定");
    }

    /// <summary>
    /// 批量更新指定目录下的所有宠物配置
    /// </summary>
    private void BatchUpdatePetsInDirectory()
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
            if (config != null && config != sourceConfig) // 排除源配置自身
            {
                directoryPetConfigs.Add(config);
            }
        }

        if (directoryPetConfigs.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", $"目录 \"{targetDirectory}\" 下没有找到宠物配置文件！", "确定");
            return;
        }

        int updatedCount = 0;
        foreach (PetConfig targetConfig in directoryPetConfigs)
        {
            if (UpdatePetConfig(targetConfig))
            {
                updatedCount++;
                Debug.Log($"成功更新宠物配置: {targetConfig.宠物名称}");
            }
        }

        // 保存所有修改过的资产
        AssetDatabase.SaveAssets();
        // 刷新编辑器资源库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("操作完成", $"批量更新目录 \"{targetDirectory}\" 下的宠物配置完成！\n\n成功更新 {updatedCount} / {directoryPetConfigs.Count} 个宠物。", "确定");
    }

    /// <summary>
    /// 批量更新指定宠物的升星消耗配置
    /// </summary>
    private void BatchUpdateSpecificPets()
    {
        List<string> targetPetNames = targetPetNamesText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(name => name.Trim())
                                                              .Where(name => !string.IsNullOrEmpty(name))
                                                              .ToList();
        
        int updatedCount = 0;
        foreach (string petName in targetPetNames)
        {
            // 使用AssetDatabase.FindAssets在整个项目中查找配置
            string[] guids = AssetDatabase.FindAssets($"{petName} t:PetConfig");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"未找到名为 '{petName}' 的宠物配置文件，已跳过。");
                continue;
            }

            // 理论上宠物名称是唯一的，所以我们只取第一个结果
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            PetConfig targetConfig = AssetDatabase.LoadAssetAtPath<PetConfig>(path);

            if (targetConfig != null && UpdatePetConfig(targetConfig))
            {
                Debug.Log($"成功更新宠物配置: {petName} (路径: {path})");
                updatedCount++;
            }
        }

        // 保存所有修改过的资产
        AssetDatabase.SaveAssets();
        // 刷新编辑器资源库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("操作完成", $"批量更新宠物配置完成！\n\n成功更新 {updatedCount} / {targetPetNames.Count} 个宠物。", "确定");
    }

    /// <summary>
    /// 更新单个宠物配置
    /// </summary>
    /// <param name="targetConfig">目标宠物配置</param>
    /// <returns>是否更新成功</returns>
    private bool UpdatePetConfig(PetConfig targetConfig)
    {
        try
        {
            // 清空并重新生成升星消耗列表，每个宠物的升星消耗都是自身
            targetConfig.升星消耗列表 = new List<升星消耗>();
            
            // 2星需要3只1星
            targetConfig.升星消耗列表.Add(new 升星消耗
            {
                星级 = 2,
                成功率 = 1.0f, // 100%成功率
                消耗材料 = new List<升星材料>
                {
                    new 升星材料
                    {
                        消耗类型 = 消耗类型.宠物,
                        消耗名称 = targetConfig,
                        需要数量 = 3,
                        消耗星级 = 1
                    }
                }
            });
            
            // 3星需要3个2星
            targetConfig.升星消耗列表.Add(new 升星消耗
            {
                星级 = 3,
                成功率 = 1.0f, // 100%成功率
                消耗材料 = new List<升星材料>
                {
                    new 升星材料
                    {
                        消耗类型 = 消耗类型.宠物,
                        消耗名称 = targetConfig,
                        需要数量 = 3,
                        消耗星级 = 2
                    }
                }
            });
            
            // 4星需要3个3星
            targetConfig.升星消耗列表.Add(new 升星消耗
            {
                星级 = 4,
                成功率 = 1.0f, // 100%成功率
                消耗材料 = new List<升星材料>
                {
                    new 升星材料
                    {
                        消耗类型 = 消耗类型.宠物,
                        消耗名称 = targetConfig,
                        需要数量 = 3,
                        消耗星级 = 3
                    }
                }
            });
            
            // 5星需要4个3星
            targetConfig.升星消耗列表.Add(new 升星消耗
            {
                星级 = 5,
                成功率 = 1.0f, // 100%成功率
                消耗材料 = new List<升星材料>
                {
                    new 升星材料
                    {
                        消耗类型 = 消耗类型.宠物,
                        消耗名称 = targetConfig,
                        需要数量 = 4,
                        消耗星级 = 3
                    }
                }
            });


            // 标记为已修改，以便保存
            EditorUtility.SetDirty(targetConfig);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"更新宠物配置 {targetConfig.宠物名称} 时发生错误: {ex.Message}");
            return false;
        }
    }
}
