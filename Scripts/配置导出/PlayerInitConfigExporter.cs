#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
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
                sb.AppendLine($"        ['描述'] = '{config.描述}',");

                // 货币初始化
                sb.AppendLine("        ['货币初始化'] = {");
                foreach (var currency in config.货币初始化)
                {
                    if (currency.货币名称 != null)
                    {
                        sb.AppendLine("            {");
                        sb.AppendLine($"                ['货币名称'] = '{currency.货币名称.name}',");
                        sb.AppendLine($"                ['初始数量'] = {currency.初始数量},");
                        sb.AppendLine("            },");
                    }
                }
                sb.AppendLine("        },");

                // 变量初始化
                sb.AppendLine("        ['变量初始化'] = {");
                foreach (var variable in config.变量初始化)
                {
                    if (!string.IsNullOrEmpty(variable.变量名称))
                    {
                        sb.AppendLine("            {");
                        sb.AppendLine($"                ['变量名称'] = '{variable.变量名称}',");
                        sb.AppendLine($"                ['初始值'] = {variable.初始值},");
                        sb.AppendLine("            },");
                    }
                }
                sb.AppendLine("        },");

                // 其他设置
                sb.AppendLine("        ['其他设置'] = {");
                sb.AppendLine($"            ['是否新手'] = {config.其他设置.是否新手.ToString().ToLower()},");
                sb.AppendLine($"            ['初始等级'] = {config.其他设置.初始等级},");
                sb.AppendLine("        },");

                sb.AppendLine("    },");
            }

            sb.AppendLine("}");
            sb.AppendLine("-- --- 自动生成配置结束 ---");
            sb.AppendLine();
            sb.AppendLine("return PlayerInitConfig");
        }
    }
}
#endif
