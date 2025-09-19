using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Pet;
using System.Linq;

public class BatchUpdatePartnerConfigEditor : EditorWindow
{
    private PartnerConfig sourceConfig;
    private bool updateAllPartners = false;  // 默认不选择更新所有
    private bool updateByDirectory = true;   // 默认选择按目录更新
    private string targetPartnerNamesText = "";
    private Vector2 scrollPosition;
    
    // 预定义的目录选项
    private readonly string[] predefinedDirectories = {
        "GameConf/伙伴/初级初级",
        "GameConf/伙伴/初级终极", 
        "GameConf/伙伴/中级初级",
        "GameConf/伙伴/中级终极",
        "GameConf/伙伴/高级初级",
        "GameConf/伙伴/高级终极",
        "GameConf/伙伴/在线奖励",
        "GameConf/伙伴/商店区域",
        "GameConf/伙伴/扭蛋商城"
    };
    
    // 选中的目录索引
    private int selectedDirectoryIndex = 0;

    [MenuItem("Tools/批量更新伙伴升星消耗")]
    public static void ShowWindow()
    {
        GetWindow<BatchUpdatePartnerConfigEditor>("批量更新伙伴配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量更新伙伴配置", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("1. 将作为模板的伙伴配置文件拖入下方\"源伙伴配置\"。\n2. 选择更新方式：所有伙伴、指定伙伴或按目录更新。\n3. 点击\"更新配置\"按钮。", MessageType.Info);

        sourceConfig = (PartnerConfig)EditorGUILayout.ObjectField("源伙伴配置 (模板)", sourceConfig, typeof(PartnerConfig), false);

        EditorGUILayout.Space();
        
        // 更新方式选择
        GUILayout.Label("更新方式:", EditorStyles.boldLabel);
        updateAllPartners = EditorGUILayout.Toggle("更新所有伙伴", updateAllPartners);
        
        if (!updateAllPartners)
        {
            updateByDirectory = EditorGUILayout.Toggle("按目录更新", updateByDirectory);
            
            if (updateByDirectory)
            {
                EditorGUILayout.Space();
                GUILayout.Label("选择目标目录:", EditorStyles.boldLabel);
                selectedDirectoryIndex = EditorGUILayout.Popup("目录", selectedDirectoryIndex, predefinedDirectories);
                
                EditorGUILayout.HelpBox($"将更新目录 \"{predefinedDirectories[selectedDirectoryIndex]}\" 下的所有伙伴升星消耗", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                GUILayout.Label("指定伙伴更新:", EditorStyles.boldLabel);
                GUILayout.Label("目标伙伴名称 (每行一个):");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
                targetPartnerNamesText = EditorGUILayout.TextArea(targetPartnerNamesText);
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.HelpBox("将更新指定名称的伙伴升星消耗", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("将更新所有伙伴的升星消耗和携带效果", MessageType.Info);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("更新配置", GUILayout.Height(30)))
        {
            if (sourceConfig == null)
            {
                Debug.LogError("请先指定源伙伴配置！");
                return;
            }

            if (!updateAllPartners && !updateByDirectory && string.IsNullOrWhiteSpace(targetPartnerNamesText))
            {
                Debug.LogError("请在文本框中输入目标伙伴的名称！");
                return;
            }

            if (updateAllPartners)
            {
                BatchUpdateAllPartners();
            }
            else if (updateByDirectory)
            {
                BatchUpdatePartnersInDirectory();
            }
            else
            {
                BatchUpdateSpecificPartners();
            }
        }
    }

    /// <summary>
    /// 批量更新所有伙伴的升星消耗配置
    /// </summary>
    private void BatchUpdateAllPartners()
    {
        // 查找所有PartnerConfig资产
        string[] guids = AssetDatabase.FindAssets("t:PartnerConfig");
        List<PartnerConfig> allPartnerConfigs = new List<PartnerConfig>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartnerConfig config = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);
            if (config != null && config != sourceConfig) // 排除源配置自身
            {
                allPartnerConfigs.Add(config);
            }
        }

        int updatedCount = 0;
        foreach (PartnerConfig targetConfig in allPartnerConfigs)
        {
            if (UpdatePartnerConfig(targetConfig))
            {
                updatedCount++;
                Debug.Log($"成功更新伙伴配置: {targetConfig.宠物名称}");
            }
        }

        // 保存所有修改过的资产
        AssetDatabase.SaveAssets();
        // 刷新编辑器资源库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("操作完成", $"批量更新所有伙伴配置完成！\n\n成功更新 {updatedCount} 个伙伴。", "确定");
    }

    /// <summary>
    /// 批量更新指定目录下的伙伴升星消耗配置
    /// </summary>
    private void BatchUpdatePartnersInDirectory()
    {
        try
        {
            string targetDirectory = predefinedDirectories[selectedDirectoryIndex];
            string fullPath = "Assets/" + targetDirectory;
            
            string[] guids = AssetDatabase.FindAssets("t:PartnerConfig", new[] { fullPath });
            List<PartnerConfig> directoryPartnerConfigs = new List<PartnerConfig>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PartnerConfig config = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);
                if (config != null)
                {
                    directoryPartnerConfigs.Add(config);
                }
            }

            if (directoryPartnerConfigs.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"目录 \"{targetDirectory}\" 下没有找到伙伴配置文件！", "确定");
                return;
            }

            int updatedCount = 0;
            foreach (var partner in directoryPartnerConfigs)
            {
                if (UpdatePartnerStarUpConsumption(partner))
                {
                    EditorUtility.SetDirty(partner);
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            EditorApplication.delayCall += () =>
            {
                if (!AssetDatabase.IsAssetImportWorkerProcess())
                {
                    AssetDatabase.Refresh();
                }
            };

            EditorUtility.DisplayDialog("操作完成", 
                $"批量更新目录 \"{targetDirectory}\" 下的伙伴升星消耗完成！\n\n" +
                $"成功更新 {updatedCount} 个伙伴。", "确定");

            Debug.Log($"伙伴升星消耗批量更新完成：成功 {updatedCount} 项");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"批量更新出现异常: {ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 批量更新指定伙伴的升星消耗配置
    /// </summary>
    private void BatchUpdateSpecificPartners()
    {
        List<string> targetPartnerNames = targetPartnerNamesText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(name => name.Trim())
                                                              .Where(name => !string.IsNullOrEmpty(name))
                                                              .ToList();
        
        int updatedCount = 0;
        foreach (string partnerName in targetPartnerNames)
        {
            // 使用AssetDatabase.FindAssets在整个项目中查找配置
            string[] guids = AssetDatabase.FindAssets($"{partnerName} t:PartnerConfig");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"未找到名为 '{partnerName}' 的伙伴配置文件，已跳过。");
                continue;
            }

            // 理论上伙伴名称是唯一的，所以我们只取第一个结果
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            PartnerConfig targetConfig = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);

            if (targetConfig != null && UpdatePartnerStarUpConsumption(targetConfig))
            {
                Debug.Log($"成功更新伙伴配置: {partnerName} (路径: {path})");
                updatedCount++;
            }
        }

        // 保存所有修改过的资产
        AssetDatabase.SaveAssets();
        // 刷新编辑器资源库
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("操作完成", $"批量更新伙伴配置完成！\n\n成功更新 {updatedCount} / {targetPartnerNames.Count} 个伙伴。", "确定");
    }

