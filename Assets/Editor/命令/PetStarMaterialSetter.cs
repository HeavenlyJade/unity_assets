using UnityEditor;
using UnityEngine;
using MiGame.Pet;
using System.Collections.Generic;

public class PetStarMaterialSetter
{
    [MenuItem("工具/宠物/一键配置宠物升星材料")]
    public static void ConfigurePetStarMaterials()
    {
        // 查找项目中的所有PetConfig资源
        string[] guids = AssetDatabase.FindAssets("t:PetConfig");
        if (guids.Length == 0)
        {
            Debug.Log("项目中未找到任何宠物配置（PetConfig）。");
            return;
        }

        Debug.Log($"找到 {guids.Length} 个宠物配置，开始处理...");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PetConfig pet = AssetDatabase.LoadAssetAtPath<PetConfig>(path);

            if (pet != null)
            {
                // 清空旧的升星消耗列表
                pet.升星消耗列表 = new List<升星消耗>();

                // 为2至5星配置升星材料
                for (int starLevel = 2; starLevel <= 5; starLevel++)
                {
                    var starUpCost = new 升星消耗
                    {
                        星级 = starLevel,
                        成功率 = 100,
                        消耗材料 = new List<升星材料>()
                    };

                    var material = new 升星材料
                    {
                        消耗类型 = 消耗类型.宠物,
                        消耗名称 = pet, // 消耗宠物自身
                        消耗星级 = starLevel - 1, // 需要的星级是目标星级减1
                        需要数量 = 3
                    };

                    starUpCost.消耗材料.Add(material);
                    pet.升星消耗列表.Add(starUpCost);
                }

                // 标记资源为已修改状态，以便保存
                EditorUtility.SetDirty(pet);
                Debug.Log($"已为宠物 '{pet.name}' 自动配置2-5星的升星材料。");
            }
        }

        // 保存所有修改过的资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("所有宠物的升星材料配置完成！");
    }
} 