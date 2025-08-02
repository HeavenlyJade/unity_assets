using UnityEngine;
using UnityEditor;
using MiGame.Pet;
using MiGame.Items;
using System.Collections.Generic;

public class BatchUpdateWingEffects : EditorWindow
{
    [MenuItem("Tools/批量标准化翅膀携带效果")]
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

        // 2. 查找项目中所有的翅膀配置文件
        string[] guids = AssetDatabase.FindAssets("t:WingConfig");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "项目中未找到任何翅膀(WingConfig)配置文件。", "确定");
            return;
        }

        int updatedWings = 0;

        // 3. 遍历所有找到的翅膀
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WingConfig wing = AssetDatabase.LoadAssetAtPath<WingConfig>(path);

            if (wing == null) continue;

            // 备份原始效果列表，用于获取效果数值
            var originalEffects = new List<携带效果>(wing.携带效果);
            
            // 创建新的效果列表
            var newEffects = new List<携带效果>();

            // --- 创建效果1: 移动加成 ---
            var moveEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_移动加成",
                加成类型 = 加成类型.玩家属性,
                物品目标 = null,
                目标变量 = "数据_固定值_移动速度",
                作用类型 = "单独相加",
                // 尝试从原始列表的第一项继承效果数值
                效果数值 = (originalEffects.Count > 0 && originalEffects[0] != null) ? originalEffects[0].效果数值 : ""
            };
            newEffects.Add(moveEffect);

            // --- 创建效果2: 金币加成 ---
            var goldEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_金币加成",
                加成类型 = 加成类型.物品,
                物品目标 = goldItem,
                // 尝试从原始列表的第二项继承效果数值
                效果数值 = (originalEffects.Count > 1 && originalEffects[1] != null) ? originalEffects[1].效果数值 : ""
            };
            newEffects.Add(goldEffect);

            // --- 创建效果3: 加速度 ---
            var accelerationEffect = new 携带效果
            {
                变量类型 = 变量类型.玩家变量,
                变量名称 = "加成_百分比_加速度",
                加成类型 = 加成类型.玩家属性,
                物品目标 = null,
                目标变量 = "数据_固定值_加速度",
                作用类型 = "单独相加",
                // 尝试从原始列表的第三项继承效果数值
                效果数值 = (originalEffects.Count > 2 && originalEffects[2] != null) ? originalEffects[2].效果数值 : ""
            };
            newEffects.Add(accelerationEffect);

            // 替换掉原来的列表
            wing.携带效果 = newEffects;
            
            // 标记为已修改
            EditorUtility.SetDirty(wing);
            updatedWings++;
        }

        // 保存所有更改
        AssetDatabase.SaveAssets();
        
        // 显示结果
        string summary = $"操作完成！\n\n- 共找到并标准化了 {updatedWings} 个翅膀的携带效果。";
        EditorUtility.DisplayDialog("批量更新成功", summary, "确定");
        
        AssetDatabase.Refresh();
    }
} 