    /// <summary>
    /// 更新单个伙伴配置
    /// </summary>
    /// <param name="targetConfig">目标伙伴配置</param>
    /// <returns>是否更新成功</returns>
    private bool UpdatePartnerConfig(PartnerConfig targetConfig)
    {
        try
        {
            // 深拷贝并修改升星消耗列表，确保消耗的伙伴是目标伙伴自身
            targetConfig.升星消耗列表 = new List<升星消耗>();
            foreach (var source消耗 in sourceConfig.升星消耗列表)
            {
                var new消耗 = new 升星消耗
                {
                    星级 = source消耗.星级,
                    成功率 = source消耗.成功率,
                    消耗材料 = new List<升星材料>()
                };

                foreach (var sourceMaterial in source消耗.消耗材料)
                {
                    var newMaterial = new 升星材料
                    {
                        消耗类型 = sourceMaterial.消耗类型,
                        材料物品 = sourceMaterial.材料物品,
                        消耗名称 = sourceMaterial.消耗名称,
                        需要数量 = sourceMaterial.需要数量,
                        消耗星级 = sourceMaterial.消耗星级
                    };

                    // 如果消耗材料是源伙伴本身，则将其替换为当前的目标伙伴
                    if ((newMaterial.消耗类型 == 消耗类型.伙伴 || newMaterial.消耗类型 == 消耗类型.宠物) && newMaterial.消耗名称 == sourceConfig)
                    {
                        newMaterial.消耗名称 = targetConfig;
                    }
                    
                    new消耗.消耗材料.Add(newMaterial);
                }
                targetConfig.升星消耗列表.Add(new消耗);
            }

            
            // 携带效果可以直接复制
            targetConfig.携带效果 = new List<携带效果>(sourceConfig.携带效果);
            
            // 标记为已修改，以便保存
            EditorUtility.SetDirty(targetConfig);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"更新伙伴配置 {targetConfig.宠物名称} 时发生错误: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 只更新伙伴的升星消耗配置（不修改携带效果）
    /// </summary>
    /// <param name="targetConfig">目标伙伴配置</param>
    /// <returns>是否更新成功</returns>
    private bool UpdatePartnerStarUpConsumption(PartnerConfig targetConfig)
    {
        try
        {
            // 只修改升星消耗列表，确保消耗的伙伴是目标伙伴自身
            targetConfig.升星消耗列表 = new List<升星消耗>();
            foreach (var source消耗 in sourceConfig.升星消耗列表)
            {
                var new消耗 = new 升星消耗
                {
                    星级 = source消耗.星级,
                    成功率 = source消耗.成功率,
                    消耗材料 = new List<升星材料>()
                };

                foreach (var sourceMaterial in source消耗.消耗材料)
                {
                    var newMaterial = new 升星材料
                    {
                        消耗类型 = sourceMaterial.消耗类型,
                        材料物品 = sourceMaterial.材料物品,
                        消耗名称 = sourceMaterial.消耗名称,
                        需要数量 = sourceMaterial.需要数量,
                        消耗星级 = sourceMaterial.消耗星级
                    };

                    // 如果消耗材料是源伙伴本身，则将其替换为当前的目标伙伴
                    if ((newMaterial.消耗类型 == 消耗类型.伙伴 || newMaterial.消耗类型 == 消耗类型.宠物) && newMaterial.消耗名称 == sourceConfig)
                    {
                        newMaterial.消耗名称 = targetConfig;
                    }
                    
                    new消耗.消耗材料.Add(newMaterial);
                }
                targetConfig.升星消耗列表.Add(new消耗);
            }

            // 不修改携带效果，保持原有配置
            // targetConfig.携带效果 保持不变
            
            // 标记为已修改，以便保存
            EditorUtility.SetDirty(targetConfig);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"更新伙伴升星消耗 {targetConfig.宠物名称} 时发生错误: {ex.Message}");
            return false;
        }
    }
}
