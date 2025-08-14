using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Pet;
using System.Linq;

public class BatchUpdatePetConfigEditor : EditorWindow
{
    private PetConfig sourceConfig;
    private bool updateAllPets = true;
    private string targetPetNamesText = "";
    private Vector2 scrollPosition;

    [MenuItem("Tools/批量更新宠物升星消耗")]
    public static void ShowWindow()
    {
        GetWindow<BatchUpdatePetConfigEditor>("批量更新宠物配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量更新宠物配置", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("1. 将作为模板的宠物配置文件拖入下方\"源宠物配置\"。\n2. 选择是否更新所有宠物或指定宠物。\n3. 点击\"更新配置\"按钮。", MessageType.Info);

        sourceConfig = (PetConfig)EditorGUILayout.ObjectField("源宠物配置 (模板)", sourceConfig, typeof(PetConfig), false);

        EditorGUILayout.Space();
        updateAllPets = EditorGUILayout.Toggle("更新所有宠物", updateAllPets);
        
        if (!updateAllPets)
        {
            GUILayout.Label("目标宠物名称 (每行一个):");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            targetPetNamesText = EditorGUILayout.TextArea(targetPetNamesText);
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("更新配置"))
        {
            if (sourceConfig == null)
            {
                Debug.LogError("请先指定源宠物配置！");
                return;
            }

            if (!updateAllPets && string.IsNullOrWhiteSpace(targetPetNamesText))
            {
                Debug.LogError("请在文本框中输入目标宠物的名称！");
                return;
            }

            if (updateAllPets)
            {
                BatchUpdateAllPets();
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
            // 深拷贝并修改升星消耗列表，确保消耗的宠物是目标宠物自身
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

                    // 如果消耗材料是源宠物本身，则将其替换为当前的目标宠物
                    if ((newMaterial.消耗类型 == 消耗类型.宠物 || newMaterial.消耗类型 == 消耗类型.伙伴) && newMaterial.消耗名称 == sourceConfig)
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
            Debug.LogError($"更新宠物配置 {targetConfig.宠物名称} 时发生错误: {ex.Message}");
            return false;
        }
    }
}
