using UnityEngine;
using UnityEditor;
using MiGame.Trail;
using System.Collections.Generic;

public class UpdateTrailDisplayNames : EditorWindow
{
    [MenuItem("Tools/批量修改尾迹显示名称")]
    public static void ShowWindow()
    {
        GetWindow<UpdateTrailDisplayNames>("批量修改尾迹显示名称");
    }

    private void OnGUI()
    {
        GUILayout.Label("此工具将批量修改尾迹配置的显示名称", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("规则：将尾迹名称按下划线分割，取最后一个元素作为显示名称", EditorStyles.wordWrappedLabel);
        GUILayout.Label("例如：拖尾_星空 → 显示名称：星空", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("开始批量修改"))
        {
            UpdateAllTrailDisplayNames();
        }
    }

    private static void UpdateAllTrailDisplayNames()
    {
        // 查找所有尾迹配置文件
        string[] guids = AssetDatabase.FindAssets("t:BaseTrailConfig");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "项目中未找到任何尾迹(BaseTrailConfig)配置文件。", "确定");
            return;
        }

        int updatedCount = 0;
        List<string> updatedTrails = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseTrailConfig trail = AssetDatabase.LoadAssetAtPath<BaseTrailConfig>(path);

            if (trail == null) continue;

            // 获取原始名称
            string originalName = trail.名称;
            if (string.IsNullOrEmpty(originalName))
            {
                originalName = trail.name; // 如果名称为空，使用文件名
            }

            // 按下划线分割名称
            string[] nameParts = originalName.Split('_');
            if (nameParts.Length > 1)
            {
                // 取最后一个元素作为显示名称
                string displayName = nameParts[nameParts.Length - 1];
                
                // 更新显示名称
                trail.显示名 = displayName;
                
                // 标记为已修改
                EditorUtility.SetDirty(trail);
                updatedCount++;
                updatedTrails.Add($"{originalName} → {displayName}");
            }
            else
            {
                // 如果没有下划线，直接使用原名称
                trail.显示名 = originalName;
                EditorUtility.SetDirty(trail);
                updatedCount++;
                updatedTrails.Add($"{originalName} → {originalName}");
            }
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        
        // 显示结果
        string summary = $"操作完成！\n\n- 共修改了 {updatedCount} 个尾迹的显示名称。\n\n修改详情：\n";
        foreach (string change in updatedTrails)
        {
            summary += $"- {change}\n";
        }
        
        EditorUtility.DisplayDialog("批量修改成功", summary, "确定");
        
        AssetDatabase.Refresh();
    }
} 