using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using MiGame.Items;
using System.Collections.Generic;

public class BatchRemoveFirstPetEffect : EditorWindow
{
    [MenuItem("Tools/批量删除宠物第一个携带效果")]
    public static void BatchRemove()
    {
        // 1. 查找项目中所有的宠物配置文件
        string[] guids = AssetDatabase.FindAssets("t:PetConfig");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "项目中未找到任何宠物(PetConfig)配置文件。", "确定");
            return;
        }

        int updatedPets = 0;
        int removedEffects = 0;

        // 2. 遍历所有找到的宠物
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PetConfig pet = AssetDatabase.LoadAssetAtPath<PetConfig>(path);

            if (pet == null) continue;

            // 检查是否有携带效果
            if (pet.携带效果 == null || pet.携带效果.Count == 0)
            {
                continue; // 跳过没有携带效果的宠物
            }

            // 备份原始效果列表
            var originalEffects = new List<携带效果>(pet.携带效果);
            
            // 创建新的效果列表，移除第一个元素
            var newEffects = new List<携带效果>();
            
            // 从第二个元素开始复制（跳过第一个元素）
            for (int i = 1; i < originalEffects.Count; i++)
            {
                newEffects.Add(originalEffects[i]);
            }

            // 替换掉原来的列表
            pet.携带效果 = newEffects;
            
            // 标记为已修改
            EditorUtility.SetDirty(pet);
            updatedPets++;
            removedEffects++;
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        
        // 显示结果
        string summary = $"操作完成！\n\n- 共处理了 {updatedPets} 个宠物配置文件。\n- 删除了 {removedEffects} 个第一个携带效果。";
        EditorUtility.DisplayDialog("批量删除成功", summary, "确定");
        
        AssetDatabase.Refresh();
    }
} 