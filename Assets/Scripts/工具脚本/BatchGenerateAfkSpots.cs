using System.IO;
using UnityEditor;
using UnityEngine;
using MiGame.Scene;

/// <summary>
/// 出生地图左侧挂机点批量生成器：基于已有模板（默认使用“挂机点1”），生成“挂机点2~12”。
/// </summary>
public static class BatchGenerateAfkSpots
{
    private const string BaseDir = "Assets/GameConf/场景/挂机点/巨鲸岛/";
    private const string TemplateName = "模板.asset";
    private const string ScenePathPrefix = "Ground/map2/terrain/AutoSpot/左侧挂机点/";

    [MenuItem("Tools/场景/批量生成科技岛出生点(1-12)")]
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
            for (int i = 1; i <= 12; i++)
            {
                string assetName = $"巨鲸岛挂机点{i}.asset";
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
                    // 根据编号判断左右侧并设置节点路径
                    string sidePath = i <= 6 ? "左侧挂机点/" : "右侧挂机点/";
                    target.场景节点路径 = "Ground/map2/terrain/AutoSpot/" + sidePath + $"挂机点{i}";
                    // 设置进入指令：启动飞行动画
                    target.进入指令 = "animation { \"操作类型\": \"启动飞行\" }";
                    // 设置离开指令：取消飞行动画
                    target.离开指令 = "animation {\"操作类型\": \"取消飞行\"}";
                    // 设置区域节点配置
                    target.区域节点配置.包围盒节点 = "TriggerBox";
                    target.区域节点配置.需求描述节点 = "训练奖励/需求";
                    target.区域节点配置.作用描述节点 = "训练奖励/训练数据";

                    AssetDatabase.CreateAsset(target, assetPath);
                    EditorUtility.SetDirty(target);
                    Debug.Log($"已创建: {assetPath}");
                }
                else
                {
                    // 更新已有
                    target.名字 = $"挂机点{i}";
                    // 根据编号判断左右侧并更新节点路径
                    string sidePath = i <= 6 ? "左侧挂机点/" : "右侧挂机点/";
                    target.场景节点路径 = "Ground/map2/terrain/AutoSpot/" + sidePath + $"挂机点{i}";
                    // 更新进入指令：启动飞行动画
                    target.进入指令 = "animation { \"操作类型\": \"启动飞行\" }";
                    // 更新离开指令：取消飞行动画
                    target.离开指令 = "animation {\"操作类型\": \"取消飞行\"}";
                    // 更新区域节点配置
                    target.区域节点配置.包围盒节点 = "TriggerBox";
                    target.区域节点配置.需求描述节点 = "训练奖励/需求";
                    target.区域节点配置.作用描述节点 = "训练奖励/训练数据";
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

        Debug.Log("批量生成完成: 科技岛挂机点(1-12) - 1-6为左侧，7-12为右侧");
    }
}


