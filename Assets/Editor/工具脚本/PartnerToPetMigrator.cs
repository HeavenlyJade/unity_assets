using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using System.IO;
using System.Collections.Generic;

public class PartnerToPetMigrator : EditorWindow
{
    [MenuItem("Tools/迁移伙伴到宠物")]
    public static void MigratePartnersToPets()
    {
        // 定义要迁移的伙伴名称列表
        List<string> partnerNames = new List<string>
        {
            "东海帝王",
            "丸善斯基", 
            "无声铃鹿",
            "曼波",
            "黄金船"
        };

        // 确保目标目录存在
        string targetDir = "Assets/GameConf/宠物/UR";
        if (!AssetDatabase.IsValidFolder(targetDir))
        {
            AssetDatabase.CreateFolder("Assets/GameConf/宠物", "UR");
        }

        int migratedCount = 0;
        List<string> errors = new List<string>();

        foreach (string partnerName in partnerNames)
        {
            try
            {
                // 源文件路径
                string sourcePath = $"Assets/GameConf/伙伴/UR/{partnerName}.asset";
                
                // 检查源文件是否存在
                if (!File.Exists(Path.Combine(Application.dataPath, sourcePath.Replace("Assets/", ""))))
                {
                    errors.Add($"源文件不存在: {sourcePath}");
                    continue;
                }

                // 加载源伙伴配置
                PartnerConfig sourcePartner = AssetDatabase.LoadAssetAtPath<PartnerConfig>(sourcePath);
                if (sourcePartner == null)
                {
                    errors.Add($"无法加载伙伴配置: {sourcePath}");
                    continue;
                }

                // 创建新的宠物配置
                PetConfig newPet = ScriptableObject.CreateInstance<PetConfig>();

                // 复制所有基础属性
                CopyBasePetConfig(sourcePartner, newPet);

                // 目标文件路径
                string targetPath = $"{targetDir}/{partnerName}.asset";

                // 创建新文件
                AssetDatabase.CreateAsset(newPet, targetPath);

                // 删除原文件
                AssetDatabase.DeleteAsset(sourcePath);

                migratedCount++;
                Debug.Log($"成功迁移: {partnerName} 从伙伴到宠物");
            }
            catch (System.Exception e)
            {
                errors.Add($"迁移 {partnerName} 时出错: {e.Message}");
            }
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 显示结果
        string resultMessage = $"迁移完成！\n\n成功迁移: {migratedCount} 个文件";
        if (errors.Count > 0)
        {
            resultMessage += $"\n\n错误 ({errors.Count} 个):\n" + string.Join("\n", errors);
        }

        EditorUtility.DisplayDialog("迁移结果", resultMessage, "确定");
    }

    private static void CopyBasePetConfig(BasePetConfig source, BasePetConfig target)
    {
        // 复制所有基础属性
        target.宠物名称 = source.宠物名称;
        target.宠物描述 = source.宠物描述;
        target.稀有度 = source.稀有度;
        target.初始等级 = source.初始等级;
        target.最大等级 = source.最大等级;
        target.元素类型 = source.元素类型;
        target.基础属性列表 = new List<属性配置>(source.基础属性列表);
        target.成长率列表 = new List<成长率配置>(source.成长率列表);
        target.升星消耗列表 = new List<升星消耗>(source.升星消耗列表);
        target.携带效果 = new List<携带效果>(source.携带效果);
        target.技能列表 = new List<string>(source.技能列表);
        target.进化条件 = source.进化条件;
        target.进化后形态 = source.进化后形态;
        target.获取方式 = new List<string>(source.获取方式);
        target.模型资源 = source.模型资源;
        target.头像资源 = source.头像资源;
        target.动画资源 = source.动画资源;
        target.音效资源 = source.音效资源;
        target.特殊标签 = new List<string>(source.特殊标签);
    }
} 