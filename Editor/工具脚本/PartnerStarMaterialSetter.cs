using UnityEditor;
using UnityEngine;
using MiGame.Pet;
using System.Collections.Generic;

public class PartnerStarMaterialSetter
{
    [MenuItem("工具/伙伴/一键配置伙伴升星材料")]
    public static void ConfigurePartnerStarMaterials()
    {
        // 查找项目中的所有PartnerConfig资源
        string[] guids = AssetDatabase.FindAssets("t:PartnerConfig");
        if (guids.Length == 0)
        {
            Debug.Log("项目中未找到任何伙伴配置（PartnerConfig）。");
            return;
        }

        Debug.Log($"找到 {guids.Length} 个伙伴配置，开始处理...");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PartnerConfig partner = AssetDatabase.LoadAssetAtPath<PartnerConfig>(path);

            if (partner != null)
            {
                // 清空旧的升星消耗列表
                partner.升星消耗列表 = new List<升星消耗>();

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
                        消耗类型 = 消耗类型.伙伴,
                        消耗名称 = partner, // 消耗伙伴自身
                        消耗星级 = starLevel - 1, // 需要的星级是目标星级减1
                        需要数量 = 3
                    };

                    starUpCost.消耗材料.Add(material);
                    partner.升星消耗列表.Add(starUpCost);
                }

                // 标记资源为已修改状态，以便保存
                EditorUtility.SetDirty(partner);
                Debug.Log($"已为伙伴 '{partner.name}' 自动配置2-5星的升星材料。");
            }
        }

        // 保存所有修改过的资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("所有伙伴的升星材料配置完成！");
    }
} 