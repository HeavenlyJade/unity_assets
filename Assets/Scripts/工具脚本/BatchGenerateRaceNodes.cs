using System.IO;
using UnityEditor;
using UnityEngine;
using MiGame.Scene;

/// <summary>
/// 比赛节点批量生成器：基于现有飞行比赛节点配置生成新的比赛节点
/// </summary>
public static class BatchGenerateRaceNodes
{
    private const string BaseDir = "Assets/GameConf/场景/比赛节点/";
    private const string TemplateName = "樱花岛飞行.asset";
    private const string ScenePathPrefix = "Ground/init_map/terrain/Scene/";

    [MenuItem("Tools/场景/批量生成比赛节点")]
    public static void GenerateRaceNodes()
    {
        // 基础校验
        if (!AssetDatabase.IsValidFolder("Assets/GameConf") || !AssetDatabase.IsValidFolder("Assets/GameConf/场景"))
        {
            Debug.LogError("未找到基础目录: Assets/GameConf/场景");
            return;
        }

        if (!AssetDatabase.IsValidFolder(BaseDir))
        {
            Debug.LogError($"未找到比赛节点目录: {BaseDir}");
            return;
        }

        // 加载模板
        string templatePath = Path.Combine(BaseDir, TemplateName).Replace("\\", "/");
        SceneNodeConfig template = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(templatePath);
        if (template == null)
        {
            Debug.LogError($"未能找到模板比赛节点: {TemplateName}");
            return;
        }

        EditorApplication.LockReloadAssemblies();
        AssetDatabase.DisallowAutoRefresh();
        AssetDatabase.StartAssetEditing();
        try
        {
            // 生成两个新的比赛节点
            for (int i = 1; i <= 2; i++)
            {
                string assetName = $"比赛节点{i}.asset";
                string assetPath = Path.Combine(BaseDir, assetName).Replace("\\", "/");

                // 若已存在则更新，不存在则克隆创建
                SceneNodeConfig target = AssetDatabase.LoadAssetAtPath<SceneNodeConfig>(assetPath);
                if (target == null)
                {
                    // 克隆模板
                    target = Object.Instantiate(template);
                    // 重置唯一ID，让 OnValidate 自动生成
                    target.唯一ID = string.Empty;
                    // 设置显示名
                    target.名字 = $"比赛节点{i}";
                    // 设置节点路径
                    target.场景节点路径 = ScenePathPrefix + $"jump_plat";
                    // 设置场景类型为飞行比赛
                    target.场景类型 = SceneNodeType.飞行比赛;
                    // 设置所属场景
                    target.所属场景 = TutorialSceneType.init_map;
                    // 设置区域节点配置
                    target.区域节点配置.包围盒节点 = $"race{i}_TriggerBox";
                    target.区域节点配置.复活节点 = "BasicRespawn";
                    target.区域节点配置.传送节点 = "TeleportNode";
                    target.区域节点配置.导航节点 = "navNode";
                    target.区域节点配置.倒计时显示节点 = $"countdown{i}_Display";
                    target.区域节点配置.比赛场景 = $"Ground/init_map/terrain/Scene/race_track_{i}";
                    // 设置玩法规则
                    target.玩法规则.比赛时长 = 60;
                    target.玩法规则.准备时间 = 10;
                    target.玩法规则.胜利条件 = 0;

                    AssetDatabase.CreateAsset(target, assetPath);
                    EditorUtility.SetDirty(target);
                    Debug.Log($"已创建: {assetPath}");
                }
                else
                {
                    // 更新已有
                    target.名字 = $"比赛节点{i}";
                    target.场景节点路径 = ScenePathPrefix + $"race_node_{i}";
                    target.场景类型 = SceneNodeType.飞行比赛;
                    target.所属场景 = TutorialSceneType.init_map;
                    target.区域节点配置.包围盒节点 = $"race{i}_TriggerBox";
                    target.区域节点配置.复活节点 = "BasicRespawn";
                    target.区域节点配置.传送节点 = "TeleportNode";
                    target.区域节点配置.导航节点 = "navNode";
                    target.区域节点配置.倒计时显示节点 = $"countdown{i}_Display";
                    target.区域节点配置.比赛场景 = $"Ground/init_map/terrain/Scene/race_track_{i}";
                    target.玩法规则.比赛时长 = 60;
                    target.玩法规则.准备时间 = 10;
                    target.玩法规则.胜利条件 = 0;
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

        Debug.Log("批量生成完成: 比赛节点(1-2)");
    }
}
