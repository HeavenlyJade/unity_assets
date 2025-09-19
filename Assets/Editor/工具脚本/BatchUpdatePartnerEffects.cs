using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using MiGame.Items;
using System.Collections.Generic;
using System.Linq;

public class BatchUpdatePartnerEffects : EditorWindow
{
    private bool updateAllPartners = false;  // 默认不选择更新所有
    private bool updateByDirectory = true;   // 默认选择按目录更新
    
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

    [MenuItem("Tools/批量标准化伙伴携带效果")]
    public static void ShowWindow()
    {
        GetWindow<BatchUpdatePartnerEffects>("批量标准化伙伴携带效果");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量标准化伙伴携带效果", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("标准化伙伴的携带效果配置，包括金币加成和训练加成。", MessageType.Info);

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
                
                EditorGUILayout.HelpBox($"将标准化目录 \"{predefinedDirectories[selectedDirectoryIndex]}\" 下的所有伙伴携带效果", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("将标准化所有伙伴的携带效果", MessageType.Info);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("标准化携带效果", GUILayout.Height(30)))
        {
            if (updateAllPartners)
            {
                BatchUpdateAllPartners();
            }
            else if (updateByDirectory)
            {
                BatchUpdatePartnersInDirectory();
            }
        }
    }

    private void BatchUpdateAllPartners()
    {
        try
        {
            // 1. 定义并加载金币配置文件
            string goldItemPath = "Assets/GameConf/物品/货币/金币.asset";
            ItemType goldItem = AssetDatabase.LoadAssetAtPath<ItemType>(goldItemPath);

            if (goldItem == null)
            {
                EditorUtility.DisplayDialog("错误", $"未能加载金币配置文件，请检查路径是否正确: {goldItemPath}", "确定");
                return;
            }

            // 2. 查找项目中所有的伙伴配置文件
            string[] guids = AssetDatabase.FindAssets("t:PartnerConfig");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "项目中未找到任何伙伴(PartnerConfig)配置文件。", "确定");
                return;
            }

            int updatedPartners = 0;

            // 3. 遍历所有找到的伙伴
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PartnerConfig partner = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);

                if (partner == null) continue;

                if (UpdatePartnerEffects(partner, goldItem))
                {
                    updatedPartners++;
                }
            }

            // 保存所有更改
            AssetDatabase.SaveAssets();
            
            // 显示结果
            EditorUtility.DisplayDialog("操作完成", $"批量标准化所有伙伴携带效果完成！\n\n成功标准化 {updatedPartners} 个伙伴。", "确定");
            
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"批量更新出现异常: {ex.Message}", "确定");
        }
    }

    private void BatchUpdatePartnersInDirectory()
    {
        try
        {
            string targetDirectory = predefinedDirectories[selectedDirectoryIndex];
            string fullPath = "Assets/" + targetDirectory;
            
            // 1. 定义并加载金币配置文件
            string goldItemPath = "Assets/GameConf/物品/货币/金币.asset";
            ItemType goldItem = AssetDatabase.LoadAssetAtPath<ItemType>(goldItemPath);

            if (goldItem == null)
            {
                EditorUtility.DisplayDialog("错误", $"未能加载金币配置文件，请检查路径是否正确: {goldItemPath}", "确定");
                return;
            }

            // 2. 查找指定目录下的伙伴配置文件
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

            int updatedPartners = 0;
            foreach (var partner in directoryPartnerConfigs)
            {
                if (UpdatePartnerEffects(partner, goldItem))
                {
                    EditorUtility.SetDirty(partner);
                    updatedPartners++;
                }
            }

            if (updatedPartners > 0)
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
                $"批量标准化目录 \"{targetDirectory}\" 下的伙伴携带效果完成！\n\n" +
                $"成功标准化 {updatedPartners} 个伙伴。", "确定");

            Debug.Log($"伙伴携带效果批量标准化完成：成功 {updatedPartners} 项");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"批量更新出现异常: {ex.Message}", "确定");
        }
    }

    /// <summary>
    /// 更新单个伙伴的携带效果
    /// </summary>
    /// <param name="partner">目标伙伴配置</param>
    /// <param name="goldItem">金币物品配置</param>
    /// <returns>是否更新成功</returns>
    private bool UpdatePartnerEffects(PartnerConfig partner, ItemType goldItem)
    {
        try
        {
            // 备份原始效果列表，用于获取效果数值
            var originalEffects = new List<携带效果>(partner.携带效果);
            
            // 创建新的效果列表
            var newEffects = new List<携带效果>();

            // --- 创建效果1: 金币加成 ---
            var goldEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_金币加成",
                加成类型 = 加成类型.物品,
                物品目标 = goldItem,
                // 尝试从原始列表的第一项继承效果数值
                效果数值 = (originalEffects.Count > 0 && originalEffects[0] != null) ? originalEffects[0].效果数值 : ""
            };
            newEffects.Add(goldEffect);

            // --- 创建效果2: 训练加成 ---
            var trainingEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_训练加成",
                加成类型 = 加成类型.玩家变量,
                物品目标 = null,
                目标变量 = "数据_固定值_战力值",
                作用类型 = "单独相加",
                // 尝试从原始列表的第二项继承效果数值
                效果数值 = (originalEffects.Count > 1 && originalEffects[1] != null) ? originalEffects[1].效果数值 : ""
            };
            newEffects.Add(trainingEffect);

            // 替换掉原来的列表
            partner.携带效果 = newEffects;
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"更新伙伴携带效果 {partner.宠物名称} 时发生错误: {ex.Message}");
            return false;
        }
    }
}
