using UnityEditor;
using UnityEngine;
using MiGame.Pet;
using System.Collections.Generic;

/// <summary>
/// 翅膀升星材料批量配置器
/// 自动为所有翅膀配置2-5星的升星消耗材料
/// </summary>
public class WingStarMaterialSetter
{
    [MenuItem("Tools/一键配置翅膀升星材料")]
    public static void ConfigureWingStarMaterials()
    {
        // 查找项目中的所有WingConfig资源
        string[] guids = AssetDatabase.FindAssets("t:WingConfig");
        if (guids.Length == 0)
        {
            Debug.Log("项目中未找到任何翅膀配置（WingConfig）。");
            return;
        }

        Debug.Log($"找到 {guids.Length} 个翅膀配置，开始处理...");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WingConfig wing = AssetDatabase.LoadAssetAtPath<WingConfig>(path);

            if (wing != null)
            {
                // 清空旧的升星消耗列表
                wing.升星消耗列表 = new List<升星消耗>();

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
                        消耗类型 = 消耗类型.翅膀,
                        消耗名称 = wing, // 消耗翅膀自身
                        消耗星级 = starLevel - 1, // 需要的星级是目标星级减1
                        需要数量 = 3
                    };

                    starUpCost.消耗材料.Add(material);
                    wing.升星消耗列表.Add(starUpCost);
                }

                // 标记资源为已修改状态，以便保存
                EditorUtility.SetDirty(wing);
                Debug.Log($"已为翅膀 '{wing.name}' 自动配置2-5星的升星材料。");
            }
        }

        // 保存所有修改过的资源
        AssetDatabase.SaveAssets();
        // 使用延迟调用来避免在资源导入期间刷新
        EditorApplication.delayCall += () => {
            if (!AssetDatabase.IsAssetImportWorkerProcess())
            {
                AssetDatabase.Refresh();
            }
        };

        Debug.Log("所有翅膀的升星材料配置完成！");
    }
} 