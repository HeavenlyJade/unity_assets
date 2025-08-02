using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using MiGame.Items;
using System.Collections.Generic;

public class BatchUpdatePartnerEffects : EditorWindow
{
    [MenuItem("Tools/批量标准化伙伴携带效果")]
    public static void BatchUpdate()
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
            
            // 标记为已修改
            EditorUtility.SetDirty(partner);
            updatedPartners++;
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        
        // 显示结果
        string summary = $"操作完成！\n\n- 共找到并标准化了 {updatedPartners} 个伙伴的携带效果。";
        EditorUtility.DisplayDialog("批量更新成功", summary, "确定");
        
        AssetDatabase.Refresh();
    }
}
