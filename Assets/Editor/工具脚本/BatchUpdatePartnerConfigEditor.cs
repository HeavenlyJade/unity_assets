using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using MiGame.Pet;
using System.Linq;

public class BatchUpdatePartnerConfigEditor : EditorWindow
{
    private PartnerConfig sourceConfig;
    private string targetPartnerNamesText = "";
    private Vector2 scrollPosition;

    [MenuItem("Tools/批量更新伙伴配置(高级)")]
    public static void ShowWindow()
    {
        GetWindow<BatchUpdatePartnerConfigEditor>("批量更新伙伴配置");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量更新伙伴配置", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("1. 将作为模板的伙伴配置文件拖入下方“源伙伴配置”。\n2. 在文本框中输入所有目标伙伴的名字（每行一个）。\n3. 点击“更新配置”按钮。", MessageType.Info);

        sourceConfig = (PartnerConfig)EditorGUILayout.ObjectField("源伙伴配置 (模板)", sourceConfig, typeof(PartnerConfig), false);

        GUILayout.Label("目标伙伴名称 (每行一个):");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        targetPartnerNamesText = EditorGUILayout.TextArea(targetPartnerNamesText);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("更新配置"))
        {
            if (sourceConfig == null)
            {
                Debug.LogError("请先指定源伙伴配置！");
                return;
            }

            if (string.IsNullOrWhiteSpace(targetPartnerNamesText))
            {
                Debug.LogError("请在文本框中输入目标伙伴的名称！");
                return;
            }

            BatchUpdate();
        }
    }

    private void BatchUpdate()
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

            if (targetConfig != null)
            {
                // 复制配置
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
}
