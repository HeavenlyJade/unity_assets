using System.IO;
using UnityEditor;
using UnityEngine;
using MiGame.Scene;

/// <summary>
/// 出生地图左侧挂机点批量生成器：基于已有模板（默认使用“挂机点1”），生成“挂机点2~12”。
/// </summary>
public static class BatchGenerateAfkSpots
{
    private const string BaseDir = "Assets/GameConf/场景/挂机点/出生地图";
    private const string TemplateName = "挂机点1.asset";
    private const string ScenePathPrefix = "Ground/init_map/terrain/AutoSpot/左侧挂机点/";

    [MenuItem("Tools/场景/批量生成出生地图左侧挂机点(2-12)")]
    public static void GenerateAfkSpots()
    {
        // 基础校验
        if (!AssetDatabase.IsValidFolder("Assets/GameConf") || !AssetDatabase.IsValidFolder("Assets/GameConf/场景"))
        {
            Debug.LogError("未找到基础目录: Assets/GameConf/场景");
            return;
        }

        if (!AssetDatabase.IsValidFolder(BaseDir))
        {
            Debug.LogError($"未找到挂机点目录: {BaseDir}");
            return;
        }

        // 加载模板（优先使用 挂机点1.asset）
        string templatePath = Path.Combine(BaseDir, TemplateName).Replace("\\", "/");
        SceneNodeConfig template = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(templatePath);
        if (template == null)
        {
            // 若没有“挂机点1”，则尝试目录内任意一个 SceneNodeConfig 作为模板
            string[] candidates = AssetDatabase.FindAssets("t:SceneNodeConfig", new[] { BaseDir });
            if (candidates != null && candidates.Length > 0)
            {
                string anyPath = AssetDatabase.GUIDToAssetPath(candidates[0]);
                template = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(anyPath);
            }
        }

        if (template == null)
        {
            Debug.LogError("未能找到模板 SceneNodeConfig（例如：挂机点1.asset）。");
            return;
        }

        EditorApplication.LockReloadAssemblies();
        AssetDatabase.DisallowAutoRefresh();
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 2; i <= 12; i++)
            {
                string assetName = $"挂机点{i}.asset";
                string assetPath = Path.Combine(BaseDir, assetName).Replace("\\", "/");

                // 若已存在则更新，不存在则克隆创建
                SceneNodeConfig target = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(assetPath);
                if (target == null)
                {
                    // 克隆模板
                    target = Object.Instantiate(template);
                    // 重置唯一ID，让 OnValidate 自动生成
                    target.唯一ID = string.Empty;
                    // 设置显示名（初次会被同步）
                    target.名字 = $"挂机点{i}";
                    // 设置节点路径
                    target.场景节点路径 = ScenePathPrefix + $"挂机点{i}";

                    AssetDatabase.CreateAsset(target, assetPath);
                    EditorUtility.SetDirty(target);
                    Debug.Log($"已创建: {assetPath}");
                }
                else
                {
                    // 更新已有
                    target.名字 = $"挂机点{i}";
                    target.场景节点路径 = ScenePathPrefix + $"挂机点{i}";
                    EditorUtility.SetDirty(target);
                    Debug.Log($"已更新: {assetPath}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.AllowAutoRefresh();
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
        }

        Debug.Log("批量生成完成: 出生地图 左侧 挂机点(2-12)");
    }
}


