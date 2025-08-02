using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using MiGame.Items;
using System.Collections.Generic;

public class BatchUpdatePetEffects : EditorWindow
{
    [MenuItem("Tools/批量标准化宠物携带效果")]
    public static void BatchUpdate()
    {
        // 1. 查找项目中所有的宠物配置文件
        string[] guids = AssetDatabase.FindAssets("t:PetConfig");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "项目中未找到任何宠物(PetConfig)配置文件。", "确定");
            return;
        }

        int updatedPets = 0;

        // 2. 遍历所有找到的宠物
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PetConfig pet = AssetDatabase.LoadAssetAtPath<PetConfig>(path);

            if (pet == null) continue;

            // 备份原始效果列表
            var originalEffects = new List<携带效果>(pet.携带效果);
            
            // 创建新的效果列表，先保留原有的所有效果
            var newEffects = new List<携带效果>(originalEffects);

            // --- 新增效果: 战力值加成 ---
            var powerEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_训练加成",
                加成类型 = 加成类型.玩家变量,
                物品目标 = null,
                目标变量 = "数据_固定值_战力值",
                作用类型 = "单独相加",
                // 尝试从原始列表的第一项继承效果数值
                效果数值 = (originalEffects.Count > 0 && originalEffects[0] != null) ? originalEffects[0].效果数值 : ""
            };
            newEffects.Add(powerEffect);

            // 替换掉原来的列表
            pet.携带效果 = newEffects;
            
            // 标记为已修改
            EditorUtility.SetDirty(pet);
            updatedPets++;
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        
        // 显示结果
        string summary = $"操作完成！\n\n- 共找到并标准化了 {updatedPets} 个宠物的携带效果。\n- 为每个宠物新增了战力值加成效果。";
        EditorUtility.DisplayDialog("批量更新成功", summary, "确定");
        
        AssetDatabase.Refresh();
    }
} 