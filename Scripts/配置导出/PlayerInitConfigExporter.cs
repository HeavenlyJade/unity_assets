#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MiGame.Data
{
    public static class PlayerInitConfigExporter
    {
        private const string LUA_CONFIG_PATH_DEFAULT = "Assets/Lua/MiGame/Data/PlayerInitConfig.lua";

public static void ExportToPath(string exportPath)
        {
            var allConfigs = new List<PlayerInitConfig>();
            string[] guids = AssetDatabase.FindAssets("t:PlayerInitConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<PlayerInitConfig>(path);
                if (config != null)
                {
                    allConfigs.Add(config);
                }
            }

            if (allConfigs.Count == 0)
            {
                Debug.Log("未找到任何玩家初始化配置 (PlayerInitConfig) 文件，无需导出。");
                return;
            }

            StringBuilder luaBuilder = new StringBuilder();
            BuildLuaContent(luaBuilder, allConfigs);

            // 确保目录存在
            string directory = Path.GetDirectoryName(exportPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(exportPath, luaBuilder.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"成功导出 {allConfigs.Count} 个玩家初始化配置到 {exportPath}");
            EditorUtility.DisplayDialog("导出成功", $"成功导出 {allConfigs.Count} 个配置到:\n{exportPath}", "确定");
        }

        private static void BuildLuaContent(StringBuilder sb, List<PlayerInitConfig> configs)
        {
            sb.AppendLine("-- PlayerInitConfig.lua");
            sb.AppendLine("-- 自动生成的玩家初始化配置。自定义代码将被覆盖。");
            sb.AppendLine();
            sb.AppendLine("---@class PlayerInitConfig");
            sb.AppendLine("---@field Data table<string, table>");
            sb.AppendLine();
            sb.AppendLine("---@type PlayerInitConfig");
            sb.AppendLine("local PlayerInitConfig = {Data = {}}");
            sb.AppendLine();
            sb.AppendLine("-- --- 自动生成配置开始 ---");
            sb.AppendLine("PlayerInitConfig.Data = {");

            foreach (var config in configs)
            {
                string configName = config.name; // 使用文件名作为配置名
                sb.AppendLine($"    ['{configName}'] = {{");
                sb.AppendLine($"        ['配置名称'] = '{configName}',");

                // 使用反射获取所有公共字段
                FieldInfo[] fields = typeof(PlayerInitConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    if (field.Name == "name") continue; // 跳过Unity内置的name字段
                    
                    var value = field.GetValue(config);
                    if (value != null)
                    {
                        ExportFieldToLua(sb, field.Name, value, 2);
                    }
                }

                sb.AppendLine("    },");
            }

            sb.AppendLine("}");
            sb.AppendLine("-- --- 自动生成配置结束 ---");
            sb.AppendLine();
            sb.AppendLine("return PlayerInitConfig");
        }

        /// <summary>
        /// 将字段值导出为Lua格式
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">字段值</param>
        /// <param name="indentLevel">缩进级别</param>
        private static void ExportFieldToLua(StringBuilder sb, string fieldName, object value, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            
            if (value == null)
                return;

            // 处理不同类型的值
            if (value is string stringValue)
            {
                sb.AppendLine($"{indent}['{fieldName}'] = '{stringValue}',");
            }
            else if (value is bool boolValue)
            {
                sb.AppendLine($"{indent}['{fieldName}'] = {boolValue.ToString().ToLower()},");
            }
            else if (value is int || value is float || value is double)
            {
                sb.AppendLine($"{indent}['{fieldName}'] = {value},");
            }
            else if (value is IList list)
            {
                sb.AppendLine($"{indent}['{fieldName}'] = {{");
                
                foreach (var item in list)
                {
                    if (item != null)
                    {
                        // 如果是基础类型（如字符串），直接导出值
                        if (item is string stringItem)
                        {
                            // 只导出非空字符串
                            if (!string.IsNullOrEmpty(stringItem))
                            {
                                sb.AppendLine($"{indent}    '{stringItem}',");
                            }
                        }
                        else if (item is int || item is float || item is double)
                        {
                            sb.AppendLine($"{indent}    {item},");
                        }
                        else if (item is bool boolItem)
                        {
                            sb.AppendLine($"{indent}    {boolItem.ToString().ToLower()},");
                        }
                        else
                        {
                            // 复杂对象，使用原来的方法
                            ExportObjectToLua(sb, item, indentLevel + 1);
                        }
                    }
                }
                
                sb.AppendLine($"{indent}}},");
            }
            else if (value.GetType().IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(value.GetType()))
            {
                // 处理自定义类（如OtherSettings）
                sb.AppendLine($"{indent}['{fieldName}'] = {{");
                
                FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(value);
                    if (fieldValue != null)
                    {
                        ExportFieldToLua(sb, field.Name, fieldValue, indentLevel + 1);
                    }
                }
                
                sb.AppendLine($"{indent}}},");
            }
        }

        /// <summary>
        /// 将对象导出为Lua表格式
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="obj">对象</param>
        /// <param name="indentLevel">缩进级别</param>
        private static void ExportObjectToLua(StringBuilder sb, object obj, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            
            sb.AppendLine($"{indent}{{");
            
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(obj);
                if (value != null)
                {
                    // 特殊处理ScriptableObject引用
                    if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                    {
                        var scriptableObj = value as ScriptableObject;
                        if (scriptableObj != null)
                        {
                            sb.AppendLine($"{indent}    ['{field.Name}'] = '{scriptableObj.name}',");
                        }
                    }
                    else
                    {
                        ExportFieldToLua(sb, field.Name, value, indentLevel + 1);
                    }
                }
            }
            
            sb.AppendLine($"{indent}}},");
        }
    }
}
#endif
