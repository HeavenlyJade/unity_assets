using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MiGame.Pet;

public static class PetEffectBatchUpdater
{
    /// <summary>
    /// 从宠物JSON读取“加成_百分比_训练加成”，批量更新 GameConf/宠物 下对应资源的携带效果数值。
    /// 匹配规则：优先按 路径 Assets/GameConf/宠物/{品级}/{宠物名称}.asset 加载；失败则扫描全局同名 PetConfig。
    /// 当资源中不存在携带效果时，会创建一个默认项，变量名称为“训练加成”，作用类型为“单独相加”。
    /// </summary>
    [MenuItem("Tools/宠物/批量更新训练加成效果数值")] 
    public static void UpdatePetTrainingBonusFromJson()
    {
        try
        {
            string jsonPath = Path.Combine(Application.dataPath, "Scripts/配置exel/宠物.json");
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"未找到宠物配置JSON文件: {jsonPath}");
                return;
            }

            string jsonText = File.ReadAllText(jsonPath);
            var jsonList = JsonUtilityWrapper.FromJsonArray<PetJsonData>(jsonText);
            if (jsonList == null || jsonList.Count == 0)
            {
                Debug.LogError("宠物配置JSON解析失败或数据为空");
                return;
            }

            int updateCount = 0;
            int notFoundCount = 0;
            int skippedCount = 0;
            List<string> notFoundNames = new List<string>();

            foreach (var item in jsonList)
            {
                if (string.IsNullOrEmpty(item.宠物名称))
                {
                    skippedCount++;
                    continue;
                }

                // 从JSON读取效果数值
                string effectValue = item.加成_百分比_训练加成;
                if (string.IsNullOrEmpty(effectValue))
                {
                    // 没有配置则跳过，不清空既有配置
                    skippedCount++;
                    continue;
                }

                // 尝试按品级路径直接定位
                string directPath = BuildPetAssetPath(item.品级, item.宠物名称);
                PetConfig pet = AssetDatabase.LoadAssetAtPath<PetConfig>(directPath);
                if (pet == null)
                {
                    // 退化为全局查找同名 PetConfig
                    pet = FindPetByName(item.宠物名称);
                }

                if (pet == null)
                {
                    notFoundCount++;
                    notFoundNames.Add(item.宠物名称);
                    continue;
                }

                // 更新（或创建）携带效果里“训练加成”的效果数值
                bool changed = ApplyTrainingBonusEffect(pet, effectValue);
                if (changed)
                {
                    EditorUtility.SetDirty(pet);
                    updateCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            if (updateCount > 0)
            {
                AssetDatabase.SaveAssets();
            }
            // 避免导入期间刷新问题，使用延迟调用
            EditorApplication.delayCall += () =>
            {
                if (!AssetDatabase.IsAssetImportWorkerProcess())
                {
                    AssetDatabase.Refresh();
                }
            };

            Debug.Log($"宠物训练加成批量更新完成：成功 {updateCount} 项，跳过 {skippedCount} 项，未找到 {notFoundCount} 项");
            if (notFoundNames.Count > 0)
            {
                Debug.LogWarning("未找到的宠物：" + string.Join("，", notFoundNames));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"批量更新出现异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static string BuildPetAssetPath(string quality, string name)
    {
        // 直接按原样放入品级子目录，保持与现有目录一致（N/R/SR/SSR/UR）
        // 若 quality 为空，尝试在根目录查找
        if (string.IsNullOrEmpty(quality))
        {
            return $"Assets/GameConf/宠物/{name}.asset";
        }
        return $"Assets/GameConf/宠物/{quality}/{name}.asset";
    }

    private static PetConfig FindPetByName(string name)
    {
        // 全局查找所有 PetConfig 资源，匹配 ScriptableObject 字段“宠物名称”或资源名
        string[] guids = AssetDatabase.FindAssets("t:MiGame.Pet.PetConfig");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var pet = AssetDatabase.LoadAssetAtPath<PetConfig>(path);
            if (pet == null) continue;
            if (pet.宠物名称 == name || pet.name == name)
            {
                return pet;
            }
        }
        return null;
    }

    private static bool ApplyTrainingBonusEffect(PetConfig pet, string effectValue)
    {
        if (pet.携带效果 == null)
        {
            pet.携带效果 = new List<携带效果>();
        }

        // 优先更新变量名称为“训练加成”的效果
        var effect = pet.携带效果.FirstOrDefault(e => e != null && e.变量名称 == "训练加成");
        if (effect != null)
        {
            if (effect.效果数值 == effectValue)
            {
                return false; // 无变化
            }
            effect.效果数值 = effectValue;
            return true;
        }

        // 若没有“训练加成”，且存在单个效果，则更新第一个效果的数值（保守更新）
        if (pet.携带效果.Count == 1 && pet.携带效果[0] != null)
        {
            var first = pet.携带效果[0];
            if (first.效果数值 == effectValue)
            {
                return false;
            }
            first.效果数值 = effectValue;
            return true;
        }

        // 否则新增一个“训练加成”效果项
        var newEffect = new 携带效果
        {
            变量类型 = 变量类型.玩家属性,
            变量名称 = "训练加成",
            效果数值 = effectValue,
            加成类型 = 加成类型.玩家属性,
            目标变量 = string.Empty,
            作用类型 = "单独相加"
        };
        pet.携带效果.Add(newEffect);
        return true;
    }

    [Serializable]
    private class PetJsonData
    {
        // 与 Scripts/配置exel/宠物.json 字段对齐
        public string 宠物名称;
        public string 抽奖区域;
        public string 品级;
        public string 图片;
        public string 图片资源;
        public string 模型;
        public string 动画;
        public string 加成_百分比_训练加成;
    }
}


